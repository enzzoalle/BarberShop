namespace App.Domain.DTO;

public class UsuarioAutenticadoResponse
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string Usuario { get; set; }
    public string NumeroTelefone { get; set; }
    public bool IsAdmin { get; set; }
}

