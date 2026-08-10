using App.Domain.DTO;

namespace App.Domain.Interfaces.Application;

public interface IFuncionariosService
{
    IEnumerable<FuncionarioResponseDTO> Listar();
    IEnumerable<FuncionarioResponseDTO> ListarAtivos();
    void Cadastrar(CadastrarFuncionarioRequestDTO request);
    void AlterarStatus(int id, bool ativo);
}
