using App.Domain.Enums;

namespace App.Domain.Entities;

public class Agendamentos
{
    public int Id { get; set; }
    public Clientes Clientes { get; set; }
    public Servicos Servicos { get; set; }
    public DateTime DataAgendamento { get; set; }
    public TimeSpan HorarioAgendamento { get; set; }
    public string? Observacao { get; set; }
    public StatusAgendamentoEnum StatusAgendamento { get; set; }
    public bool FoiPago { get; set; }
    public DateTime DataCriacao { get; set; }
}