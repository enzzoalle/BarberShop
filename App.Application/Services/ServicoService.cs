using App.Domain.Entities;
using App.Domain.Interfaces;
using App.Domain.Interfaces.Repository;

namespace App.Application.Services;

public class ServicoService : IServicoService
{
    private readonly IRepositoryBase<Servico> _servicoRepository;

    public ServicoService(IRepositoryBase<Servico> servicoRepository)
    {
        _servicoRepository = servicoRepository;
    }

    public IEnumerable<Servico> Listar()
    {
        return _servicoRepository
            .Query(x => x.Ativo)
            .OrderBy(x => x.Nome)
            .ToList();
    }

    public IEnumerable<Servico> ListarTodos()
    {
        return _servicoRepository
            .Query(x => true)
            .OrderBy(x => x.Nome)
            .ToList();
    }

    public void Incluir(Servico servico)
    {
        if (string.IsNullOrWhiteSpace(servico.Nome))
        {
            throw new InvalidOperationException("O nome do serviço é obrigatório.");
        }

        if (servico.Duracao <= TimeSpan.Zero)
        {
            throw new InvalidOperationException("A duração do serviço deve ser maior que zero.");
        }

        if (servico.Valor <= 0)
        {
            throw new InvalidOperationException("O valor do serviço deve ser maior que zero.");
        }

        servico.Nome = servico.Nome.Trim();
        servico.Descricao = string.IsNullOrWhiteSpace(servico.Descricao) ? null : servico.Descricao.Trim();
        servico.DataCriacao = servico.DataCriacao == default ? DateTime.Now : servico.DataCriacao;
        servico.Ativo = true;

        _servicoRepository.Insert(servico);
    }

    public void AlterarStatus(int id, bool ativo)
    {
        var servico = _servicoRepository.FindById(id);
        servico.Ativo = ativo;
        _servicoRepository.Update(servico);
    }
}