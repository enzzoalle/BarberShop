using App.Domain.DTO;
using App.Domain.Entities;
using App.Domain.Interfaces.Application;
using App.Domain.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;

namespace App.Application.Services;

public class FuncionariosService : IFuncionariosService
{
    private readonly IRepositoryBase<Funcionarios> _funcionarioRepository;
    private readonly IUsuariosService _usuariosService;

    public FuncionariosService(IRepositoryBase<Funcionarios> funcionarioRepository, IUsuariosService usuariosService)
    {
        _funcionarioRepository = funcionarioRepository;
        _usuariosService = usuariosService;
    }

    public IEnumerable<FuncionarioResponseDTO> Listar()
        => Projetar(_funcionarioRepository.Query(x => true));

    public IEnumerable<FuncionarioResponseDTO> ListarAtivos()
        => Projetar(_funcionarioRepository.Query(x => x.Ativo));

    public void Cadastrar(CadastrarFuncionarioRequestDTO request)
    {
        var usuarioId = _usuariosService.CadastrarComoAdmin(request.Nome, request.NumeroTelefone, request.Senha);

        var funcionario = new Funcionarios
        {
            UsuarioId = usuarioId,
            Ativo = true,
            DataCriacao = DateTime.Now
        };

        _funcionarioRepository.Insert(funcionario);
    }

    public void AlterarStatus(int id, bool ativo)
    {
        var funcionario = _funcionarioRepository.FindById(id)
                          ?? throw new InvalidOperationException("Funcionário não encontrado.");

        funcionario.Ativo = ativo;
        _funcionarioRepository.Update(funcionario);
    }

    private static List<FuncionarioResponseDTO> Projetar(IQueryable<Funcionarios> query)
        => query
            .Include(f => f.Usuario)
            .OrderBy(f => f.Usuario.Nome)
            .Select(f => new FuncionarioResponseDTO
            {
                Id = f.Id,
                UsuarioId = f.UsuarioId,
                Nome = f.Usuario.Nome,
                NumeroTelefone = f.Usuario.NumeroTelefone,
                FotoPerfil = f.Usuario.FotoPerfil,
                Ativo = f.Ativo
            })
            .ToList();
}