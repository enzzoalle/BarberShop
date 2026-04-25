using App.Domain.DTO;

namespace App.Domain.Interfaces;

public interface IAgendamentosService
{
    IEnumerable<object> Listar();
    IEnumerable<string> ListarHorariosDisponiveis(DateTime data, int servicoId);
    void Incluir(CriarAgendamentoRequest request);
    void IncluirManual(CriarAgendamentoManualRequestDTO requestDto);
    string AprovarSolicitacao(int id);
}