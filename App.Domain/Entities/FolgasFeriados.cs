namespace App.Domain.Entities;

public class FolgasFeriados
{
    public int Id { get; set; }
    public DateTime Data { get; set; }
    public string? Descricao { get; set; }
    public int ParametrosId { get; set; }
    public Parametros Parametros { get; set; } = null!;
}


