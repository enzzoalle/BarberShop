using App.Domain.DTO;
using App.Domain.Entities;
using App.Domain.Interfaces.Application;
using App.Domain.Interfaces.Repository;
using App.Common;

namespace App.Application.Services;

public class UsuariosService : IUsuariosService
{
    private readonly IRepositoryBase<Usuarios> _usuarioRepository;
    private readonly IRepositoryBase<Funcionarios> _funcionarioRepository;

    public UsuariosService(IRepositoryBase<Usuarios> usuarioRepository, IRepositoryBase<Funcionarios> funcionarioRepository)
    {
        _usuarioRepository = usuarioRepository;
        _funcionarioRepository = funcionarioRepository;
    }

    public IEnumerable<Usuarios> Listar()
        => _usuarioRepository.Query(x => true).ToList();

    public void Cadastrar(CadastrarUsuarioRequestDTO requestDto)
        => CriarUsuario(requestDto.Nome, requestDto.NumeroTelefone, requestDto.Senha, isAdmin: false);

    public int CadastrarComoAdmin(string nome, string telefone, string senha)
        => CriarUsuario(nome, telefone, senha, isAdmin: true).Id;

    private Usuarios CriarUsuario(string nome, string telefone, string senha, bool isAdmin)
    {
        if (string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(senha) || string.IsNullOrWhiteSpace(telefone))
        {
            throw new InvalidOperationException("Usuário, telefone e senha são obrigatórios.");
        }

        var nomeNormalizado = nome.Trim();
        var telefoneNormalizado = TextoHelper.NormalizarTelefone(telefone);

        if (string.IsNullOrWhiteSpace(telefoneNormalizado))
        {
            throw new InvalidOperationException("Telefone inválido.");
        }

        ValidarDadosUnicos(nomeNormalizado, telefoneNormalizado, idParaIgnorar: null);

        var novoUsuario = new Usuarios
        {
            Nome = nomeNormalizado,
            Senha = Criptografia.GeraHash(senha.Trim()),
            NumeroTelefone = telefoneNormalizado,
            DataCriacao = DateTime.Now,
            IsAdmin = isAdmin
        };

        _usuarioRepository.Insert(novoUsuario);
        return novoUsuario;
    }

    public UsuarioAutenticadoResponseDTO Logar(LoginUsuarioRequestDTO requestDto)
    {
        if (string.IsNullOrWhiteSpace(requestDto.Usuario) || string.IsNullOrWhiteSpace(requestDto.Senha))
        {
            throw new UnauthorizedAccessException("Usuário ou senha inválidos.");
        }

        var nomeNormalizado = requestDto.Usuario.Trim();
        var senhaHash = Criptografia.GeraHash(requestDto.Senha.Trim());

        var usuario = _usuarioRepository.Query(x => x.Nome.ToLower() == nomeNormalizado.ToLower() && x.Senha == senhaHash).FirstOrDefault()
                      ?? throw new UnauthorizedAccessException("Usuário ou senha inválidos.");

        var funcionario = _funcionarioRepository.Query(x => x.UsuarioId == usuario.Id).FirstOrDefault();
        if (funcionario is { Ativo: false })
        {
            throw new UnauthorizedAccessException("Usuário inativo.");
        }

        return new UsuarioAutenticadoResponseDTO
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Usuario = usuario.Nome,
            NumeroTelefone = usuario.NumeroTelefone,
            IsAdmin = usuario.IsAdmin,
            FotoPerfil = usuario.FotoPerfil
        };
    }

    public void Excluir(int id)
    {
        var usuario = _usuarioRepository.FindById(id)
                      ?? throw new InvalidOperationException("Usuário não encontrado.");

        _usuarioRepository.Remove(usuario);
    }

    public void Editar(EditarUsuarioRequestDTO request)
    {
        if (string.IsNullOrWhiteSpace(request.Nome) || string.IsNullOrWhiteSpace(request.NumeroTelefone))
        {
            throw new InvalidOperationException("Nome e telefone são obrigatórios.");
        }

        var usuario = _usuarioRepository.FindById(request.Id)
                      ?? throw new InvalidOperationException("Usuário não encontrado.");

        var nomeNormalizado = request.Nome.Trim();
        var telefoneNormalizado = TextoHelper.NormalizarTelefone(request.NumeroTelefone);

        if (string.IsNullOrWhiteSpace(telefoneNormalizado))
        {
            throw new InvalidOperationException("Telefone inválido.");
        }

        ValidarDadosUnicos(nomeNormalizado, telefoneNormalizado, idParaIgnorar: usuario.Id);

        usuario.Nome = nomeNormalizado;
        usuario.NumeroTelefone = telefoneNormalizado;

        _usuarioRepository.Update(usuario);
    }

    public string AtualizarFotoPerfil(int id, string nomeArquivo, byte[] conteudo)
    {
        var extensoesPermitidas = new[] { ".png", ".jpg", ".jpeg" };
        var extensao = Path.GetExtension(nomeArquivo).ToLowerInvariant();

        if (!extensoesPermitidas.Contains(extensao))
        {
            throw new InvalidOperationException("Formato de imagem inválido.");
        }

        var usuario = _usuarioRepository.FindById(id)
                      ?? throw new InvalidOperationException("Usuário não encontrado.");

        var fotoPerfil = Convert.ToBase64String(conteudo);
        usuario.FotoPerfil = fotoPerfil;
        _usuarioRepository.Update(usuario);

        return fotoPerfil;
    }

    private void ValidarDadosUnicos(string nome, string telefone, int? idParaIgnorar)
    {
        if (_usuarioRepository.Query(x => x.NumeroTelefone == telefone && x.Id != (idParaIgnorar ?? 0)).Any())
        {
            throw new InvalidOperationException("Já existe um cadastro para esse telefone.");
        }

        if (_usuarioRepository.Query(x => x.Nome.ToLower() == nome.ToLower() && x.Id != (idParaIgnorar ?? 0)).Any())
        {
            throw new InvalidOperationException("Já existe um cadastro para esse usuário.");
        }
    }
}