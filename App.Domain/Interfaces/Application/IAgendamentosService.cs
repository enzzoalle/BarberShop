using App.Domain.DTO;

namespace App.Domain.Interfaces.Application;

public interface IAgendamentosService
{
    IEnumerable<object> Listar();
    IEnumerable<AgendamentoDashboardDiaDTO> ObterDashboardUltimos7Dias(int? funcionarioId);
    IEnumerable<string> ListarHorariosDisponiveis(DateTime data, int servicoId, int funcionarioId);
    void Incluir(CriarAgendamentoRequest request);
    void IncluirManual(CriarAgendamentoManualRequestDTO requestDto);
    string AprovarSolicitacao(int id);
}