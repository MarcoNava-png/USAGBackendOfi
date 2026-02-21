using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Configuration.Constants;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.Models;
using WebApplication2.Core.Requests.DocumentoRequisito;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DocumentoRequisitoController : ControllerBase
    {
        private readonly IDocumentoRequisitoService _service;
        private readonly IMapper _mapper;

        public DocumentoRequisitoController(IDocumentoRequisitoService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR},{Rol.COORDINADOR},{Rol.CONTROL_ESCOLAR},{Rol.ADMISIONES}")]
        public async Task<ActionResult<List<DocumentoRequisitoDto>>> GetAll()
        {
            var docs = await _service.GetAll();
            var dtos = _mapper.Map<List<DocumentoRequisitoDto>>(docs);
            return Ok(dtos);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR},{Rol.COORDINADOR},{Rol.CONTROL_ESCOLAR},{Rol.ADMISIONES}")]
        public async Task<ActionResult<DocumentoRequisitoDto>> GetById(int id)
        {
            var doc = await _service.GetById(id);
            if (doc == null)
                return NotFound(new { message = "Documento requisito no encontrado" });

            var dto = _mapper.Map<DocumentoRequisitoDto>(doc);
            return Ok(dto);
        }

        [HttpPost]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR}")]
        public async Task<ActionResult<DocumentoRequisitoDto>> Create([FromBody] DocumentoRequisitoRequest request)
        {
            try
            {
                var doc = new DocumentoRequisito
                {
                    Clave = request.Clave,
                    Descripcion = request.Descripcion,
                    EsObligatorio = request.EsObligatorio,
                    Orden = request.Orden
                };

                var created = await _service.Crear(doc);
                var dto = _mapper.Map<DocumentoRequisitoDto>(created);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR}")]
        public async Task<ActionResult<DocumentoRequisitoDto>> Update([FromBody] DocumentoRequisitoUpdateRequest request)
        {
            try
            {
                var doc = new DocumentoRequisito
                {
                    IdDocumentoRequisito = request.IdDocumentoRequisito,
                    Clave = request.Clave,
                    Descripcion = request.Descripcion,
                    EsObligatorio = request.EsObligatorio,
                    Orden = request.Orden,
                    Activo = request.Activo
                };

                var updated = await _service.Actualizar(doc);
                var dto = _mapper.Map<DocumentoRequisitoDto>(updated);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.Eliminar(id);
                return Ok(new { message = "Documento requisito eliminado correctamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/toggle")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR}")]
        public async Task<ActionResult<DocumentoRequisitoDto>> Toggle(int id)
        {
            try
            {
                var updated = await _service.ToggleActivo(id);
                var dto = _mapper.Map<DocumentoRequisitoDto>(updated);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
