using App.Domain.DTO;
using App.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace App.Api.Controllers;

[Route("Agendamento")]
public class AgendamentoController : ControllerBase
{
    private readonly IAgendamentoService _agendamentoService;

    public AgendamentoController(IAgendamentoService agendamentoService)
    {
        _agendamentoService = agendamentoService;
    }

    [HttpGet]
    [Route("Listar")]
    public IActionResult Listar()
    {
        var registros = _agendamentoService.Listar();
        return Ok(registros);
    }

    [HttpGet]
    [Route("ListarHorariosDisponiveis")]
    public IActionResult ListarHorariosDisponiveis(DateTime data, int servicoId)
    {
        var horarios = _agendamentoService.ListarHorariosDisponiveis(data, servicoId);
        return Ok(horarios);
    }

    [HttpPost]
    [Route("Incluir")]
    public IActionResult Incluir([FromBody] CriarAgendamentoRequest request)
    {
        _agendamentoService.Incluir(request);
        return Ok("Agendamento realizado com sucesso!");
    }
}