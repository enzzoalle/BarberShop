using App.Domain.DTO;
using App.Domain.Entities;
using App.Domain.Interfaces;
using App.Domain.Interfaces.Repository;

namespace App.Application.Services;

public class ServicosService : IServicosService
{
    private readonly IRepositoryBase<Servicos> _servicoRepository;

    public ServicosService(IRepositoryBase<Servicos> servicoRepository)
    {
        _servicoRepository = servicoRepository;
    }

    public IEnumerable<object> ListarAtivos()
    {
        return _servicoRepository
            .Query(x => x.Ativo)
            .OrderBy(x => x.Nome)
            .Select(x => new
            {
                x.Id,
                x.Nome,
                x.Duracao,
                x.Valor,
                x.Descricao,
                x.Ativo
            })
            .ToList();
    }

    public IEnumerable<object> ListarTodos()
    {
        return _servicoRepository
            .Query(x => true)
            .OrderBy(x => x.Nome)
            .Select(x => new
            {
                x.Id,
                x.Nome,
                x.Duracao,
                x.Valor,
                x.Descricao,
                x.Ativo
            })
            .ToList();
    }

    public void Incluir(CriarServicoRequestDTO request)
    {
        if (string.IsNullOrWhiteSpace(request.Nome))
        {
            throw new InvalidOperationException("O nome do serviço é obrigatório.");
        }

        if (request.Duracao <= TimeSpan.Zero)
        {
            throw new InvalidOperationException("A duração do serviço deve ser maior que zero.");
        }

        if (request.Valor <= 0)
        {
            throw new InvalidOperationException("O valor do serviço deve ser maior que zero.");
        }

        var servico = new Servicos
        {
            Nome = request.Nome.Trim(),
            Descricao = string.IsNullOrWhiteSpace(request.Descricao) ? null : request.Descricao.Trim(),
            Duracao = request.Duracao,
            Valor = request.Valor,
            DataCriacao = DateTime.Now,
            Ativo = true
        };

        _servicoRepository.Insert(servico);
    }

    public bool AlterarStatus(int id, bool ativo)
    {
        var servico = _servicoRepository.FindById(id);
        servico.Ativo = ativo;
        _servicoRepository.Update(servico);
        return true;
    }
}