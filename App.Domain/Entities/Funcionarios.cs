namespace App.Domain.Entities;

public class Funcionarios
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public Usuarios Usuario { get; set; } = null!;
    public bool Ativo { get; set; }
    public DateTime DataCriacao { get; set; }
}
