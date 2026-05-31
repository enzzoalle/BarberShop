using App.Domain.DTO;

namespace App.Domain.Interfaces.Application;

public interface IServicosService
{
    IEnumerable<object> ListarAtivos();
    IEnumerable<object> ListarTodos();
    void Incluir(CriarServicoRequestDTO request);
    bool AlterarStatus(int id, bool ativo);
}