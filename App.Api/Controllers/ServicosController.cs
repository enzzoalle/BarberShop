using App.Domain.DTO;
using App.Domain.Interfaces.Application;
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

    [HttpGet("ListarAtivos")]
    public IActionResult ListarAtivos()
    {
        var registros = _servicosService.ListarAtivos();
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
        _servicosService.Incluir(request);
        return Ok("Serviço cadastrado com sucesso.");
    }

    [HttpPost("AlterarStatus")]
    public IActionResult AlterarStatus([FromQuery] int id, [FromQuery] bool ativo)
    {
        _servicosService.AlterarStatus(id, ativo);
        return Ok("Status do serviço atualizado.");
    }
}