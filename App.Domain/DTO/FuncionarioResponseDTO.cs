namespace App.Domain.DTO;

public class FuncionarioResponseDTO
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string NumeroTelefone { get; set; } = string.Empty;
    public string? FotoPerfil { get; set; }
    public bool Ativo { get; set; }
}
