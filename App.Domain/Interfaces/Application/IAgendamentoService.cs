using App.Domain.DTO;
using App.Domain.Entities;

namespace App.Domain.Interfaces;

public interface IAgendamentoService
{
    IEnumerable<Agendamento> Listar();
    IEnumerable<string> ListarHorariosDisponiveis(DateTime data, int servicoId);
    void Incluir(CriarAgendamentoRequest request);
}