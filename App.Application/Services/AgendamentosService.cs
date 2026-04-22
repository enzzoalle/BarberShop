using App.Domain.DTO;
using App.Domain.Entities;
using App.Domain.Enums;
using App.Domain.Interfaces;
using App.Domain.Interfaces.Repository;

namespace App.Application.Services;

public class AgendamentosService : IAgendamentosService
{
    private static readonly TimeSpan IntervaloMinimoEntreHorarios = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan HorarioAberturaPadrao = TimeSpan.FromHours(9);
    private static readonly TimeSpan HorarioFechamentoPadrao = TimeSpan.FromHours(18);

    private readonly IRepositoryBase<Agendamentos> _agendamentoRepository;
    private readonly IRepositoryBase<Servicos> _servicoRepository;
    private readonly IRepositoryBase<Clientes> _clienteRepository;
    private readonly IRepositoryBase<Parametros> _parametrosRepository;
    private readonly IRepositoryBase<FolgasFeriados> _folgaFeriadoRepository;

    public AgendamentosService(
        IRepositoryBase<Agendamentos> agendamentoRepository,
        IRepositoryBase<Servicos> servicoRepository,
        IRepositoryBase<Clientes> clienteRepository,
        IRepositoryBase<Parametros> parametrosRepository,
        IRepositoryBase<FolgasFeriados> folgaFeriadoRepository)
    {
        _agendamentoRepository = agendamentoRepository;
        _servicoRepository = servicoRepository;
        _clienteRepository = clienteRepository;
        _parametrosRepository = parametrosRepository;
        _folgaFeriadoRepository = folgaFeriadoRepository;
    }

    public IEnumerable<Agendamentos> Listar()
    {
        return _agendamentoRepository
            .Query(x => true)
            .Select(x => new Agendamentos
            {
                Id = x.Id,
                DataAgendamento = x.DataAgendamento,
                HorarioAgendamento = x.HorarioAgendamento,
                Observacao = x.Observacao,
                StatusAgendamento = x.StatusAgendamento,
                FoiPago = x.FoiPago,
                DataCriacao = x.DataCriacao,
                Clientes = new Clientes
                {
                    Id = x.Clientes.Id,
                    Nome = x.Clientes.Nome,
                    NumeroTelefone = x.Clientes.NumeroTelefone,
                    DataCriacao = x.Clientes.DataCriacao,
                    UsuarioId = x.Clientes.UsuarioId
                },
                Servicos = new Servicos
                {
                    Id = x.Servicos.Id,
                    Nome = x.Servicos.Nome,
                    Duracao = x.Servicos.Duracao,
                    Valor = x.Servicos.Valor,
                    Ativo = x.Servicos.Ativo,
                    DataCriacao = x.Servicos.DataCriacao
                }
            })
            .OrderByDescending(x => x.DataAgendamento)
            .ThenBy(x => x.HorarioAgendamento)
            .ToList();
    }

    public IEnumerable<string> ListarHorariosDisponiveis(DateTime data, int servicoId)
    {
        var servico = _servicoRepository.FindById(servicoId);
        var expediente = ObterExpediente();

        if (!DiaDisponivelParaAgendamento(data, expediente.DiasFuncionamento) || DataEstaBloqueada(data))
        {
            return [];
        }

        var dataSelecionada = data.Date;

        var agendamentosDoDia = _agendamentoRepository
            .Query(x => x.DataAgendamento.Date == dataSelecionada && x.StatusAgendamento != StatusAgendamentoEnum.Cancelado)
            .Select(x => new
            {
                Inicio = x.HorarioAgendamento,
                Duracao = x.Servicos.Duracao
            })
            .ToList();

        var horariosDisponiveis = new List<string>();
        var horarioAtual = expediente.Abertura;

        while (horarioAtual + servico.Duracao <= expediente.Fechamento)
        {
            var horarioFinal = horarioAtual + servico.Duracao;
            var conflitaComAgendamento = agendamentosDoDia.Any(x => horarioAtual < x.Inicio + x.Duracao && horarioFinal > x.Inicio);

            if (!conflitaComAgendamento)
            {
                horariosDisponiveis.Add(horarioAtual.ToString(@"hh\:mm"));
            }

            horarioAtual += IntervaloMinimoEntreHorarios;
        }

        return horariosDisponiveis;
    }

    public void Incluir(CriarAgendamentoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.NumeroTelefoneCliente))
        {
            throw new InvalidOperationException("Informe o telefone do cliente.");
        }

        IncluirInterno(new CriarAgendamentoManualRequest
        {
            NomeCliente = request.NomeCliente,
            NumeroTelefoneCliente = request.NumeroTelefoneCliente,
            ServicoId = request.ServicoId,
            DataAgendamento = request.DataAgendamento,
            HorarioAgendamento = request.HorarioAgendamento,
            Observacao = request.Observacao
        });
    }

    public void IncluirManual(CriarAgendamentoManualRequest request)
    {
        IncluirInterno(request);
    }

    private (TimeSpan Abertura, TimeSpan Fechamento, DiasFuncionamentoEnum DiasFuncionamento) ObterExpediente()
    {
        var parametros = _parametrosRepository.GetAll().FirstOrDefault();
        if (parametros is null)
        {
            return (HorarioAberturaPadrao, HorarioFechamentoPadrao, DiasFuncionamentoEnum.AteSabado);
        }

        return (
            parametros.HorarioAbertura,
            parametros.HorarioFechamento ?? HorarioFechamentoPadrao,
            parametros.DiasFuncionamento);
    }

    private static bool DiaDisponivelParaAgendamento(DateTime data, DiasFuncionamentoEnum diasFuncionamento)
    {
        return diasFuncionamento switch
        {
            DiasFuncionamentoEnum.DiasUteis => data.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday,
            DiasFuncionamentoEnum.AteSabado => data.DayOfWeek is not DayOfWeek.Sunday,
            DiasFuncionamentoEnum.TodoDia => true,
            _ => false
        };
    }

    private void IncluirInterno(CriarAgendamentoManualRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.NomeCliente))
        {
            throw new InvalidOperationException("Informe o nome do cliente.");
        }

        if (DataEstaBloqueada(request.DataAgendamento))
        {
            throw new InvalidOperationException("A data selecionada está bloqueada por folga ou feriado.");
        }

        var servico = _servicoRepository.FindById(request.ServicoId);
        var horariosDisponiveis = ListarHorariosDisponiveis(request.DataAgendamento, request.ServicoId);
        var horarioSolicitado = request.HorarioAgendamento.ToString(@"hh\:mm");

        if (!horariosDisponiveis.Contains(horarioSolicitado))
        {
            throw new InvalidOperationException("O horário selecionado não está mais disponível.");
        }

        var numeroTelefone = NormalizarTelefone(request.NumeroTelefoneCliente);

        Clientes? clienteExistente = null;
        if (!string.IsNullOrWhiteSpace(numeroTelefone))
        {
            clienteExistente = _clienteRepository
                .Query(x => x.NumeroTelefone == numeroTelefone)
                .OrderByDescending(x => x.DataCriacao)
                .FirstOrDefault();
        }

        if (clienteExistente is null)
        {
            clienteExistente = new Clientes
            {
                Nome = request.NomeCliente.Trim(),
                NumeroTelefone = numeroTelefone,
                DataCriacao = DateTime.Now
            };

            _clienteRepository.Insert(clienteExistente);
        }
        else
        {
            clienteExistente.Nome = request.NomeCliente.Trim();
            clienteExistente.NumeroTelefone = numeroTelefone;
            _clienteRepository.Update(clienteExistente);
        }

        var novoAgendamento = new Agendamentos
        {
            Clientes = clienteExistente,
            Servicos = servico,
            DataAgendamento = request.DataAgendamento.Date,
            HorarioAgendamento = request.HorarioAgendamento,
            Observacao = string.IsNullOrWhiteSpace(request.Observacao) ? null : request.Observacao.Trim(),
            StatusAgendamento = StatusAgendamentoEnum.Pendente,
            FoiPago = false,
            DataCriacao = DateTime.Now
        };

        _agendamentoRepository.Insert(novoAgendamento);
    }

    private bool DataEstaBloqueada(DateTime data)
    {
        var dataSelecionada = data.Date;
        var proximoDia = dataSelecionada.AddDays(1);
        return _folgaFeriadoRepository.Query(x => x.Data >= dataSelecionada && x.Data < proximoDia).Any();
    }

    private static string? NormalizarTelefone(string? numeroTelefone)
    {
        if (string.IsNullOrWhiteSpace(numeroTelefone))
        {
            return null;
        }

        var numeros = new string(numeroTelefone.Where(char.IsDigit).ToArray());
        return string.IsNullOrWhiteSpace(numeros) ? null : numeros;
    }
}