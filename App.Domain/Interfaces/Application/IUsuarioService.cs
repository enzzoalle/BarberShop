using App.Domain.DTO;
using App.Domain.Entities;

namespace App.Domain.Interfaces;

public interface IUsuarioService
{
    IEnumerable<Usuario> Listar();
    void Cadastrar(CadastrarUsuarioRequest request);
    UsuarioAutenticadoResponse Logar(LoginUsuarioRequest request);
    void Incluir(Usuario usuario);
    void Excluir(int id);
    void Editar(Usuario usuario);
}