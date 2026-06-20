using App.Domain.DTO;

namespace App.Domain.Interfaces.Application;

public interface IHorariosFixosService
{
    IEnumerable<HorarioFixoResponseDTO> ListarPorUsuario(int usuarioId);
    void Incluir(IncluirHorarioFixoRequestDTO request);
    void Excluir(int id);
}
