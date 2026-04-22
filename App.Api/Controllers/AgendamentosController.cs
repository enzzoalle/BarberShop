using App.Domain.DTO;
using App.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace App.Api.Controllers;

[Route("Agendamentos")]
public class AgendamentosController : ControllerBase
{
    private readonly IAgendamentosService _agendamentosService;

    public AgendamentosController(IAgendamentosService agendamentosService)
    {
        _agendamentosService = agendamentosService;
    }

    [HttpGet("Listar")]
    public IActionResult Listar()
    {
        var registros = _agendamentosService.Listar();
        return Ok(registros);
    }

    [HttpGet("ListarHorariosDisponiveis")]
    public IActionResult ListarHorariosDisponiveis(DateTime data, int servicoId)
    {
        var horarios = _agendamentosService.ListarHorariosDisponiveis(data, servicoId);
        return Ok(horarios);
    }

    [HttpPost("Incluir")]
    public IActionResult Incluir([FromBody] CriarAgendamentoRequest request)
    {
        try
        {
            _agendamentosService.Incluir(request);
            return Ok("Agendamentos realizado com sucesso!");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("IncluirManual")]
    public IActionResult IncluirManual([FromBody] CriarAgendamentoManualRequest request)
    {
        try
        {
            _agendamentosService.IncluirManual(request);
            return Ok("Agendamentos manual realizado com sucesso!");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}