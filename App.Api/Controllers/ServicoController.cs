using App.Domain.Interfaces;
using App.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace App.Api.Controllers;

[Route("Servico")]
public class ServicoController : ControllerBase
{
    private readonly IServicoService _servicoService;

    public ServicoController(IServicoService servicoService)
    {
        _servicoService = servicoService;
    }

    [HttpGet]
    [Route("Listar")]
    public IActionResult Listar()
    {
        var registros = _servicoService.Listar();
        return Ok(registros);
    }

    [HttpGet]
    [Route("ListarTodos")]
    public IActionResult ListarTodos()
    {
        var registros = _servicoService.ListarTodos();
        return Ok(registros);
    }

    [HttpPost]
    [Route("Incluir")]
    public IActionResult Incluir([FromBody] Servico servico)
    {
        try
        {
            _servicoService.Incluir(servico);
            return Ok("Serviço cadastrado com sucesso.");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    [Route("AlterarStatus")]
    public IActionResult AlterarStatus(int id, bool ativo)
    {
        _servicoService.AlterarStatus(id, ativo);
        return Ok("Status do serviço atualizado.");
    }
}