using App.Domain.DTO;
using App.Domain.Entities;

namespace App.Domain.Interfaces;

public interface IUsuariosService
{
    IEnumerable<Usuarios> Listar();
    void Cadastrar(CadastrarUsuarioRequest request);
    UsuarioAutenticadoResponse Logar(LoginUsuarioRequest request);
    void Incluir(Usuarios usuarios);
    void Excluir(int id);
    void Editar(Usuarios usuarios);
}