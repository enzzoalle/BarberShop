using App.Domain.DTO;
using App.Domain.Entities;

namespace App.Domain.Interfaces;

public interface IAgendamentosService
{
    IEnumerable<Agendamentos> Listar();
    IEnumerable<string> ListarHorariosDisponiveis(DateTime data, int servicoId);
    void Incluir(CriarAgendamentoRequest request);
    void IncluirManual(CriarAgendamentoManualRequestDTO requestDto);
    string AprovarSolicitacao(int id);
}