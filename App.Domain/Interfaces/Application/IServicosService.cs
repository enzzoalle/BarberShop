using App.Domain.Entities;

namespace App.Domain.Interfaces;

public interface IServicosService
{
    IEnumerable<Servicos> Listar();
    IEnumerable<Servicos> ListarTodos();
    void Incluir(Servicos servicos);
    void AlterarStatus(int id, bool ativo);
}