namespace App.Domain.DTO;

public class CriarServicoRequestDTO
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public TimeSpan Duracao { get; set; }
    public decimal Valor { get; set; }
}