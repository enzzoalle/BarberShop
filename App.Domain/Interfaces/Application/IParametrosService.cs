using App.Domain.DTO;

namespace App.Domain.Interfaces;

public interface IParametrosService
{
    ParametrosResponseDTO Obter();
    void Salvar(SalvarParametrosRequestDTO requestDto);
}