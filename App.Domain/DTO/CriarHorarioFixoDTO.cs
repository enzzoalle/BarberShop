using App.Domain.Enums;

namespace App.Domain.DTO;

public class CriarHorarioFixoRequest
{
    public string NomeCliente { get; set; }
    public string NumeroTelefoneCliente { get; set; }
    public int ServicoId { get; set; }
    public DiasSemanaEnum DiaSemana { get; set; }
    public int IntervaloSemanas { get; set; }
    public TimeSpan HorarioAgendamento { get; set; }
}