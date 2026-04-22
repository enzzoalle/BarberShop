namespace App.Domain.Entities;

public class Clientes
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string? NumeroTelefone { get; set; }
    public DateTime DataCriacao { get; set; }
    public int? UsuarioId { get; set; }
    public Usuarios? Usuario { get; set; }

    public ICollection<Agendamentos> Agendamentos { get; set; } = new List<Agendamentos>();
}