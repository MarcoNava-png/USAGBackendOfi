//using AutoMapper;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using WebApplication2.Configuration.Constants;
//using WebApplication2.Core.Common;
//using WebApplication2.Core.DTOs;
//using WebApplication2.Core.Models;
//using WebApplication2.Core.Requests.Auth;
//using WebApplication2.Services.Interfaces;

//namespace WebApplication2.Controllers
//{
//    [Route("api/coordinadores")]
//    [ApiController]
//    public class CoordinadorController : ControllerBase
//    {
//        private readonly ICoordinadorService _coordinadorService;
//        private readonly IAuthService _authService;
//        private readonly IMapper _mapper;

//        public CoordinadorController(ICoordinadorService coordinadorService, IAuthService authService, IMapper mapper)
//        {
//            _coordinadorService = coordinadorService;
//            _authService = authService;
//            _mapper = mapper;
//        }

//        [HttpGet]
//        public async Task<ActionResult<PagedResult<CoordinadorDto>>> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
//        {
//            var pagination = await _coordinadorService.GetCoordinadores(page, pageSize);

//            var coordinadoresDto = _mapper.Map<IEnumerable<CoordinadorDto>>(pagination.Items);

//            var response = new PagedResult<CoordinadorDto>
//            {
//                TotalItems = pagination.TotalItems,
//                Items = [.. coordinadoresDto],
//                PageNumber = pagination.PageNumber,
//                PageSize = pagination.PageSize
//            };

//            return Ok(response);
//        }

//        [HttpPost]
//        public async Task<ActionResult<CoordinadorDto>> Post([FromBody] CoordinadorSignupRequest request)
//        {
//            var user = new IdentityUser
//            {
//                UserName = request.Email,
//                Email = request.Email,
//            };

//            try
//            {
//                var signupResponse = await _authService.Signup(user, request.Password, [Rol.COORDINADOR]);

//                var newCoordinador = new Coordinador
//                {
//                    Persona = new Persona
//                    {
//                        Nombre = request.Nombre,
//                        ApellidoPaterno = request.ApellidoPaterno,
//                        ApellidoMaterno = request.ApellidoMaterno,
//                        FechaNacimiento = request.FechaNacimiento,
//                        PersonaGeneroId = request.PersonaGeneroId,
//                        UserId = signupResponse.Id,
//                        Estatus = StatusEnum.Activo,
//                        Direccion = new Direccion
//                        {
//                            Calle = request.Calle,
//                            Numero = request.Numero,
//                            CodigoPostalId = request.CodigoPostalId,
//                        }
//                    }
//                };

//                var coordinador = await _coordinadorService.CrearCoordinador(newCoordinador);

//                var coordinadorDto = _mapper.Map<CoordinadorDto>(coordinador);

//                return Ok(coordinadorDto);
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, ex.Message);
//            }
//        }

//        [HttpDelete("{id}")]
//        public async Task<IActionResult> Delete(int id)
//        {
//            try
//            {
//                await _coordinadorService.EliminarCoordinador(id);

//                return NoContent();
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, ex.Message);
//            }
//        }
//    }
//}
