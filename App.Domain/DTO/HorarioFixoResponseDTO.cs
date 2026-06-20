namespace App.Domain.DTO;

public class HorarioFixoResponseDTO
{
    public int Id { get; set; }
    public int DiaDaSemana { get; set; }
    public string Horario { get; set; } = string.Empty;
    public int RepetirACadaSemanas { get; set; }
    public string Servico { get; set; } = string.Empty;
}
