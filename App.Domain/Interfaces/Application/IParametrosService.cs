using App.Domain.DTO;

namespace App.Domain.Interfaces;

public interface IParametrosService
{
    ParametrosResponse Obter();
    void Salvar(SalvarParametrosRequest request);
}