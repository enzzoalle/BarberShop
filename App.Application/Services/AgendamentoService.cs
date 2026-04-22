using App.Domain.DTO;
using App.Domain.Entities;
using App.Domain.Enums;
using App.Domain.Interfaces;
using App.Domain.Interfaces.Repository;

namespace App.Application.Services;

public class AgendamentoService : IAgendamentoService
{
    private static readonly TimeSpan IntervaloMinimoEntreHorarios = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan HorarioAberturaPadrao = TimeSpan.FromHours(9);
    private static readonly TimeSpan HorarioFechamentoPadrao = TimeSpan.FromHours(18);

    private readonly IRepositoryBase<Agendamento> _agendamentoRepository;
    private readonly IRepositoryBase<Servico> _servicoRepository;
    private readonly IRepositoryBase<Cliente> _clienteRepository;
    private readonly IRepositoryBase<Empresa> _empresaRepository;

    public AgendamentoService(
        IRepositoryBase<Agendamento> agendamentoRepository,
        IRepositoryBase<Servico> servicoRepository,
        IRepositoryBase<Cliente> clienteRepository,
        IRepositoryBase<Empresa> empresaRepository)
    {
        _agendamentoRepository = agendamentoRepository;
        _servicoRepository = servicoRepository;
        _clienteRepository = clienteRepository;
        _empresaRepository = empresaRepository;
    }

    public IEnumerable<Agendamento> Listar()
    {
        return _agendamentoRepository
            .Query(x => true)
            .Select(x => new Agendamento
            {
                Id = x.Id,
                DataAgendamento = x.DataAgendamento,
                HorarioAgendamento = x.HorarioAgendamento,
                Observacao = x.Observacao,
                StatusAgendamento = x.StatusAgendamento,
                FoiPago = x.FoiPago,
                DataCriacao = x.DataCriacao,
                Cliente = new Cliente
                {
                    Id = x.Cliente.Id,
                    Nome = x.Cliente.Nome,
                    NumeroTelefone = x.Cliente.NumeroTelefone,
                    DataCriacao = x.Cliente.DataCriacao,
                    UsuarioId = x.Cliente.UsuarioId
                },
                Servico = new Servico
                {
                    Id = x.Servico.Id,
                    Nome = x.Servico.Nome,
                    Duracao = x.Servico.Duracao,
                    Valor = x.Servico.Valor,
                    Ativo = x.Servico.Ativo,
                    DataCriacao = x.Servico.DataCriacao
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

        if (!DiaDisponivelParaAgendamento(data, expediente.DiasFuncionamento))
        {
            return [];
        }

        var dataSelecionada = data.Date;

        var agendamentosDoDia = _agendamentoRepository
            .Query(x => x.DataAgendamento.Date == dataSelecionada && x.StatusAgendamento != StatusAgendamentoEnum.Cancelado)
            .Select(x => new
            {
                Inicio = x.HorarioAgendamento,
                Duracao = x.Servico.Duracao
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
        var servico = _servicoRepository.FindById(request.ServicoId);
        var horariosDisponiveis = ListarHorariosDisponiveis(request.DataAgendamento, request.ServicoId);
        var horarioSolicitado = request.HorarioAgendamento.ToString(@"hh\:mm");

        if (!horariosDisponiveis.Contains(horarioSolicitado))
        {
            throw new InvalidOperationException("O horário selecionado não está mais disponível.");
        }

        var clienteExistente = _clienteRepository
            .Query(x => x.NumeroTelefone == request.NumeroTelefoneCliente)
            .OrderByDescending(x => x.DataCriacao)
            .FirstOrDefault();

        if (clienteExistente is null)
        {
            clienteExistente = new Cliente
            {
                Nome = request.NomeCliente,
                NumeroTelefone = request.NumeroTelefoneCliente,
                DataCriacao = DateTime.Now
            };

            _clienteRepository.Insert(clienteExistente);
        }
        else
        {
            clienteExistente.Nome = request.NomeCliente;
            _clienteRepository.Update(clienteExistente);
        }

        var novoAgendamento = new Agendamento
        {
            Cliente = clienteExistente,
            Servico = servico,
            DataAgendamento = request.DataAgendamento.Date,
            HorarioAgendamento = request.HorarioAgendamento,
            Observacao = request.Observacao,
            StatusAgendamento = StatusAgendamentoEnum.Pendente,
            FoiPago = false,
            DataCriacao = DateTime.Now
        };

        _agendamentoRepository.Insert(novoAgendamento);
    }

    private (TimeSpan Abertura, TimeSpan Fechamento, DiasFuncionamentoEnum DiasFuncionamento) ObterExpediente()
    {
        var empresa = _empresaRepository.GetAll().FirstOrDefault();
        if (empresa is null)
        {
            return (HorarioAberturaPadrao, HorarioFechamentoPadrao, DiasFuncionamentoEnum.AteSabado);
        }

        return (
            empresa.HorarioAbertura,
            empresa.HorarioFechamento ?? HorarioFechamentoPadrao,
            empresa.DiasFuncionamento);
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
}