using App.Domain.DTO;
using App.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace App.Api.Controllers;

[Route("Servicos")]
public class ServicosController : ControllerBase
{
    private readonly IServicosService _servicosService;

    public ServicosController(IServicosService servicosService)
    {
        _servicosService = servicosService;
    }

    [HttpGet("Listar")]
    public IActionResult Listar()
    {
        var registros = _servicosService.Listar();
        return Ok(registros);
    }

    [HttpGet("ListarTodos")]
    public IActionResult ListarTodos()
    {
        var registros = _servicosService.ListarTodos();
        return Ok(registros);
    }

    [HttpPost("Incluir")]
    public IActionResult Incluir([FromBody] CriarServicoRequestDTO request)
    {
        try
        {
            _servicosService.Incluir(request);
            return Ok("Serviço cadastrado com sucesso.");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("AlterarStatus")]
    public IActionResult AlterarStatus([FromQuery] int id, [FromQuery] bool ativo)
    {
        try
        {
            _servicosService.AlterarStatus(id, ativo);
            return Ok("Status do serviço atualizado.");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}