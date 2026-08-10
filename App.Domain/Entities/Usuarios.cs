namespace App.Domain.Entities;

public class Usuarios
{
    public int Id { get; set; }
    public Clientes? Cliente { get; set; }
    public Funcionarios? Funcionario { get; set; }
    public string Nome { get; set; }
    public string Senha { get; set; }
    public string NumeroTelefone { get; set; }
    public bool IsAdmin { get; set; }
    public string? FotoPerfil { get; set; }
    public DateTime DataCriacao { get; set; }
}