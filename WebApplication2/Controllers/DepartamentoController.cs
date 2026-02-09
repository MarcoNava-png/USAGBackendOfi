using Microsoft.AspNetCore.Mvc;
using WebApplication2.Core.Models;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Controllers
{
    [Route("api/departamentos")]
    [ApiController]
    public class DepartamentoController : ControllerBase
    {
        //private readonly IDepartamentoService _departamentoService;

        //public DepartamentoController(IDepartamentoService departamentoService)
        //{
        //    _departamentoService = departamentoService;
        //}

        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<Departamento>>> Get()
        //{
        //    var departamentos = await _departamentoService.GetDepartamentos();

        //    return Ok(departamentos);
        //}
    }
}
