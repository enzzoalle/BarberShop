namespace App.Domain.Entities;

public class AgendamentosRecorrentes
{
    public int Id { get; set; }
    public Clientes Clientes { get; set; }
    public Servicos Servicos { get; set; }
    public DateTime DataAgendamento { get; set; }
    public TimeSpan HorarioAgendamento { get; set; }
}