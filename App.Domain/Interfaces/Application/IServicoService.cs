using App.Domain.Entities;

namespace App.Domain.Interfaces;

public interface IServicoService
{
    IEnumerable<Servico> Listar();
    IEnumerable<Servico> ListarTodos();
    void Incluir(Servico servico);
    void AlterarStatus(int id, bool ativo);
}