using App.Domain.DTO;
using App.Domain.Interfaces;
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
    public IActionResult Salvar([FromBody] SalvarParametrosRequest request)
    {
        try
        {
            _parametrosService.Salvar(request);
            return Ok("Parâmetros atualizados com sucesso.");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}