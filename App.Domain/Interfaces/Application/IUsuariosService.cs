using App.Domain.DTO;
using App.Domain.Entities;

namespace App.Domain.Interfaces.Application;

public interface IUsuariosService
{
    IEnumerable<Usuarios> Listar();
    void Cadastrar(CadastrarUsuarioRequestDTO requestDto);
    int CadastrarComoAdmin(string nome, string telefone, string senha);
    UsuarioAutenticadoResponseDTO Logar(LoginUsuarioRequestDTO requestDto);
    void Excluir(int id);
    void Editar(EditarUsuarioRequestDTO request);
    string AtualizarFotoPerfil(int id, string nomeArquivo, byte[] conteudo);
}