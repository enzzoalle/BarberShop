using App.Domain.DTO;
using App.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace App.Api.Controllers;

[Route("Usuarios")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuariosService _usuariosService;

    public UsuariosController(IUsuariosService usuariosService)
    {
        _usuariosService = usuariosService;
    }

    [HttpPost("Cadastrar")]
    public IActionResult Cadastrar([FromBody] CadastrarUsuarioRequestDTO request)
    {
        try
        {
            _usuariosService.Cadastrar(request);
            return Ok("Cadastro realizado com sucesso!");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("Logar")]
    public IActionResult Logar([FromBody] LoginUsuarioRequestDTO request)
    {
        try
        {
            var usuario = _usuariosService.Logar(request);
            return Ok(usuario);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    [HttpPost("LogarAdmin")]
    public IActionResult LogarAdmin([FromBody] LoginUsuarioRequestDTO request)
    {
        try
        {
            var usuario = _usuariosService.Logar(request);
            if (!usuario.IsAdmin)
            {
                return StatusCode(StatusCodes.Status403Forbidden, "Usuário sem permissão para acessar o painel administrativo.");
            }

            return Ok(usuario);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    [HttpDelete("Excluir")]
    public IActionResult Excluir([FromQuery] int id)
    {
        try
        {
            _usuariosService.Excluir(id);
            return Ok("Registro excluído com sucesso!");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("Editar")]
    public IActionResult Editar([FromBody] EditarUsuarioRequestDTO request)
    {
        try
        {
            _usuariosService.Editar(request);
            return Ok("Registro editado com sucesso!");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}