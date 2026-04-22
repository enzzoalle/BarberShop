using App.Domain.Enums;

namespace App.Domain.Entities;

public class Agendamento
{
    public int Id { get; set; }
    public Cliente Cliente { get; set; }
    public Servico Servico { get; set; }
    public DateTime DataAgendamento { get; set; }
    public TimeSpan HorarioAgendamento { get; set; }
    public string? Observacao { get; set; }
    public StatusAgendamentoEnum StatusAgendamento { get; set; }
    public bool FoiPago { get; set; }
    public DateTime DataCriacao { get; set; }
}