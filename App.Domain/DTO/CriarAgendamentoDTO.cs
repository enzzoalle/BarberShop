using System.ComponentModel.DataAnnotations;

namespace App.Domain.DTO;

public class CriarAgendamentoRequest
{
    [Required(ErrorMessage = "Informe o nome do cliente.")]
    public string NomeCliente { get; set; }

    [Required(ErrorMessage = "Informe o telefone do cliente.")]
    public string NumeroTelefoneCliente { get; set; }

    [Required(ErrorMessage = "Informe o serviço.")]
    public int ServicoId { get; set; }

    [Required(ErrorMessage = "Informe o funcionário.")]
    public int FuncionarioId { get; set; }

    public DateTime DataAgendamento { get; set; }
    public TimeSpan HorarioAgendamento { get; set; }
    public string? Observacao { get; set; }
}