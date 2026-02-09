using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Configuration.Constants;
using WebApplication2.Core.Common;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.Models;
using WebApplication2.Services;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = $"{Rol.ADMIN},{Rol.CONTROL_ESCOLAR}")]
    public class BitacoraController : ControllerBase
    {
        private readonly IBitacoraService _svc;
        private readonly IMapper _mapper;
        
        public BitacoraController(IBitacoraService svc, IMapper mapper)
        {
            _svc = svc;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<long>> Agregar([FromBody] BitacoraCreateDto dto, CancellationToken ct)
        {
            var id = await _svc.AgregarAsync(dto, ct);
            return Ok(id);
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<BitacoraDto>>> GetBitacora(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string filter = "")
        {
            var result = await _svc.GetBitacora(page, pageSize, filter);
            return Ok(result);
        }
    }
}
