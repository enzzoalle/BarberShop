using App.Domain.Enums;

namespace App.Domain.DTO;

public class ParametrosResponse
{
    public TimeSpan HorarioAbertura { get; set; }
    public TimeSpan? HorarioFechamento { get; set; }
    public DiasFuncionamentoEnum DiasFuncionamento { get; set; }
    public IEnumerable<string> DatasFolgaFeriado { get; set; } = [];
}