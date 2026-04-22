using App.Domain.DTO;
using App.Domain.Entities;
using App.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace App.Api.Controllers;

[Route("Usuario")]
public class UsuarioController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;

    public UsuarioController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    [HttpGet]
    [Route("Listar")]
    public IActionResult Listar()
    {
        var registros = _usuarioService.Listar();
        return Ok(registros);
    }

    [HttpPost]
    [Route("Incluir")]
    public IActionResult Incluir([FromBody] Usuario usuario)
    {
        try
        {
            _usuarioService.Incluir(usuario);
            return Ok("Registro incluído com sucesso!");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    [Route("Cadastrar")]
    public IActionResult Cadastrar([FromBody] CadastrarUsuarioRequest request)
    {
        try
        {
            _usuarioService.Cadastrar(request);
            return Ok("Cadastro realizado com sucesso!");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    [Route("Logar")]
    public IActionResult Logar([FromBody] LoginUsuarioRequest request)
    {
        try
        {
            var usuario = _usuarioService.Logar(request);
            return Ok(usuario);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    [HttpPost]
    [Route("LogarAdmin")]
    public IActionResult LogarAdmin([FromBody] LoginUsuarioRequest request)
    {
        try
        {
            var usuario = _usuarioService.Logar(request);
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

    [HttpPost]
    [Route("Excluir")]
    public IActionResult Excluir(int id)
    {
        _usuarioService.Excluir(id);
        return Ok("Registro excluído com sucesso!");
    }

    [HttpPost]
    [Route("Editar")]
    public IActionResult Editar([FromBody] Usuario usuario)
    {
        _usuarioService.Editar(usuario);
        return Ok("Registro editado com sucesso!");
    }
}