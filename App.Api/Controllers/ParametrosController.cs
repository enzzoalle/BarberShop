using App.Domain.DTO;
using App.Domain.Interfaces.Application;
using Microsoft.AspNetCore.Mvc;

namespace App.Api.Controllers;

[Route("Parametros")]
public class ParametrosController : ControllerBase
{
    private readonly IParametrosService _parametrosService;

    public ParametrosController(IParametrosService parametrosService)
    {
        _parametrosService = parametrosService;
    }

    [HttpGet("Obter")]
    public IActionResult Obter()
    {
        var parametros = _parametrosService.Obter();
        return Ok(parametros);
    }

    [HttpPost("Salvar")]
    public IActionResult Salvar([FromBody] SalvarParametrosRequestDTO requestDto)
    {
        _parametrosService.Salvar(requestDto);
        return Ok("Parâmetros atualizados com sucesso.");
    }
}