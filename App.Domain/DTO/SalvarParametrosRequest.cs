using App.Domain.Enums;

namespace App.Domain.DTO;

public class SalvarParametrosRequest
{
    public TimeSpan HorarioAbertura { get; set; }
    public TimeSpan? HorarioFechamento { get; set; }
    public DiasFuncionamentoEnum DiasFuncionamento { get; set; }
    public IEnumerable<DateTime>? DatasFolgaFeriado { get; set; }
}

