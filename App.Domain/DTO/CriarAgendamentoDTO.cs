namespace App.Domain.DTO;

public class CriarAgendamentoRequest
{
    public string NomeCliente { get; set; }
    public string NumeroTelefoneCliente { get; set; }
    public int ServicoId { get; set; }
    public DateTime DataAgendamento { get; set; }
    public TimeSpan HorarioAgendamento { get; set; }
    public string? Observacao { get; set; }
}