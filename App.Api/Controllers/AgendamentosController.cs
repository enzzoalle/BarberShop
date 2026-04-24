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
    public IActionResult ListarHorariosDisponiveis([FromQuery] DateTime data, [FromQuery] int servicoId)
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
            return Ok("Solicitação de agendamento enviada com sucesso!");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("IncluirManual")]
    public IActionResult IncluirManual([FromBody] CriarAgendamentoManualRequestDTO requestDto)
    {
        try
        {
            _agendamentosService.IncluirManual(requestDto);
            return Ok("Agendamento manual realizado com sucesso!");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("AprovarSolicitacao")]
    public IActionResult AprovarSolicitacao([FromQuery] int id)
    {
        try
        {
            var linkWhatsapp = _agendamentosService.AprovarSolicitacao(id);
            return Ok(new { url = linkWhatsapp });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}