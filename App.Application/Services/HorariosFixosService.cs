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
        IRepositoryBase<Servicos> servicosRepository)
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
        var usuario = _usuariosRepository.FindById(request.UsuarioId);
        if (usuario == null)
        {
            throw new InvalidOperationException("Usuário não encontrado.");
        }

        var servico = _servicosRepository.FindById(request.ServicoId);
        if (servico == null)
        {
            throw new InvalidOperationException("Serviço não encontrado.");
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
