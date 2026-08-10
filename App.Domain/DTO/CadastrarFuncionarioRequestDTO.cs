namespace App.Domain.DTO;

public class CadastrarFuncionarioRequestDTO
{
    public string Nome { get; set; } = string.Empty;
    public string NumeroTelefone { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}
