using App.Domain.Enums;

namespace App.Domain.Entities;

public class Parametros
{
    public int Id { get; set; }
    public TimeSpan HorarioAbertura { get; set; }
    public TimeSpan? HorarioFechamento { get; set; }
    public DiasFuncionamentoEnum DiasFuncionamento { get; set; }
    public ICollection<FolgasFeriados> FolgasFeriados { get; set; } = new List<FolgasFeriados>();
}