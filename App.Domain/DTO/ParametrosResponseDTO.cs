using App.Domain.Enums;

namespace App.Domain.DTO;

public class ParametrosResponseDTO
{
    public TimeSpan HorarioAbertura { get; set; }
    public TimeSpan? HorarioFechamento { get; set; }
    public DiasFuncionamentoEnum DiasFuncionamento { get; set; }
    public IEnumerable<string> DatasFolgaFeriado { get; set; } = [];
    public string? TelefonePrincipal { get; set; }
    public string? TelefoneSecundario { get; set; }
    public string? Localizacao { get; set; }
}