namespace App.Domain.Entities;

public class Servico
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string? Descricao { get; set; }
    public TimeSpan Duracao { get; set; }
    public decimal Valor { get; set; }
    public bool Ativo { get; set; }
    public DateTime DataCriacao { get; set; }

    public ICollection<Agendamento> Agendamentos { get; set; } = new List<Agendamento>();
}