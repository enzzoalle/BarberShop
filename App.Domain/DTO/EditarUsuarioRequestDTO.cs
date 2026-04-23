namespace App.Domain.DTO;

public class EditarUsuarioRequestDTO
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string NumeroTelefone { get; set; } = string.Empty;
}