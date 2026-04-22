using App.Domain.Enums;

namespace App.Domain.Entities;

public class Empresa
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public TimeSpan HorarioAbertura { get; set; }
    public TimeSpan? HorarioFechamento { get; set; }
    public DiasFuncionamentoEnum DiasFuncionamento { get; set; }

    public ICollection<Servico> Servicos { get; set; } = new List<Servico>();
}