using App.Domain.DTO;
using App.Domain.Entities;
using App.Domain.Interfaces.Application;
using App.Domain.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;

namespace App.Application.Services;

public class HorariosFixosService : IHorariosFixosService
{
    private readonly IRepositoryBase<HorariosFixos> _horariosFixosRepository;
    private readonly IRepositoryBase<Usuarios> _usuariosRepository;
    private readonly IRepositoryBase<Servicos> _servicosRepository;

    public HorariosFixosService(
        IRepositoryBase<HorariosFixos> horariosFixosRepository,
        IRepositoryBase<Usuarios> usuariosRepository,
        IRepositoryBase<Servicos> servicosRepository
    )
    {
        _horariosFixosRepository = horariosFixosRepository;
        _usuariosRepository = usuariosRepository;
        _servicosRepository = servicosRepository;
    }

    public IEnumerable<HorarioFixoResponseDTO> ListarPorUsuario(int usuarioId)
    {
        return _horariosFixosRepository.Query(h => h.UsuarioId == usuarioId)
            .Include(h => h.Servico)
            .Select(h => new HorarioFixoResponseDTO
            {
                Id = h.Id,
                DiaDaSemana = (int)h.DiaDaSemana,
                Horario = h.Horario.ToString(@"hh\:mm"),
                RepetirACadaSemanas = h.RepetirACadaSemanas,
                Servico = h.Servico.Nome
            })
            .ToList();
    }

    public void Incluir(IncluirHorarioFixoRequestDTO request)
    {
        if (request.RepetirACadaSemanas < 1)
        {
            throw new InvalidOperationException("Informe a cada quantas semanas o horário deve se repetir (mínimo 1).");
        }

        _ = _usuariosRepository.FindById(request.UsuarioId)
            ?? throw new InvalidOperationException("Usuário não encontrado.");

        _ = _servicosRepository.FindById(request.ServicoId)
            ?? throw new InvalidOperationException("Serviço não encontrado.");

        var jaExiste = _horariosFixosRepository
            .Query(h => h.UsuarioId == request.UsuarioId
                        && h.ServicoId == request.ServicoId
                        && h.DiaDaSemana == request.DiaDaSemana
                        && h.Horario == request.Horario)
            .Any();

        if (jaExiste)
        {
            throw new InvalidOperationException("Você já possui um horário fixo cadastrado para esse dia e horário.");
        }

        var novo = new HorariosFixos
        {
            UsuarioId = request.UsuarioId,
            ServicoId = request.ServicoId,
            DiaDaSemana = request.DiaDaSemana,
            Horario = request.Horario,
            RepetirACadaSemanas = request.RepetirACadaSemanas,
            DataCriacao = DateTime.Now
        };

        _horariosFixosRepository.Insert(novo);
    }

    public void Excluir(int id)
    {
        var horario = _horariosFixosRepository.FindById(id);
        if (horario != null)
        {
            _horariosFixosRepository.Remove(horario);
        }
    }
}
