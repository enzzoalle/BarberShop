namespace App.Domain.Entities;

public class HorariosFixos
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public Usuarios Usuario { get; set; } = null!;
    public int ServicoId { get; set; }
    public Servicos Servico { get; set; } = null!;
    public DayOfWeek DiaDaSemana { get; set; }
    public TimeSpan Horario { get; set; }
    public int RepetirACadaSemanas { get; set; }
    public DateTime DataCriacao { get; set; }
}
