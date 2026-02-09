using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Configuration.Constants;
using WebApplication2.Core.Common;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.Models;
using WebApplication2.Core.Requests.Parciales;
using WebApplication2.Core.Requests.PeriodoAcademico;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = $"{Rol.ADMIN},{Rol.CONTROL_ESCOLAR},{Rol.DIRECTOR},{Rol.COORDINADOR},{Rol.DOCENTE}")]
    public class ParcialesController : ControllerBase
    {
        private readonly IParcialesService _ParcialesService;
        private readonly IMapper _mapper;

        public ParcialesController(IParcialesService parcialesService, IMapper mapper)
        {
            _ParcialesService = parcialesService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<ParcialesDto>>> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 100)
        {
            var pagination = await _ParcialesService.GetParciales(page, pageSize);

            var ParcialesDto = _mapper.Map<IEnumerable<ParcialesDto>>(pagination.Items);

            var response = new PagedResult<ParcialesDto>
            {
                TotalItems = pagination.TotalItems,
                Items = [.. ParcialesDto],
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize
            };

            return Ok(response);
        }

        [HttpPost]
        public async Task<ActionResult<ParcialesDto>> Post([FromBody] ParcialesRequest request)
        {
            var parciales = _mapper.Map<Parciales>(request);

            await _ParcialesService.CrearParciales(parciales);

            var ParcialesDto = _mapper.Map<ParcialesDto>(parciales);

            return Ok(ParcialesDto);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] ParcialesUpdateRequest request)
        {
            try
            {
                var newParcial = _mapper.Map<Parciales>(request);

                var parcial = await _ParcialesService.ActualizarParciales(newParcial);

                var parcialesDto = _mapper.Map<ParcialesDto>(parcial);

                return Ok(parcialesDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
