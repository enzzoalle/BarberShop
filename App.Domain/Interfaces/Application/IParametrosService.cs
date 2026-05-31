using App.Domain.DTO;

namespace App.Domain.Interfaces.Application;

public interface IParametrosService
{
    ParametrosResponseDTO Obter();
    void Salvar(SalvarParametrosRequestDTO requestDto);
}