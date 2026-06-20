using System;

namespace App.Domain.DTO;

public class IncluirHorarioFixoRequestDTO
{
    public int UsuarioId { get; set; }
    public int ServicoId { get; set; }
    public DayOfWeek DiaDaSemana { get; set; }
    public TimeSpan Horario { get; set; }
    public int RepetirACadaSemanas { get; set; }
}
