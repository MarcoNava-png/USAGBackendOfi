using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Configuration.Constants;
using WebApplication2.Core.Common;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.Models;
using WebApplication2.Core.Requests.MateriaPlan;
using WebApplication2.Core.Requests.NewFolder;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = $"{Rol.ADMIN},{Rol.CONTROL_ESCOLAR},{Rol.DIRECTOR},{Rol.COORDINADOR},{Rol.ACADEMICO}")]
    public class MateriaPlanController : ControllerBase
    {
        private readonly IMateriaPlanService _materiaPlanService;
        private readonly IMapper _mapper;

        public MateriaPlanController(IMateriaPlanService materiaPlanService, IMapper mapper)
        {
            _materiaPlanService = materiaPlanService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 1000)
        {
            var pagination = await _materiaPlanService.GetMateriaPlanes(page, pageSize);

            var estudiantesDto = _mapper.Map<IEnumerable<MateriaPlanDto>>(pagination.Items);

            var response = new PagedResult<MateriaPlanDto>
            {
                TotalItems = pagination.TotalItems,
                Items = [.. estudiantesDto],
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize
            };

            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var materiaPlan = await _materiaPlanService.GetMateriaPlanDetalle(id);

            if (materiaPlan == null)
            {
                return NotFound();
            }

            return Ok(materiaPlan);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MateriaPlanRequest request)
        {
            var newMateriaPlan = _mapper.Map<MateriaPlan>(request);

            var materiaPlan = await _materiaPlanService.CrearMateriaPlan(newMateriaPlan);

            var materiaPlanDto = _mapper.Map<MateriaPlanDto>(materiaPlan);

            return CreatedAtAction(nameof(GetById), new { id = materiaPlanDto.IdMateriaPlan }, materiaPlanDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] MateriaPlanUpdateRequest request)
        {
            var newMateriaPlan = _mapper.Map<MateriaPlan>(request);

            try
            {
                var materiaPlan = await _materiaPlanService.ActualizarMateriaPlan(newMateriaPlan);

                var materiaPlanDto = _mapper.Map<MateriaPlanDto>(materiaPlan);

                return Ok(materiaPlanDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var (exito, mensaje) = await _materiaPlanService.EliminarMateriaPlan(id);

                if (!exito)
                    return BadRequest(new { message = mensaje });

                return Ok(new { message = mensaje });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("por-plan/{idPlanEstudios}")]
        public async Task<IActionResult> GetMateriasPorPlan(int idPlanEstudios)
        {
            try
            {
                var materias = await _materiaPlanService.GetMateriasPorPlanAsync(idPlanEstudios);
                var materiasDto = _mapper.Map<IEnumerable<MateriaPlanDto>>(materias);

                return Ok(new Response<IEnumerable<MateriaPlanDto>> { Data = materiasDto });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("importar")]
        public async Task<IActionResult> ImportarMaterias([FromBody] WebApplication2.Core.Requests.MateriaPlan.ImportarMateriasRequest request)
        {
            try
            {
                if (request.Materias == null || request.Materias.Count == 0)
                {
                    return BadRequest(new { message = "Debe proporcionar al menos una materia para importar" });
                }

                if (!request.IdPlanEstudios.HasValue && string.IsNullOrWhiteSpace(request.ClavePlanEstudios))
                {
                    return BadRequest(new { message = "Debe especificar IdPlanEstudios o ClavePlanEstudios" });
                }

                var resultado = await _materiaPlanService.ImportarMateriasAsync(request);

                if (!resultado.Exito)
                {
                    return BadRequest(new Response<WebApplication2.Core.Requests.MateriaPlan.ImportarMateriasResponse>
                    {
                        Data = resultado,
                        IsSuccess = false,
                        MessageError = resultado.Mensaje
                    });
                }

                return Ok(new Response<WebApplication2.Core.Requests.MateriaPlan.ImportarMateriasResponse>
                {
                    Data = resultado,
                    IsSuccess = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al importar materias", error = ex.Message });
            }
        }
    }
}
