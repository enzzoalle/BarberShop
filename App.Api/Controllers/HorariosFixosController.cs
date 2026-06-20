using App.Domain.DTO;
using App.Domain.Interfaces.Application;
using Microsoft.AspNetCore.Mvc;

namespace App.Api.Controllers;

[Route("HorariosFixos")]
public class HorariosFixosController : ControllerBase
{
    private readonly IHorariosFixosService _horariosFixosService;

    public HorariosFixosController(IHorariosFixosService horariosFixosService)
    {
        _horariosFixosService = horariosFixosService;
    }

    [HttpGet("ListarPorUsuario")]
    public IActionResult ListarPorUsuario([FromQuery] int usuarioId)
    {
        var horarios = _horariosFixosService.ListarPorUsuario(usuarioId);
        return Ok(horarios);
    }

    [HttpPost("Incluir")]
    public IActionResult Incluir([FromBody] IncluirHorarioFixoRequestDTO request)
    {
        try
        {
            _horariosFixosService.Incluir(request);
            return Ok(new { mensagem = "Incluído com sucesso." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("Excluir")]
    public IActionResult Excluir([FromQuery] int id)
    {
        _horariosFixosService.Excluir(id);
        return Ok();
    }
}
