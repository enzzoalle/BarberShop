using App.Domain.DTO;
using App.Domain.Interfaces.Application;
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
    public IActionResult ListarHorariosDisponiveis([FromQuery] DateTime data, [FromQuery] int servicoId, [FromQuery] int funcionarioId)
    {
        var horarios = _agendamentosService.ListarHorariosDisponiveis(data, servicoId, funcionarioId);
        return Ok(horarios);
    }

    [HttpGet("DashboardUltimos7Dias")]
    public IActionResult DashboardUltimos7Dias([FromQuery] int? funcionarioId)
    {
        var dashboard = _agendamentosService.ObterDashboardUltimos7Dias(funcionarioId);
        return Ok(dashboard);
    }

    [HttpPost("Incluir")]
    public IActionResult Incluir([FromBody] CriarAgendamentoRequest request)
    {
        _agendamentosService.Incluir(request);
        return Ok(new { mensagem = "Solicitação de agendamento enviada com sucesso!" });
    }

    [HttpPost("IncluirManual")]
    public IActionResult IncluirManual([FromBody] CriarAgendamentoManualRequestDTO requestDto)
    {
        _agendamentosService.IncluirManual(requestDto);
        return Ok("Agendamento manual realizado com sucesso!");
    }

    [HttpPost("AprovarSolicitacao")]
    public IActionResult AprovarSolicitacao([FromQuery] int id)
    {
        var linkWhatsapp = _agendamentosService.AprovarSolicitacao(id);
        return Ok(new { url = linkWhatsapp });
    }
}