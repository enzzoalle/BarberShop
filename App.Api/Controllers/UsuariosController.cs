using App.Domain.DTO;
using App.Domain.Entities;
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

    [HttpGet("Listar")]
    public IActionResult Listar()
    {
        var registros = _usuariosService.Listar();
        return Ok(registros);
    }

    [HttpPost("Incluir")]
    public IActionResult Incluir([FromBody] Usuarios usuarios)
    {
        try
        {
            _usuariosService.Incluir(usuarios);
            return Ok("Registro incluído com sucesso!");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("Cadastrar")]
    public IActionResult Cadastrar([FromBody] CadastrarUsuarioRequest request)
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
    public IActionResult Logar([FromBody] LoginUsuarioRequest request)
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
    public IActionResult LogarAdmin([FromBody] LoginUsuarioRequest request)
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

    [HttpPost("Excluir")]
    public IActionResult Excluir(int id)
    {
        _usuariosService.Excluir(id);
        return Ok("Registro excluído com sucesso!");
    }

    [HttpPost("Editar")]
    public IActionResult Editar([FromBody] Usuarios usuarios)
    {
        _usuariosService.Editar(usuarios);
        return Ok("Registro editado com sucesso!");
    }
}