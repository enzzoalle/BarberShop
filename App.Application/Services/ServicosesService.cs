using App.Domain.Entities;
using App.Domain.Interfaces;
using App.Domain.Interfaces.Repository;

namespace App.Application.Services;

public class ServicosesService : IServicosService
{
    private readonly IRepositoryBase<Servicos> _servicoRepository;

    public ServicosesService(IRepositoryBase<Servicos> servicoRepository)
    {
        _servicoRepository = servicoRepository;
    }

    public IEnumerable<Servicos> Listar()
    {
        return _servicoRepository
            .Query(x => x.Ativo)
            .OrderBy(x => x.Nome)
            .ToList();
    }

    public IEnumerable<Servicos> ListarTodos()
    {
        return _servicoRepository
            .Query(x => true)
            .OrderBy(x => x.Nome)
            .ToList();
    }

    public void Incluir(Servicos servicos)
    {
        if (string.IsNullOrWhiteSpace(servicos.Nome))
        {
            throw new InvalidOperationException("O nome do serviço é obrigatório.");
        }

        if (servicos.Duracao <= TimeSpan.Zero)
        {
            throw new InvalidOperationException("A duração do serviço deve ser maior que zero.");
        }

        if (servicos.Valor <= 0)
        {
            throw new InvalidOperationException("O valor do serviço deve ser maior que zero.");
        }

        servicos.Nome = servicos.Nome.Trim();
        servicos.Descricao = string.IsNullOrWhiteSpace(servicos.Descricao) ? null : servicos.Descricao.Trim();
        servicos.DataCriacao = servicos.DataCriacao == default ? DateTime.Now : servicos.DataCriacao;
        servicos.Ativo = true;

        _servicoRepository.Insert(servicos);
    }

    public void AlterarStatus(int id, bool ativo)
    {
        var servico = _servicoRepository.FindById(id);
        servico.Ativo = ativo;
        _servicoRepository.Update(servico);
    }
}