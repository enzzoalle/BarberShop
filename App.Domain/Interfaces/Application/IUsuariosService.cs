using App.Domain.DTO;
using App.Domain.Entities;

namespace App.Domain.Interfaces.Application;

public interface IUsuariosService
{
    IEnumerable<Usuarios> Listar();
    void Cadastrar(CadastrarUsuarioRequestDTO requestDto);
    UsuarioAutenticadoResponseDTO Logar(LoginUsuarioRequestDTO requestDto);
    void Excluir(int id);
    void Editar(EditarUsuarioRequestDTO request);
    void AtualizarFotoPerfil(int id, string fotoPerfil);
}