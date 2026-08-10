using App.Domain.DTO;
using App.Domain.Interfaces.Application;
using Microsoft.AspNetCore.Mvc;

namespace App.Api.Controllers;

[Route("Funcionarios")]
public class FuncionariosController : ControllerBase
{
    private readonly IFuncionariosService _funcionariosService;

    public FuncionariosController(IFuncionariosService funcionariosService)
    {
        _funcionariosService = funcionariosService;
    }

    [HttpGet("Listar")]
    public IActionResult Listar()
    {
        var registros = _funcionariosService.Listar();
        return Ok(registros);
    }

    [HttpGet("ListarAtivos")]
    public IActionResult ListarAtivos()
    {
        var registros = _funcionariosService.ListarAtivos();
        return Ok(registros);
    }

    [HttpPost("Cadastrar")]
    public IActionResult Cadastrar([FromBody] CadastrarFuncionarioRequestDTO request)
    {
        _funcionariosService.Cadastrar(request);
        return Ok("Funcionário cadastrado com sucesso.");
    }

    [HttpPost("AlterarStatus")]
    public IActionResult AlterarStatus([FromQuery] int id, [FromQuery] bool ativo)
    {
        _funcionariosService.AlterarStatus(id, ativo);
        return Ok("Status do funcionário atualizado.");
    }
}
