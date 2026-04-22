namespace App.Domain.Entities;

public class Cliente
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string NumeroTelefone { get; set; }
    public DateTime DataCriacao { get; set; }
    public int? UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public ICollection<Agendamento> Agendamentos { get; set; } = new List<Agendamento>();
}