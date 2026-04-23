using App.Domain.Enums;

namespace App.Domain.DTO;

public class SalvarParametrosRequestDTO
{
    public TimeSpan HorarioAbertura { get; set; }
    public TimeSpan? HorarioFechamento { get; set; }
    public DiasFuncionamentoEnum DiasFuncionamento { get; set; }
    public IEnumerable<DateTime>? DatasFolgaFeriado { get; set; }
}

