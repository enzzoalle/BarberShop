using App.Domain.DTO;
using App.Domain.Entities;

namespace App.Domain.Interfaces;

public interface IServicosService
{
    IEnumerable<object> Listar();
    IEnumerable<object> ListarTodos();
    void Incluir(CriarServicoRequestDTO request);
    bool AlterarStatus(int id, bool ativo);
}