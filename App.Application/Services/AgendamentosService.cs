using App.Domain.DTO;
using App.Domain.Entities;
using App.Domain.Enums;
using App.Domain.Interfaces;
using App.Domain.Interfaces.Repository;
using App.Common;
using Microsoft.EntityFrameworkCore;

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

    public IEnumerable<object> Listar()
    {
        return _agendamentoRepository
            .Query(x => true)
            .OrderByDescending(x => x.DataAgendamento)
            .ThenBy(x => x.HorarioAgendamento)
            .Select(x => new
            {
                x.Id,
                x.DataAgendamento,
                x.HorarioAgendamento,
                x.StatusAgendamento,
                Clientes = new { x.Clientes.Nome, x.Clientes.NumeroTelefone },
                Servicos = new { x.Servicos.Nome, x.Servicos.Duracao, x.Servicos.Valor }
            })
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
            .Query(x => x.DataAgendamento.Date == dataSelecionada
                        && x.StatusAgendamento != StatusAgendamentoEnum.Cancelado)
            .Select(x => new { Inicio = x.HorarioAgendamento, x.Servicos.Duracao })
            .ToList();

        var horariosDisponiveis = new List<string>();
        var horarioAtual = expediente.Abertura;

        if (dataSelecionada == DateTime.Today)
        {
            var horaAtual = DateTime.Now.TimeOfDay;
            while (horarioAtual <= horaAtual)
            {
                horarioAtual += IntervaloMinimoEntreHorarios;
            }
        }

        while (horarioAtual + servico.Duracao <= expediente.Fechamento)
        {
            var horarioFinal = horarioAtual + servico.Duracao;

            var conflita = agendamentosDoDia
                .Any(x => horarioAtual < x.Inicio + x.Duracao && horarioFinal > x.Inicio);

            if (!conflita)
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

        IncluirInterno(new CriarAgendamentoManualRequestDTO
        {
            NomeCliente = request.NomeCliente,
            NumeroTelefoneCliente = request.NumeroTelefoneCliente,
            ServicoId = request.ServicoId,
            DataAgendamento = request.DataAgendamento,
            HorarioAgendamento = request.HorarioAgendamento,
            Observacao = request.Observacao
        }, aprovarAutomaticamente: false);
    }

    public void IncluirManual(CriarAgendamentoManualRequestDTO requestDto)
        => IncluirInterno(requestDto, aprovarAutomaticamente: true);

    public string AprovarSolicitacao(int id)
    {
        var dados = _agendamentoRepository
            .Query(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.StatusAgendamento,
                x.DataAgendamento,
                x.HorarioAgendamento,
                NomeCliente = x.Clientes.Nome,
                NumeroTelefoneCliente = x.Clientes.NumeroTelefone,
                NomeServico = x.Servicos.Nome
            })
            .FirstOrDefault() ?? throw new InvalidOperationException("Solicitação de agendamento não encontrada.");

        var telefone = TelefoneHelper.Normalizar(dados.NumeroTelefoneCliente);
        if (string.IsNullOrWhiteSpace(telefone))
        {
            throw new InvalidOperationException("Este cliente não possui telefone para envio no WhatsApp.");
        }

        if (dados.StatusAgendamento != StatusAgendamentoEnum.Aprovado)
        {
            var agendamento = _agendamentoRepository.FindById(id);
            agendamento.StatusAgendamento = StatusAgendamentoEnum.Aprovado;
            _agendamentoRepository.Update(agendamento);
        }

        var mensagem = MontarMensagemConfirmacao(
            dados.NomeCliente, dados.NomeServico, dados.DataAgendamento, dados.HorarioAgendamento);

        return $"https://wa.me/{telefone}?text={Uri.EscapeDataString(mensagem)}";
    }

    private (TimeSpan Abertura, TimeSpan Fechamento, DiasFuncionamentoEnum DiasFuncionamento) ObterExpediente()
    {
        var parametros = _parametrosRepository
            .Query(x => true)
            .Select(x => new
            {
                x.HorarioAbertura,
                x.HorarioFechamento,
                x.DiasFuncionamento
            })
            .FirstOrDefault();

        return parametros is null
            ? (HorarioAberturaPadrao, HorarioFechamentoPadrao, DiasFuncionamentoEnum.AteSabado)
            : (parametros.HorarioAbertura, parametros.HorarioFechamento ?? HorarioFechamentoPadrao, parametros.DiasFuncionamento);
    }

    private static bool DiaDisponivelParaAgendamento(DateTime data, DiasFuncionamentoEnum diasFuncionamento)
        => diasFuncionamento switch
        {
            DiasFuncionamentoEnum.DiasUteis => data.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday,
            DiasFuncionamentoEnum.AteSabado => data.DayOfWeek is not DayOfWeek.Sunday,
            DiasFuncionamentoEnum.TodoDia => true,
            _ => false
        };

    private void IncluirInterno(CriarAgendamentoManualRequestDTO requestDto, bool aprovarAutomaticamente)
    {
        if (string.IsNullOrWhiteSpace(requestDto.NomeCliente))
        {
            throw new InvalidOperationException("Informe o nome do cliente.");
        }

        if (DataEstaBloqueada(requestDto.DataAgendamento))
        {
            throw new InvalidOperationException("A data selecionada está bloqueada por folga ou feriado.");
        }

        var servico = _servicoRepository.FindById(requestDto.ServicoId);

        var horarioSolicitado = requestDto.HorarioAgendamento.ToString(@"hh\:mm");
        var horariosDisponiveis = ListarHorariosDisponiveis(requestDto.DataAgendamento, requestDto.ServicoId);

        if (!horariosDisponiveis.Contains(horarioSolicitado))
        {
            throw new InvalidOperationException("O horário selecionado não está mais disponível.");
        }

        var numeroTelefone = TelefoneHelper.Normalizar(requestDto.NumeroTelefoneCliente);

        var cliente = BuscarOuCriarCliente(requestDto.NomeCliente, numeroTelefone);

        var novoAgendamento = new Agendamentos
        {
            Clientes = cliente,
            Servicos = servico,
            DataAgendamento = requestDto.DataAgendamento.Date,
            HorarioAgendamento = requestDto.HorarioAgendamento,
            Observacao = string.IsNullOrWhiteSpace(requestDto.Observacao) ? null : requestDto.Observacao.Trim(),
            StatusAgendamento = aprovarAutomaticamente ? StatusAgendamentoEnum.Aprovado : StatusAgendamentoEnum.Pendente,
            FoiPago = false,
            DataCriacao = DateTime.Now
        };

        _agendamentoRepository.Insert(novoAgendamento);
    }

    private Clientes BuscarOuCriarCliente(string nome, string? telefone)
    {
        Clientes? cliente = null;

        if (!string.IsNullOrWhiteSpace(telefone))
        {
            cliente = _clienteRepository
                .Query(x => x.NumeroTelefone == telefone)
                .OrderByDescending(x => x.DataCriacao)
                .FirstOrDefault();
        }

        if (cliente is null)
        {
            cliente = new Clientes
            {
                Nome = nome.Trim(),
                NumeroTelefone = telefone,
                DataCriacao = DateTime.Now
            };
            _clienteRepository.Insert(cliente);
        }
        else
        {
            cliente.Nome = nome.Trim();
            cliente.NumeroTelefone = telefone;
            _clienteRepository.Update(cliente);
        }

        return cliente;
    }

    private bool DataEstaBloqueada(DateTime data)
    {
        var dataSelecionada = data.Date;
        var proximoDia = dataSelecionada.AddDays(1);

        return _folgaFeriadoRepository
            .Query(x => x.Data >= dataSelecionada && x.Data < proximoDia)
            .Any();
    }

    private static string MontarMensagemConfirmacao(string nomeCliente, string nomeServico, DateTime dataAgendamento, TimeSpan horarioAgendamento)
    {
        var nome = string.IsNullOrWhiteSpace(nomeCliente) ? "cliente" : nomeCliente.Trim();
        var servico = string.IsNullOrWhiteSpace(nomeServico) ? "serviço" : nomeServico.Trim();
        var data = dataAgendamento.ToString("dd/MM/yyyy");
        var horario = horarioAgendamento.ToString(@"hh\:mm");

        return $"Olá, {nome}! Sua solicitação foi aprovada. Seu horário para {servico} foi confirmado para {data} às {horario}.";
    }
}