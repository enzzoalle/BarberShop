using App.Domain.DTO;
using App.Domain.Entities;
using App.Domain.Interfaces;
using App.Domain.Interfaces.Repository;
using App.Common;

namespace App.Application.Services;

public class UsuarioService : IUsuarioService
{
    private readonly IRepositoryBase<Usuario> _usuarioRepository;

    public UsuarioService(IRepositoryBase<Usuario> usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    public IEnumerable<Usuario> Listar()
    {
        var registros = _usuarioRepository.GetAll();
        return registros;
    }

    public void Cadastrar(CadastrarUsuarioRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nome) || string.IsNullOrWhiteSpace(request.Senha) || string.IsNullOrWhiteSpace(request.NumeroTelefone))
        {
            throw new InvalidOperationException("Usuário, telefone e senha são obrigatórios.");
        }

        var usuarioNormalizado = request.Nome.Trim();
        var numeroTelefoneNormalizado = NormalizarTelefone(request.NumeroTelefone);
        if (string.IsNullOrWhiteSpace(numeroTelefoneNormalizado))
        {
            throw new InvalidOperationException("Telefone inválido.");
        }

        var telefoneJaExiste = _usuarioRepository.Query(x => x.NumeroTelefone == numeroTelefoneNormalizado).Any();
        if (telefoneJaExiste)
        {
            throw new InvalidOperationException("Já existe um cadastro para esse telefone.");
        }

        var usuarioJaExiste = _usuarioRepository.Query(x => x.Nome.ToLower() == usuarioNormalizado.ToLower()).Any();
        if (usuarioJaExiste)
        {
            throw new InvalidOperationException("Já existe um cadastro para esse usuário.");
        }

        var senhaHash = Criptografia.geraHash_SHA512(request.Senha.Trim());

        var novoUsuario = new Usuario
        {
            Nome = usuarioNormalizado,
            Senha = senhaHash,
            NumeroTelefone = numeroTelefoneNormalizado,
            DataCriacao = DateTime.Now,
            IsAdmin = false
        };

        _usuarioRepository.Insert(novoUsuario);
    }

    public UsuarioAutenticadoResponse Logar(LoginUsuarioRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Usuario) || string.IsNullOrWhiteSpace(request.Senha))
        {
            throw new UnauthorizedAccessException("Usuário ou senha inválidos.");
        }

        var usuarioNormalizado = request.Usuario.Trim();
        var senhaHash = Criptografia.geraHash_SHA512(request.Senha.Trim());

        var usuario = _usuarioRepository
            .Query(x => x.Nome.ToLower() == usuarioNormalizado.ToLower() && x.Senha == senhaHash)
            .FirstOrDefault();

        if (usuario is null)
        {
            throw new UnauthorizedAccessException("Usuário ou senha inválidos.");
        }

        return new UsuarioAutenticadoResponse
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Usuario = usuario.Nome,
            NumeroTelefone = usuario.NumeroTelefone,
            IsAdmin = usuario.IsAdmin
        };
    }

    public void Incluir(Usuario usuario)
    {
        Cadastrar(new CadastrarUsuarioRequest
        {
            Nome = usuario.Nome,
            Senha = usuario.Senha,
            NumeroTelefone = usuario.NumeroTelefone
        });
    }
    
    public void Excluir(int id)
    {
        var objeto = _usuarioRepository.FindById(id);
        _usuarioRepository.Remove(objeto);
    }
    
    public void Editar(Usuario usuario)
    {
        _usuarioRepository.Update(usuario);
    }

    private static string NormalizarTelefone(string numeroTelefone)
    {
        return new string(numeroTelefone.Where(char.IsDigit).ToArray());
    }
}