using Microsoft.EntityFrameworkCore;
using WebApplication2.Core.Common;
using WebApplication2.Core.DTOs.Profesor;
using WebApplication2.Core.Models;
using WebApplication2.Core.Responses.Profesor;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Services
{
    public class ProfesorService : IProfesorService
    {
        private readonly ApplicationDbContext _dbContext;

        public ProfesorService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PagedResult<Profesor>> GetAllProfesores(int page, int pageSize)
        {
            var query = _dbContext.Profesor
                .Include(d => d.IdPersonaNavigation)
                    .ThenInclude(p => p.IdGeneroNavigation)
                .Include(d => d.IdPersonaNavigation)
                    .ThenInclude(p => p.IdDireccionNavigation)
                .Include(d => d.IdPersonaNavigation)
                    .ThenInclude(p => p.IdEstadoCivilNavigation)
                .Where(p => p.Status == Core.Enums.StatusEnum.Active);

            var totalItems = await query.CountAsync();

            var profesores = await query
                .OrderBy(p => p.IdPersonaNavigation.ApellidoPaterno)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Profesor>
            {
                TotalItems = totalItems,
                Items = profesores,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        public async Task<PagedResult<Profesor>> GetProfesores(int campusId, int page, int pageSize)
        {
            var totalItems = await _dbContext.Profesor
                .Include(d => d.IdPersonaNavigation)
                .ThenInclude(p => p.IdGeneroNavigation)
                .Where(p => p.Status == Core.Enums.StatusEnum.Active && (p.CampusId == campusId || p.CampusId == null))
                .CountAsync();

            var profesores = await _dbContext.Profesor
                .Include(d => d.IdPersonaNavigation)
                .ThenInclude(p => p.IdGeneroNavigation)
                .Where(p => p.Status == Core.Enums.StatusEnum.Active && (p.CampusId == campusId || p.CampusId == null))
                .OrderBy(p => p.IdPersonaNavigation.ApellidoPaterno)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Profesor>
            {
                TotalItems = totalItems,
                Items = profesores,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        public async Task<Profesor> CrearProfesor(Profesor profesor)
        {
            await _dbContext.AddAsync(profesor);
            await _dbContext.SaveChangesAsync();

            return profesor;
        }

        public async Task<Profesor> ActualizarProfesor(Profesor newProfesor)
        {
            var profesor = await _dbContext.Profesor
                .Include(d => d.IdPersonaNavigation)
                .SingleOrDefaultAsync(p => p.IdProfesor == newProfesor.IdProfesor);

            if (profesor == null)
            {
                throw new Exception("No existe profesor con el id ingresado");
            }

            if (newProfesor.IdPersonaNavigation != null)
            {
                var persona = await _dbContext.Persona.SingleOrDefaultAsync(p => p.IdPersona == profesor.IdPersona);

                persona.Nombre = newProfesor.IdPersonaNavigation.Nombre;
                persona.ApellidoPaterno = newProfesor.IdPersonaNavigation.ApellidoPaterno;
                persona.ApellidoMaterno = newProfesor.IdPersonaNavigation.ApellidoMaterno;
                persona.FechaNacimiento = newProfesor.IdPersonaNavigation.FechaNacimiento;
                persona.IdGenero = newProfesor.IdPersonaNavigation.IdGenero;
                persona.Curp = newProfesor.IdPersonaNavigation.Curp;
                persona.Correo = newProfesor.IdPersonaNavigation.Correo;
                persona.Telefono = newProfesor.IdPersonaNavigation.Telefono;

                _dbContext.Persona.Update(persona);
            }

            if (newProfesor.IdPersonaNavigation != null && newProfesor.IdPersonaNavigation.IdDireccionNavigation != null)
            {
                var direccion = await _dbContext.Direccion.SingleOrDefaultAsync(d => d.IdDireccion == profesor.IdPersonaNavigation.IdDireccion);

                direccion.Calle = newProfesor.IdPersonaNavigation.IdDireccionNavigation.Calle;
                direccion.NumeroExterior = newProfesor.IdPersonaNavigation.IdDireccionNavigation.NumeroExterior;
                direccion.NumeroInterior = newProfesor.IdPersonaNavigation.IdDireccionNavigation.NumeroInterior;
                direccion.CodigoPostalId = newProfesor.IdPersonaNavigation.IdDireccionNavigation.CodigoPostalId;

                _dbContext.Direccion.Update(direccion);
            }

            profesor.NoEmpleado = newProfesor.NoEmpleado;
            profesor.EmailInstitucional = newProfesor.EmailInstitucional;
            profesor.Status = newProfesor.Status;

            _dbContext.Profesor.Update(profesor);

            await _dbContext.SaveChangesAsync();

            return profesor;
        }

        public async Task<ValidarHorarioProfesorResponse> ValidarConflictosHorarioAsync(
            int idProfesor,
            List<HorarioValidacionDto> horariosNuevos,
            int? idGrupoMateriaActual = null,
            CancellationToken ct = default)
        {
            var profesor = await _dbContext.Profesor
                .FirstOrDefaultAsync(p => p.IdProfesor == idProfesor && p.Status == Core.Enums.StatusEnum.Active, ct);

            if (profesor == null)
                throw new KeyNotFoundException($"Profesor con ID {idProfesor} no encontrado");

            var conflictos = new List<ConflictoHorario>();

            var materiasProfesor = await _dbContext.GrupoMateria
                .Include(gm => gm.IdMateriaPlanNavigation)
                    .ThenInclude(mp => mp.IdMateriaNavigation)
                .Include(gm => gm.IdGrupoNavigation)
                .Include(gm => gm.Horario)
                    .ThenInclude(h => h.IdDiaSemanaNavigation)
                .Where(gm => gm.IdProfesor == idProfesor && gm.Status == Core.Enums.StatusEnum.Active)
                .Where(gm => idGrupoMateriaActual == null || gm.IdGrupoMateria != idGrupoMateriaActual)
                .ToListAsync(ct);

            foreach (var horarioPropuesto in horariosNuevos)
            {
                foreach (var materiaExistente in materiasProfesor)
                {
                    foreach (var horarioExistente in materiaExistente.Horario)
                    {
                        if (horarioPropuesto.Dia == horarioExistente.IdDiaSemanaNavigation.Nombre)
                        {
                            var propuestoInicio = TimeOnly.Parse(horarioPropuesto.HoraInicio);
                            var propuestoFin = TimeOnly.Parse(horarioPropuesto.HoraFin);
                            var existenteInicio = horarioExistente.HoraInicio;
                            var existenteFin = horarioExistente.HoraFin;

                            bool hayTraslape =
                                (propuestoInicio < existenteFin && propuestoFin > existenteInicio);

                            if (hayTraslape)
                            {
                                conflictos.Add(new ConflictoHorario
                                {
                                    Dia = horarioExistente.IdDiaSemanaNavigation.Nombre,
                                    HoraInicio = existenteInicio.ToString("HH:mm"),
                                    HoraFin = existenteFin.ToString("HH:mm"),
                                    NombreMateria = materiaExistente.IdMateriaPlanNavigation?.IdMateriaNavigation?.Nombre ?? "",
                                    Grupo = materiaExistente.IdGrupoNavigation?.CodigoGrupo ?? "",
                                    Aula = horarioExistente.Aula ?? ""
                                });
                            }
                        }
                    }
                }
            }

            return new ValidarHorarioProfesorResponse
            {
                TieneConflicto = conflictos.Any(),
                Conflictos = conflictos
            };
        }
    }
}
