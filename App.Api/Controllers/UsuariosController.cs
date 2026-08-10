using App.Domain.DTO;
using App.Domain.Interfaces.Application;
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
        _usuariosService.Cadastrar(request);
        return Ok("Cadastro realizado com sucesso!");
    }

    [HttpPost("Logar")]
    public IActionResult Logar([FromBody] LoginUsuarioRequestDTO request)
    {
        var usuario = _usuariosService.Logar(request);
        return Ok(usuario);
    }

    [HttpPost("LogarAdmin")]
    public IActionResult LogarAdmin([FromBody] LoginUsuarioRequestDTO request)
    {
        var usuario = _usuariosService.Logar(request);
        if (!usuario.IsAdmin)
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Usuário sem permissão para acessar o painel administrativo.");
        }

        return Ok(usuario);
    }

    [HttpDelete("Excluir")]
    public IActionResult Excluir([FromQuery] int id)
    {
        _usuariosService.Excluir(id);
        return Ok("Registro excluído com sucesso!");
    }

    [HttpPost("Editar")]
    public IActionResult Editar([FromBody] EditarUsuarioRequestDTO request)
    {
        _usuariosService.Editar(request);
        return Ok("Registro editado com sucesso!");
    }

    [HttpPost("UploadFotoPerfil")]
    public async Task<IActionResult> UploadFotoPerfil([FromQuery] int id, IFormFile file)
    {
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);

        var url = _usuariosService.AtualizarFotoPerfil(id, file.FileName, ms.ToArray());
        return Ok(new { url });
    }
}