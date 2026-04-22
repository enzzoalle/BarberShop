namespace App.Domain.Entities;

public class Usuario
{
    public int Id { get; set; }
    public Cliente? Cliente { get; set; }
    public string Nome { get; set; }
    public string Senha { get; set; }
    public string NumeroTelefone { get; set; }
    public bool IsAdmin { get; set; }
    public DateTime DataCriacao { get; set; }
}