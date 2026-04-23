using App.Domain.DTO;
using App.Domain.Entities;

namespace App.Domain.Interfaces;

public interface IServicosService
{
    IEnumerable<Servicos> Listar();
    IEnumerable<Servicos> ListarTodos();
    void Incluir(CriarServicoRequestDTO request);
    void AlterarStatus(int id, bool ativo);
}