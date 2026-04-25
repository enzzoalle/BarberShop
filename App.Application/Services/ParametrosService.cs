using App.Domain.DTO;
using App.Domain.Entities;
using App.Domain.Enums;
using App.Domain.Interfaces;
using App.Domain.Interfaces.Repository;

namespace App.Application.Services;

public class ParametrosService : IParametrosService
{
    private static readonly TimeSpan HorarioAberturaPadrao = TimeSpan.FromHours(9);
    private static readonly TimeSpan HorarioFechamentoPadrao = TimeSpan.FromHours(18);

    private readonly IRepositoryBase<Parametros> _parametrosRepository;
    private readonly IRepositoryBase<FolgasFeriados> _folgaFeriadoRepository;

    public ParametrosService(
        IRepositoryBase<Parametros> parametrosRepository,
        IRepositoryBase<FolgasFeriados> folgaFeriadoRepository)
    {
        _parametrosRepository = parametrosRepository;
        _folgaFeriadoRepository = folgaFeriadoRepository;
    }

    public ParametrosResponseDTO Obter()
    {
        var parametros = _parametrosRepository
            .Query(x => true)
            .Select(x => new
            {
                x.Id,
                x.HorarioAbertura,
                x.HorarioFechamento,
                x.DiasFuncionamento
            })
            .FirstOrDefault();

        if (parametros is null)
        {
            return new ParametrosResponseDTO
            {
                HorarioAbertura = HorarioAberturaPadrao,
                HorarioFechamento = HorarioFechamentoPadrao,
                DiasFuncionamento = DiasFuncionamentoEnum.AteSabado,
                DatasFolgaFeriado = new List<string>()
            };
        }

        var folgas = _folgaFeriadoRepository
            .Query(x => x.ParametrosId == parametros.Id)
            .OrderBy(x => x.Data)
            .Select(x => x.Data)
            .ToList()
            .Select(data => data.ToString("yyyy-MM-dd"))
            .ToList();

        return new ParametrosResponseDTO
        {
            HorarioAbertura = parametros.HorarioAbertura,
            HorarioFechamento = parametros.HorarioFechamento ?? HorarioFechamentoPadrao,
            DiasFuncionamento = parametros.DiasFuncionamento,
            DatasFolgaFeriado = folgas
        };
    }

    public void Salvar(SalvarParametrosRequestDTO requestDto)
    {
        if (requestDto.HorarioAbertura == TimeSpan.Zero)
        {
            throw new InvalidOperationException("Informe um horário de abertura válido.");
        }

        if (requestDto.HorarioFechamento is not null && requestDto.HorarioFechamento <= requestDto.HorarioAbertura)
        {
            throw new InvalidOperationException("O horário de fechamento deve ser maior que o horário de abertura.");
        }

        var parametros = _parametrosRepository.Query(x => true).FirstOrDefault();
        if (parametros is null)
        {
            parametros = new Parametros
            {
                HorarioAbertura = requestDto.HorarioAbertura,
                HorarioFechamento = requestDto.HorarioFechamento,
                DiasFuncionamento = requestDto.DiasFuncionamento
            };
            _parametrosRepository.Insert(parametros);
        }
        else
        {
            parametros.HorarioAbertura = requestDto.HorarioAbertura;
            parametros.HorarioFechamento = requestDto.HorarioFechamento;
            parametros.DiasFuncionamento = requestDto.DiasFuncionamento;
            _parametrosRepository.Update(parametros);
        }

        SincronizarFolgas(parametros.Id, requestDto.DatasFolgaFeriado);
    }

    private void SincronizarFolgas(int parametrosId, IEnumerable<DateTime>? datasRequest)
    {
        var datasDesejadas = (datasRequest ?? [])
            .Select(x => x.Date)
            .Distinct()
            .ToHashSet();

        var folgasAtuais = _folgaFeriadoRepository
            .Query(x => x.ParametrosId == parametrosId)
            .ToList();

        var paraRemover = folgasAtuais.Where(x => !datasDesejadas.Contains(x.Data.Date)).ToList();
        foreach (var folga in paraRemover)
        {
            _folgaFeriadoRepository.Remove(folga);
        }

        var datasExistentes = folgasAtuais.Select(x => x.Data.Date).ToHashSet();
        var paraInserir = datasDesejadas.Where(x => !datasExistentes.Contains(x)).ToList();
        foreach (var data in paraInserir)
        {
            _folgaFeriadoRepository.Insert(new FolgasFeriados
            {
                ParametrosId = parametrosId,
                Data = data
            });
        }
    }
}