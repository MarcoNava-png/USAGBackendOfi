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
//    [Route("api/directores")]
//    [ApiController]
//    public class DirectorController : ControllerBase
//    {
//        private readonly IDirectorService _directorService;
//        private readonly IAuthService _authService;
//        private readonly IMapper _mapper;

//        public DirectorController(IDirectorService directorService, IAuthService authService, IMapper mapper)
//        {
//            _directorService = directorService;
//            _authService = authService;
//            _mapper = mapper;
//        }

//        [HttpGet]
//        public async Task<ActionResult<PagedResult<DirectorDto>>> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
//        {
//            var pagination = await _directorService.GetDirectores(page, pageSize);

//            var directoresDto = _mapper.Map<IEnumerable<DirectorDto>>(pagination.Items);

//            var response = new PagedResult<DirectorDto>
//            {
//                TotalItems = pagination.TotalItems,
//                Items = [.. directoresDto],
//                PageNumber = pagination.PageNumber,
//                PageSize = pagination.PageSize
//            };

//            return Ok(response);
//        }

//        [HttpPost]
//        public async Task<ActionResult<DirectorDto>> Post([FromBody] DirectorSignupRequest request)
//        {
//            var user = new IdentityUser
//            {
//                UserName = request.Email,
//                Email = request.Email,
//            };

//            try
//            {
//                var signupResponse = await _authService.Signup(user, request.Password, [Rol.DIRECTOR]);

//                var newDirector = new Director
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

//                var director = await _directorService.CrearDirector(newDirector);

//                var directorDto = _mapper.Map<DirectorDto>(director);

//                return Ok(directorDto);
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
//                await _directorService.EliminarDirector(id);

//                return NoContent();
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, ex.Message);
//            }
//        }
//    }
//}
