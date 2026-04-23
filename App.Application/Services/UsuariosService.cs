using App.Domain.DTO;
using App.Domain.Entities;
using App.Domain.Interfaces;
using App.Domain.Interfaces.Repository;
using App.Common;

namespace App.Application.Services;

public class UsuariosService : IUsuariosService
{
    private readonly IRepositoryBase<Usuarios> _usuarioRepository;

    public UsuariosService(IRepositoryBase<Usuarios> usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    public IEnumerable<Usuarios> Listar()
        => _usuarioRepository.Query(x => true).ToList();

    public void Cadastrar(CadastrarUsuarioRequestDTO requestDto)
    {
        if (string.IsNullOrWhiteSpace(requestDto.Nome) || string.IsNullOrWhiteSpace(requestDto.Senha) || string.IsNullOrWhiteSpace(requestDto.NumeroTelefone))
        {
            throw new InvalidOperationException("Usuário, telefone e senha são obrigatórios.");
        }

        var nomeNormalizado = requestDto.Nome.Trim();
        var telefoneNormalizado = TelefoneHelper.Normalizar(requestDto.NumeroTelefone);

        if (string.IsNullOrWhiteSpace(telefoneNormalizado))
        {
            throw new InvalidOperationException("Telefone inválido.");
        }

        if (_usuarioRepository.Query(x => x.NumeroTelefone == telefoneNormalizado).Any())
        {
            throw new InvalidOperationException("Já existe um cadastro para esse telefone.");
        }

        if (_usuarioRepository.Query(x => x.Nome.ToLower() == nomeNormalizado.ToLower()).Any())
        {
            throw new InvalidOperationException("Já existe um cadastro para esse usuário.");
        }

        var novoUsuario = new Usuarios
        {
            Nome = nomeNormalizado,
            Senha = Criptografia.GeraHash(requestDto.Senha.Trim()),
            NumeroTelefone = telefoneNormalizado,
            DataCriacao = DateTime.Now,
            IsAdmin = false
        };

        _usuarioRepository.Insert(novoUsuario);
    }

    public UsuarioAutenticadoResponseDTO Logar(LoginUsuarioRequestDTO requestDto)
    {
        if (string.IsNullOrWhiteSpace(requestDto.Usuario) || string.IsNullOrWhiteSpace(requestDto.Senha))
        {
            throw new UnauthorizedAccessException("Usuário ou senha inválidos.");
        }

        var nomeNormalizado = requestDto.Usuario.Trim();
        var senhaHash = Criptografia.GeraHash(requestDto.Senha.Trim());

        var usuario = _usuarioRepository
                          .Query(x => x.Nome.ToLower() == nomeNormalizado.ToLower() && x.Senha == senhaHash)
                          .FirstOrDefault()
                      ?? throw new UnauthorizedAccessException("Usuário ou senha inválidos.");

        return new UsuarioAutenticadoResponseDTO
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Usuario = usuario.Nome,
            NumeroTelefone = usuario.NumeroTelefone,
            IsAdmin = usuario.IsAdmin
        };
    }

    public void Excluir(int id)
    {
        var usuario = _usuarioRepository.FindById(id);
        _usuarioRepository.Remove(usuario);
    }

    public void Editar(EditarUsuarioRequestDTO request)
    {
        var usuario = _usuarioRepository.FindById(request.Id);

        usuario.Nome           = request.Nome.Trim();
        usuario.NumeroTelefone = TelefoneHelper.Normalizar(request.NumeroTelefone) ?? usuario.NumeroTelefone;

        _usuarioRepository.Update(usuario);
    }
}