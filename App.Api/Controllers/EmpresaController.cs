using App.Domain.Entities;
using App.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace App.Api.Controllers;

[Route("Empresa")]
public class EmpresaController : ControllerBase
{
    private readonly IEmpresaService _empresaService;

    public EmpresaController(IEmpresaService empresaService)
    {
        _empresaService = empresaService;
    }

    // [HttpGet]
    // [Route("Listar")]
    // public IActionResult Listar()
    // {
    //     // var registros = _empresaService.Listar();
    //     // return Ok(registros);
    // }
}