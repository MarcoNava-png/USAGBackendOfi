using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.RegularExpressions;
using WebApplication2.Core.DTOs.Importacion;
using WebApplication2.Core.Requests.Importacion;
using WebApplication2.Core.Responses.Importacion;
using WebApplication2.Core.Models;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Services
{
    public class ImportacionService : IImportacionService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMatriculaService _matriculaService;

        public ImportacionService(ApplicationDbContext db, IMatriculaService matriculaService)
        {
            _db = db;
            _matriculaService = matriculaService;
        }

        public async Task<List<string>> GetCampusDisponiblesAsync()
        {
            return await _db.Campus
                .Where(c => c.Activo)
                .Select(c => c.Nombre)
                .ToListAsync();
        }

        public async Task<List<string>> GetPlanesDisponiblesAsync()
        {
            return await _db.PlanEstudios
                .Select(p => p.NombrePlanEstudios ?? p.ClavePlanEstudios)
                .Distinct()
                .ToListAsync();
        }

        public async Task<ValidarImportacionResponse> ValidarImportacionAsync(ValidarImportacionRequest request)
        {
            var response = new ValidarImportacionResponse
            {
                TotalRegistros = request.Estudiantes.Count
            };

            var campusExistentes = await _db.Campus
                .Where(c => c.Activo)
                .Select(c => c.Nombre.ToLower())
                .ToListAsync();

            var planesExistentes = await _db.PlanEstudios
                .Select(p => (p.NombrePlanEstudios ?? p.ClavePlanEstudios).ToLower())
                .ToListAsync();

            var matriculasExistentes = await _db.Estudiante
                .Select(e => e.Matricula.ToLower())
                .ToListAsync();

            var campusEnArchivo = new HashSet<string>();
            var cursosEnArchivo = new HashSet<string>();
            var matriculasEnArchivo = new HashSet<string>();
            var matriculasDuplicadasEnArchivo = new List<string>();

            int fila = 1;
            foreach (var est in request.Estudiantes)
            {
                var resultado = new ResultadoImportacionEstudiante
                {
                    Fila = fila++,
                    Matricula = est.Matricula,
                    NombreCompleto = $"{est.Nombre} {est.ApellidoPaterno} {est.ApellidoMaterno}".Trim()
                };

                var errores = new List<string>();

                if (string.IsNullOrWhiteSpace(est.Matricula))
                    errores.Add("Matricula es requerida");

                if (string.IsNullOrWhiteSpace(est.Nombre))
                    errores.Add("Nombre es requerido");

                if (string.IsNullOrWhiteSpace(est.ApellidoPaterno))
                    errores.Add("Apellido paterno es requerido");

                if (string.IsNullOrWhiteSpace(est.Campus))
                    errores.Add("Campus es requerido");

                if (string.IsNullOrWhiteSpace(est.Curso))
                    errores.Add("Curso/Carrera es requerido");

                if (!string.IsNullOrWhiteSpace(est.Grupo))
                {
                    var (turno, numGrupo) = ParseGrupo(est.Grupo);
                    if (turno == 0 || numGrupo == 0)
                    {
                        resultado.Advertencias.Add($"Formato de grupo '{est.Grupo}' no reconocido (esperado: 2 digitos como '31')");
                    }
                }

                if (!string.IsNullOrWhiteSpace(est.Periodo))
                {
                    var cuatrimestre = ParsePeriodo(est.Periodo);
                    if (cuatrimestre == 0)
                    {
                        resultado.Advertencias.Add($"Formato de periodo '{est.Periodo}' no reconocido (esperado: '1ero.', '2do.', '3ero.', '4to.', etc.)");
                    }
                }

                if (!string.IsNullOrWhiteSpace(est.Matricula))
                {
                    if (matriculasEnArchivo.Contains(est.Matricula.ToLower()))
                    {
                        errores.Add("Matricula duplicada en el archivo");
                        matriculasDuplicadasEnArchivo.Add(est.Matricula);
                    }
                    else
                    {
                        matriculasEnArchivo.Add(est.Matricula.ToLower());
                    }

                    if (matriculasExistentes.Contains(est.Matricula.ToLower()))
                    {
                        resultado.Advertencias.Add("Estudiante ya existe en el sistema (se actualizara)");
                    }
                }

                if (!string.IsNullOrWhiteSpace(est.Campus))
                    campusEnArchivo.Add(est.Campus);

                if (!string.IsNullOrWhiteSpace(est.Curso))
                    cursosEnArchivo.Add(est.Curso);

                if (!string.IsNullOrWhiteSpace(est.FechaNacimiento))
                {
                    if (!TryParseDate(est.FechaNacimiento, out _))
                        errores.Add($"Fecha de nacimiento invalida: {est.FechaNacimiento}");
                }

                if (!string.IsNullOrWhiteSpace(est.FechaInscripcion))
                {
                    if (!TryParseDate(est.FechaInscripcion, out _))
                        errores.Add($"Fecha de inscripcion invalida: {est.FechaInscripcion}");
                }

                resultado.Exito = errores.Count == 0;
                resultado.Mensaje = errores.Count > 0 ? string.Join("; ", errores) : "Valido";

                if (resultado.Exito)
                    response.RegistrosValidos++;
                else
                    response.RegistrosConErrores++;

                response.DetalleValidacion.Add(resultado);
            }

            foreach (var campus in campusEnArchivo)
            {
                if (campusExistentes.Contains(campus.ToLower()))
                    response.CampusEncontrados.Add(campus);
                else
                    response.CampusNoEncontrados.Add(campus);
            }

            foreach (var curso in cursosEnArchivo)
            {
                if (planesExistentes.Contains(curso.ToLower()))
                    response.CursosEncontrados.Add(curso);
                else
                    response.CursosNoEncontrados.Add(curso);
            }

            response.MatriculasDuplicadas = matriculasDuplicadasEnArchivo;
            response.EsValido = response.RegistrosConErrores == 0
                && response.CampusNoEncontrados.Count == 0
                && response.CursosNoEncontrados.Count == 0;

            return response;
        }

        public async Task<ImportarEstudiantesResponse> ImportarEstudiantesAsync(ImportarEstudiantesRequest request)
        {
            var response = new ImportarEstudiantesResponse();

            var campusDict = await _db.Campus
                .Where(c => c.Activo)
                .ToDictionaryAsync(c => c.Nombre.ToLower(), c => c);

            var planesDict = await _db.PlanEstudios
                .Include(p => p.IdCampusNavigation)
                .ToDictionaryAsync(p => $"{(p.NombrePlanEstudios ?? p.ClavePlanEstudios).ToLower()}|{p.IdCampus}", p => p);

            var planesPorNombre = await _db.PlanEstudios
                .GroupBy(p => (p.NombrePlanEstudios ?? p.ClavePlanEstudios).ToLower())
                .ToDictionaryAsync(g => g.Key, g => g.ToList());

            var generosDict = await _db.Genero
                .ToDictionaryAsync(g => g.DescGenero.ToLower(), g => g.IdGenero);

            var turnosDict = await _db.Turno
                .ToDictionaryAsync(t => t.IdTurno, t => t);

            int fila = 1;
            foreach (var est in request.Estudiantes)
            {
                var resultado = new ResultadoImportacionEstudiante
                {
                    Fila = fila++,
                    Matricula = est.Matricula,
                    NombreCompleto = $"{est.Nombre} {est.ApellidoPaterno} {est.ApellidoMaterno}".Trim()
                };

                try
                {
                    Campus? campus = null;
                    var campusKey = est.Campus?.ToLower() ?? "";

                    if (campusDict.TryGetValue(campusKey, out var existingCampus))
                    {
                        campus = existingCampus;
                    }
                    else if (request.CrearCatalogosInexistentes && !string.IsNullOrWhiteSpace(est.Campus))
                    {
                        campus = new Campus
                        {
                            ClaveCampus = GenerarClave(est.Campus),
                            Nombre = est.Campus,
                            Activo = true,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = "Sistema-Importacion",
                            Status = Core.Enums.StatusEnum.Active
                        };
                        _db.Campus.Add(campus);
                        await _db.SaveChangesAsync();
                        campusDict[campusKey] = campus;
                        resultado.Advertencias.Add($"Se creo el campus: {est.Campus}");
                    }
                    else
                    {
                        throw new Exception($"Campus no encontrado: {est.Campus}");
                    }

                    PlanEstudios? plan = null;
                    var cursoKey = est.Curso?.ToLower() ?? "";
                    var planKey = $"{cursoKey}|{campus.IdCampus}";

                    if (planesDict.TryGetValue(planKey, out var existingPlan))
                    {
                        plan = existingPlan;
                    }
                    else if (planesPorNombre.TryGetValue(cursoKey, out var planesConNombre))
                    {
                        plan = planesConNombre.FirstOrDefault(p => p.IdCampus == campus.IdCampus)
                            ?? planesConNombre.First();
                        resultado.Advertencias.Add($"Plan encontrado en otro campus, se usara: {plan.NombrePlanEstudios}");
                    }
                    else if (request.CrearCatalogosInexistentes && !string.IsNullOrWhiteSpace(est.Curso))
                    {
                        var periodicidad = await _db.Set<Periodicidad>().FirstOrDefaultAsync();
                        var nivelEducativo = await _db.Set<NivelEducativo>().FirstOrDefaultAsync();

                        plan = new PlanEstudios
                        {
                            ClavePlanEstudios = GenerarClave(est.Curso),
                            NombrePlanEstudios = est.Curso,
                            IdCampus = campus.IdCampus,
                            IdPeriodicidad = periodicidad?.IdPeriodicidad ?? 1,
                            IdNivelEducativo = nivelEducativo?.IdNivelEducativo ?? 1,
                            MinimaAprobatoriaParcial = 70,
                            MinimaAprobatoriaFinal = 70,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = "Sistema-Importacion",
                            Status = Core.Enums.StatusEnum.Active
                        };
                        _db.PlanEstudios.Add(plan);
                        await _db.SaveChangesAsync();
                        planesDict[planKey] = plan;
                        resultado.Advertencias.Add($"Se creo el plan de estudios: {est.Curso}");
                    }
                    else
                    {
                        throw new Exception($"Plan de estudios no encontrado: {est.Curso}");
                    }

                    int? idGenero = GetIdGenero(est.Genero, generosDict);

                    var estudianteExistente = await _db.Estudiante
                        .Include(e => e.IdPersonaNavigation)
                        .FirstOrDefaultAsync(e => e.Matricula.ToLower() == est.Matricula.ToLower());

                    if (estudianteExistente != null)
                    {
                        if (request.ActualizarExistentes)
                        {
                            await ActualizarEstudianteAsync(estudianteExistente, est, plan, idGenero);
                            resultado.IdEstudiante = estudianteExistente.IdEstudiante;
                            resultado.Exito = true;
                            resultado.Mensaje = "Estudiante actualizado";
                            response.Actualizados++;
                        }
                        else
                        {
                            resultado.Exito = false;
                            resultado.Mensaje = "Estudiante ya existe (usar opcion de actualizar)";
                            response.Fallidos++;
                        }
                    }
                    else
                    {
                        var nuevoEstudiante = await CrearEstudianteAsync(est, plan, idGenero);
                        resultado.IdEstudiante = nuevoEstudiante.IdEstudiante;
                        resultado.Exito = true;
                        resultado.Mensaje = "Estudiante creado exitosamente";
                        response.Exitosos++;
                    }

                    if (request.InscribirAGrupo && resultado.Exito && !string.IsNullOrWhiteSpace(est.Grupo))
                    {
                        try
                        {
                            var inscripcionResult = await InscribirAGrupoAsync(
                                resultado.IdEstudiante!.Value,
                                est.Periodo,
                                est.Grupo,
                                plan.IdPlanEstudios,
                                est.Ciclo,
                                request.CrearCatalogosInexistentes);

                            resultado.Advertencias.AddRange(inscripcionResult.Mensajes);
                        }
                        catch (Exception ex)
                        {
                            resultado.Advertencias.Add($"No se pudo inscribir al grupo: {ex.Message}");
                        }
                    }
                }
                catch (DbUpdateException dbEx)
                {
                    resultado.Exito = false;
                    var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;
                    resultado.Mensaje = innerMessage;
                    response.Fallidos++;
                }
                catch (Exception ex)
                {
                    resultado.Exito = false;
                    var innerMessage = ex.InnerException?.Message ?? ex.Message;
                    resultado.Mensaje = innerMessage;
                    response.Fallidos++;
                }

                response.Resultados.Add(resultado);
                response.TotalProcesados++;
            }

            return response;
        }

        private async Task<Estudiante> CrearEstudianteAsync(ImportarEstudianteDto dto, PlanEstudios plan, int? idGenero)
        {
            var curpNormalizado = string.IsNullOrWhiteSpace(dto.Curp) ? null : dto.Curp.Trim().ToUpper();
            var correoNormalizado = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim().ToLower();

            if (!string.IsNullOrEmpty(curpNormalizado))
            {
                var curpExiste = await _db.Persona.AnyAsync(p => p.Curp == curpNormalizado);
                if (curpExiste)
                {
                    throw new Exception($"El CURP '{curpNormalizado}' ya esta registrado en otra persona");
                }
            }

            if (!string.IsNullOrEmpty(correoNormalizado))
            {
                var correoExiste = await _db.Persona.AnyAsync(p => p.Correo == correoNormalizado);
                if (correoExiste)
                {
                    throw new Exception($"El correo '{correoNormalizado}' ya esta registrado en otra persona");
                }
            }

            var persona = new Persona
            {
                Nombre = dto.Nombre,
                ApellidoPaterno = dto.ApellidoPaterno,
                ApellidoMaterno = string.IsNullOrWhiteSpace(dto.ApellidoMaterno) ? null : dto.ApellidoMaterno,
                Curp = curpNormalizado,
                Telefono = string.IsNullOrWhiteSpace(dto.Telefono) ? null : dto.Telefono,
                Celular = string.IsNullOrWhiteSpace(dto.Celular) ? null : dto.Celular,
                Correo = correoNormalizado,
                IdGenero = idGenero,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "Sistema-Importacion",
                Status = Core.Enums.StatusEnum.Active
            };

            if (TryParseDate(dto.FechaNacimiento, out var fechaNac))
            {
                persona.FechaNacimiento = fechaNac;
            }

            if (!string.IsNullOrWhiteSpace(dto.Domicilio))
            {
                var codigoPostalExistente = await _db.Set<CodigoPostal>().FirstOrDefaultAsync();
                if (codigoPostalExistente != null)
                {
                    var direccion = new Direccion
                    {
                        Calle = $"{dto.Domicilio} {dto.Colonia}".Trim(),
                        CodigoPostalId = codigoPostalExistente.Id,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "Sistema-Importacion",
                        Status = Core.Enums.StatusEnum.Active
                    };
                    _db.Direccion.Add(direccion);
                    await _db.SaveChangesAsync();
                    persona.IdDireccion = direccion.IdDireccion;
                }
            }

            _db.Persona.Add(persona);
            await _db.SaveChangesAsync();

            DateOnly fechaIngreso = DateOnly.FromDateTime(DateTime.Today);
            if (TryParseDate(dto.FechaInscripcion, out var fechaIns))
            {
                fechaIngreso = fechaIns;
            }

            var estudiante = new Estudiante
            {
                Matricula = dto.Matricula,
                IdPersona = persona.IdPersona,
                IdPlanActual = plan.IdPlanEstudios,
                Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim().ToLower(),
                FechaIngreso = fechaIngreso,
                Activo = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "Sistema-Importacion",
                Status = Core.Enums.StatusEnum.Active
            };

            _db.Estudiante.Add(estudiante);
            await _db.SaveChangesAsync();

            var estudiantePlan = new EstudiantePlan
            {
                IdEstudiante = estudiante.IdEstudiante,
                IdPlanEstudios = plan.IdPlanEstudios,
                FechaInicio = fechaIngreso,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "Sistema-Importacion",
                Status = Core.Enums.StatusEnum.Active
            };
            _db.EstudiantePlan.Add(estudiantePlan);
            await _db.SaveChangesAsync();

            return estudiante;
        }

        private async Task ActualizarEstudianteAsync(Estudiante estudiante, ImportarEstudianteDto dto, PlanEstudios plan, int? idGenero)
        {
            var persona = estudiante.IdPersonaNavigation;

            var curpNormalizado = string.IsNullOrWhiteSpace(dto.Curp) ? null : dto.Curp.Trim().ToUpper();
            var correoNormalizado = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim().ToLower();

            if (!string.IsNullOrEmpty(curpNormalizado) && curpNormalizado != persona.Curp)
            {
                var curpExiste = await _db.Persona.AnyAsync(p => p.Curp == curpNormalizado && p.IdPersona != persona.IdPersona);
                if (curpExiste)
                {
                    throw new Exception($"El CURP '{curpNormalizado}' ya esta registrado en otra persona");
                }
            }

            if (!string.IsNullOrEmpty(correoNormalizado) && correoNormalizado != persona.Correo)
            {
                var correoExiste = await _db.Persona.AnyAsync(p => p.Correo == correoNormalizado && p.IdPersona != persona.IdPersona);
                if (correoExiste)
                {
                    throw new Exception($"El correo '{correoNormalizado}' ya esta registrado en otra persona");
                }
            }

            persona.Nombre = dto.Nombre;
            persona.ApellidoPaterno = dto.ApellidoPaterno;
            persona.ApellidoMaterno = string.IsNullOrWhiteSpace(dto.ApellidoMaterno) ? null : dto.ApellidoMaterno;

            if (!string.IsNullOrEmpty(curpNormalizado))
                persona.Curp = curpNormalizado;

            if (!string.IsNullOrWhiteSpace(dto.Telefono))
                persona.Telefono = dto.Telefono;

            if (!string.IsNullOrWhiteSpace(dto.Celular))
                persona.Celular = dto.Celular;

            if (!string.IsNullOrEmpty(correoNormalizado))
            {
                persona.Correo = correoNormalizado;
                estudiante.Email = correoNormalizado;
            }

            if (TryParseDate(dto.FechaNacimiento, out var fechaNac))
            {
                persona.FechaNacimiento = fechaNac;
            }

            if (idGenero.HasValue)
            {
                persona.IdGenero = idGenero;
            }

            estudiante.IdPlanActual = plan.IdPlanEstudios;

            await _db.SaveChangesAsync();
        }

        private async Task<InscripcionGrupoResult> InscribirAGrupoAsync(
            int idEstudiante,
            string? periodo,
            string? grupo,
            int idPlanEstudios,
            string? ciclo,
            bool crearSiNoExiste)
        {
            var result = new InscripcionGrupoResult();

            byte numeroCuatrimestre = ParsePeriodo(periodo);
            if (numeroCuatrimestre == 0)
            {
                result.Mensajes.Add($"No se pudo determinar el cuatrimestre del periodo '{periodo}'");
                return result;
            }

            var (idTurno, numeroGrupo) = ParseGrupo(grupo);
            if (idTurno == 0 || numeroGrupo == 0)
            {
                result.Mensajes.Add($"No se pudo parsear el grupo '{grupo}' (esperado formato: turno+numero, ej: '31')");
                return result;
            }

            var turnoExiste = await _db.Turno.AnyAsync(t => t.IdTurno == idTurno);
            if (!turnoExiste)
            {
                result.Mensajes.Add($"El turno {idTurno} no existe en el sistema");
                return result;
            }

            var periodoAcademico = await BuscarPeriodoAcademicoAsync(ciclo);
            if (periodoAcademico == null)
            {
                result.Mensajes.Add($"No se encontro periodo academico para el ciclo '{ciclo}'");
                return result;
            }

            var grupoEntity = await _db.Grupo
                .Include(g => g.GrupoMateria)
                    .ThenInclude(gm => gm.IdMateriaPlanNavigation)
                .FirstOrDefaultAsync(g =>
                    g.IdPlanEstudios == idPlanEstudios &&
                    g.IdPeriodoAcademico == periodoAcademico.IdPeriodoAcademico &&
                    g.NumeroCuatrimestre == numeroCuatrimestre &&
                    g.IdTurno == idTurno &&
                    g.NumeroGrupo == numeroGrupo);

            if (grupoEntity == null && crearSiNoExiste)
            {
                grupoEntity = await CrearGrupoAsync(
                    idPlanEstudios,
                    periodoAcademico.IdPeriodoAcademico,
                    numeroCuatrimestre,
                    idTurno,
                    numeroGrupo);

                result.Mensajes.Add($"Se creo el grupo {grupoEntity.CodigoGrupo}");

                grupoEntity = await _db.Grupo
                    .Include(g => g.GrupoMateria)
                        .ThenInclude(gm => gm.IdMateriaPlanNavigation)
                    .FirstAsync(g => g.IdGrupo == grupoEntity.IdGrupo);
            }

            if (grupoEntity == null)
            {
                result.Mensajes.Add($"Grupo no encontrado: Plan={idPlanEstudios}, Periodo={periodoAcademico.IdPeriodoAcademico}, Cuat={numeroCuatrimestre}, Turno={idTurno}, Grupo={numeroGrupo}");
                return result;
            }

            var materiasInscritas = 0;
            foreach (var grupoMateria in grupoEntity.GrupoMateria)
            {
                var yaInscrito = await _db.Inscripcion
                    .AnyAsync(i => i.IdEstudiante == idEstudiante && i.IdGrupoMateria == grupoMateria.IdGrupoMateria);

                if (!yaInscrito)
                {
                    var inscripcion = new Inscripcion
                    {
                        IdEstudiante = idEstudiante,
                        IdGrupoMateria = grupoMateria.IdGrupoMateria,
                        FechaInscripcion = DateTime.UtcNow,
                        Estado = "Inscrito",
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "Sistema-Importacion",
                        Status = Core.Enums.StatusEnum.Active
                    };
                    _db.Inscripcion.Add(inscripcion);
                    materiasInscritas++;
                }
            }

            if (materiasInscritas > 0)
            {
                await _db.SaveChangesAsync();
                result.Mensajes.Add($"Inscrito al grupo {grupoEntity.CodigoGrupo} en {materiasInscritas} materia(s)");
            }
            else if (grupoEntity.GrupoMateria.Count == 0)
            {
                result.Mensajes.Add($"El grupo {grupoEntity.CodigoGrupo} no tiene materias asignadas");
            }
            else
            {
                result.Mensajes.Add($"Ya estaba inscrito en todas las materias del grupo {grupoEntity.CodigoGrupo}");
            }

            result.Exito = true;
            result.IdGrupo = grupoEntity.IdGrupo;
            return result;
        }

        private async Task<PeriodoAcademico?> BuscarPeriodoAcademicoAsync(string? ciclo)
        {
            if (string.IsNullOrWhiteSpace(ciclo))
            {
                return await _db.PeriodoAcademico
                    .Where(p => p.EsPeriodoActual)
                    .FirstOrDefaultAsync();
            }

            var cicloUpper = ciclo.ToUpper();
            string? patronMeses = null;

            if (cicloUpper.Contains("ENERO-ABRIL") || cicloUpper.Contains("ENERO ABRIL"))
                patronMeses = "Enero-Abril";
            else if (cicloUpper.Contains("MAYO-AGOSTO") || cicloUpper.Contains("MAYO AGOSTO"))
                patronMeses = "Mayo-Agosto";
            else if (cicloUpper.Contains("SEPTIEMBRE-DICIEMBRE") || cicloUpper.Contains("SEPTIEMBRE DICIEMBRE"))
                patronMeses = "Septiembre-Diciembre";

            if (patronMeses != null)
            {
                var periodo = await _db.PeriodoAcademico
                    .Where(p => p.Nombre.Contains(patronMeses))
                    .OrderByDescending(p => p.FechaInicio)
                    .FirstOrDefaultAsync();

                if (periodo != null)
                    return periodo;
            }

            var matchAnio = Regex.Match(ciclo, @"20\d{2}");
            if (matchAnio.Success)
            {
                var anio = matchAnio.Value;
                var periodo = await _db.PeriodoAcademico
                    .Where(p => p.Clave.Contains(anio) || p.Nombre.Contains(anio))
                    .OrderByDescending(p => p.FechaInicio)
                    .FirstOrDefaultAsync();

                if (periodo != null)
                    return periodo;
            }

            return await _db.PeriodoAcademico
                .OrderByDescending(p => p.EsPeriodoActual)
                .ThenByDescending(p => p.FechaInicio)
                .FirstOrDefaultAsync();
        }

        private async Task<Grupo> CrearGrupoAsync(
            int idPlanEstudios,
            int idPeriodoAcademico,
            byte numeroCuatrimestre,
            int idTurno,
            byte numeroGrupo)
        {
            var turno = await _db.Turno.FindAsync(idTurno);
            var plan = await _db.PlanEstudios.FindAsync(idPlanEstudios);

            var codigoGrupo = $"{numeroCuatrimestre}{idTurno}{numeroGrupo}";
            var nombreGrupo = $"{plan?.NombrePlanEstudios ?? "Plan"} - Cuat. {numeroCuatrimestre} - {turno?.Nombre ?? "Turno"} - Grupo {numeroGrupo}";

            var grupo = new Grupo
            {
                IdPlanEstudios = idPlanEstudios,
                IdPeriodoAcademico = idPeriodoAcademico,
                NumeroCuatrimestre = numeroCuatrimestre,
                IdTurno = idTurno,
                NumeroGrupo = numeroGrupo,
                CodigoGrupo = codigoGrupo,
                NombreGrupo = nombreGrupo,
                CapacidadMaxima = 40,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "Sistema-Importacion",
                Status = Core.Enums.StatusEnum.Active
            };

            _db.Grupo.Add(grupo);
            await _db.SaveChangesAsync();

            var materiasPlan = await _db.MateriaPlan
                .Include(mp => mp.IdMateriaNavigation)
                .Where(mp => mp.IdPlanEstudios == idPlanEstudios
                    && mp.Cuatrimestre == numeroCuatrimestre
                    && mp.Status == Core.Enums.StatusEnum.Active)
                .ToListAsync();

            foreach (var materiaPlan in materiasPlan)
            {
                var nombreMateria = materiaPlan.IdMateriaNavigation?.Nombre ?? $"Materia {materiaPlan.IdMateria}";
                var grupoMateria = new GrupoMateria
                {
                    IdGrupo = grupo.IdGrupo,
                    IdMateriaPlan = materiaPlan.IdMateriaPlan,
                    Name = $"{nombreGrupo} - {nombreMateria}",
                    Cupo = 40,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "Sistema-Importacion",
                    Status = Core.Enums.StatusEnum.Active
                };
                _db.GrupoMateria.Add(grupoMateria);
            }

            await _db.SaveChangesAsync();

            return grupo;
        }

        private static byte ParsePeriodo(string? periodo)
        {
            if (string.IsNullOrWhiteSpace(periodo))
                return 0;

            var periodoLower = periodo.ToLower().Trim();

            var mapeo = new Dictionary<string, byte>
            {
                { "1ero", 1 }, { "1ero.", 1 }, { "1ro", 1 }, { "1ro.", 1 }, { "primero", 1 }, { "1", 1 },
                { "2do", 2 }, { "2do.", 2 }, { "segundo", 2 }, { "2", 2 },
                { "3ero", 3 }, { "3ero.", 3 }, { "3ro", 3 }, { "3ro.", 3 }, { "tercero", 3 }, { "3", 3 },
                { "4to", 4 }, { "4to.", 4 }, { "cuarto", 4 }, { "4", 4 },
                { "5to", 5 }, { "5to.", 5 }, { "quinto", 5 }, { "5", 5 },
                { "6to", 6 }, { "6to.", 6 }, { "sexto", 6 }, { "6", 6 },
                { "7mo", 7 }, { "7mo.", 7 }, { "septimo", 7 }, { "septimo", 7 }, { "7", 7 },
                { "8vo", 8 }, { "8vo.", 8 }, { "octavo", 8 }, { "8", 8 },
                { "9no", 9 }, { "9no.", 9 }, { "noveno", 9 }, { "9", 9 },
                { "10mo", 10 }, { "10mo.", 10 }, { "decimo", 10 }, { "decimo", 10 }, { "10", 10 },
                { "11vo", 11 }, { "11vo.", 11 }, { "11", 11 },
                { "12vo", 12 }, { "12vo.", 12 }, { "12", 12 }
            };

            if (mapeo.TryGetValue(periodoLower, out var resultado))
                return resultado;

            var match = Regex.Match(periodo, @"\d+");
            if (match.Success && byte.TryParse(match.Value, out var num))
                return num;

            return 0;
        }

        private static (int IdTurno, byte NumeroGrupo) ParseGrupo(string? grupo)
        {
            if (string.IsNullOrWhiteSpace(grupo))
                return (0, 0);

            var grupoTrim = grupo.Trim();

            if (grupoTrim.Length == 2 && int.TryParse(grupoTrim, out _))
            {
                var turno = int.Parse(grupoTrim[0].ToString());
                var numGrupo = byte.Parse(grupoTrim[1].ToString());
                return (turno, numGrupo);
            }

            if (grupoTrim.Length == 1 && byte.TryParse(grupoTrim, out var numGrupoSolo))
            {
                return (1, numGrupoSolo);
            }

            if (grupoTrim.Length == 3 && int.TryParse(grupoTrim, out _))
            {
                var turno = int.Parse(grupoTrim[1].ToString());
                var numGrupo = byte.Parse(grupoTrim[2].ToString());
                return (turno, numGrupo);
            }

            return (0, 0);
        }

        private static int? GetIdGenero(string? genero, Dictionary<string, int> generosDict)
        {
            if (string.IsNullOrWhiteSpace(genero))
                return null;

            var generoLower = genero.ToLower().Trim();

            if (generosDict.TryGetValue(generoLower, out var idGenero))
                return idGenero;

            if (generoLower.Contains("masc") || generoLower == "m" || generoLower == "hombre")
                return generosDict.GetValueOrDefault("masculino");

            if (generoLower.Contains("fem") || generoLower == "f" || generoLower == "mujer")
                return generosDict.GetValueOrDefault("femenino");

            return generosDict.GetValueOrDefault("no especifica");
        }

        private static bool TryParseDate(string? dateStr, out DateOnly result)
        {
            result = default;

            if (string.IsNullOrWhiteSpace(dateStr) || dateStr == "-" || dateStr == "0000-00-00")
                return false;

            string[] formats = {
                "yyyy-MM-dd",
                "dd/MM/yyyy",
                "MM/dd/yyyy",
                "d/M/yyyy",
                "yyyy/MM/dd"
            };

            foreach (var format in formats)
            {
                if (DateOnly.TryParseExact(dateStr, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                    return true;
            }

            if (DateTime.TryParse(dateStr, out var dt))
            {
                result = DateOnly.FromDateTime(dt);
                return true;
            }

            return false;
        }

        private static string GenerarClave(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return "CLAVE";

            var palabras = nombre.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var clave = string.Join("", palabras.Take(4).Select(p => p[0])).ToUpper();

            return clave.Length >= 3 ? clave : nombre.Substring(0, Math.Min(5, nombre.Length)).ToUpper();
        }

    public async Task<ImportarCampusResponse> ImportarCampusAsync(ImportarCampusRequest request)
    {
        var response = new ImportarCampusResponse();

        var clavesExistentes = await _db.Campus
            .Select(c => c.ClaveCampus.ToLower())
            .ToListAsync();

        var nombresExistentes = await _db.Campus
            .Select(c => c.Nombre.ToLower())
            .ToListAsync();

        int fila = 1;
        foreach (var dto in request.Campus)
        {
            var resultado = new ResultadoImportacionCampus
            {
                Fila = fila++,
                ClaveCampus = dto.ClaveCampus,
                Nombre = dto.Nombre
            };

            try
            {
                if (string.IsNullOrWhiteSpace(dto.ClaveCampus))
                    throw new Exception("La clave del campus es requerida");

                if (string.IsNullOrWhiteSpace(dto.Nombre))
                    throw new Exception("El nombre del campus es requerido");

                var claveNormalizada = dto.ClaveCampus.Trim().ToUpper();
                var nombreNormalizado = dto.Nombre.Trim();

                var campusExistente = await _db.Campus
                    .FirstOrDefaultAsync(c => c.ClaveCampus.ToLower() == claveNormalizada.ToLower());

                if (campusExistente != null)
                {
                    if (request.ActualizarExistentes)
                    {
                        var nombreUsadoPorOtro = await _db.Campus
                            .AnyAsync(c => c.Nombre.ToLower() == nombreNormalizado.ToLower() && c.IdCampus != campusExistente.IdCampus);

                        if (nombreUsadoPorOtro)
                            throw new Exception($"El nombre '{nombreNormalizado}' ya esta usado por otro campus");

                        campusExistente.Nombre = nombreNormalizado;
                        campusExistente.Activo = true;

                        if (!string.IsNullOrWhiteSpace(dto.Telefono))
                            campusExistente.Telefono = dto.Telefono.Trim();

                        if (!string.IsNullOrWhiteSpace(dto.Calle) || !string.IsNullOrWhiteSpace(dto.CodigoPostal))
                        {
                            await ActualizarDireccionCampusAsync(campusExistente, dto);
                        }

                        await _db.SaveChangesAsync();

                        resultado.IdCampus = campusExistente.IdCampus;
                        resultado.Exito = true;
                        resultado.Mensaje = "Campus actualizado";
                        response.Actualizados++;
                    }
                    else
                    {
                        resultado.Exito = false;
                        resultado.Mensaje = "Campus ya existe (usar opcion de actualizar)";
                        response.Fallidos++;
                    }
                }
                else
                {
                    var nombreDuplicado = await _db.Campus
                        .AnyAsync(c => c.Nombre.ToLower() == nombreNormalizado.ToLower());

                    if (nombreDuplicado)
                        throw new Exception($"El nombre '{nombreNormalizado}' ya esta usado por otro campus");

                    var nuevoCampus = new Campus
                    {
                        ClaveCampus = claveNormalizada,
                        Nombre = nombreNormalizado,
                        Telefono = string.IsNullOrWhiteSpace(dto.Telefono) ? null : dto.Telefono.Trim(),
                        Activo = true,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "Sistema-Importacion",
                        Status = Core.Enums.StatusEnum.Active
                    };

                    if (!string.IsNullOrWhiteSpace(dto.Calle) || !string.IsNullOrWhiteSpace(dto.CodigoPostal))
                    {
                        var direccion = await CrearDireccionAsync(dto.Calle, dto.NumeroExterior, dto.NumeroInterior, dto.CodigoPostal, dto.Colonia);
                        if (direccion != null)
                        {
                            nuevoCampus.IdDireccion = direccion.IdDireccion;
                        }
                        else
                        {
                            resultado.Advertencias.Add("No se pudo crear la direccion (codigo postal no encontrado)");
                        }
                    }

                    _db.Campus.Add(nuevoCampus);
                    await _db.SaveChangesAsync();

                    resultado.IdCampus = nuevoCampus.IdCampus;
                    resultado.Exito = true;
                    resultado.Mensaje = "Campus creado exitosamente";
                    response.Exitosos++;
                }
            }
            catch (DbUpdateException dbEx)
            {
                resultado.Exito = false;
                resultado.Mensaje = dbEx.InnerException?.Message ?? dbEx.Message;
                response.Fallidos++;
            }
            catch (Exception ex)
            {
                resultado.Exito = false;
                resultado.Mensaje = ex.Message;
                response.Fallidos++;
            }

            response.Resultados.Add(resultado);
            response.TotalProcesados++;
        }

        return response;
    }

    private async Task<Direccion?> CrearDireccionAsync(string? calle, string? numExterior, string? numInterior, string? codigoPostal, string? colonia = null)
    {
        int? codigoPostalId = null;

        if (!string.IsNullOrWhiteSpace(codigoPostal))
        {
            var codigoTrim = codigoPostal.Trim();

            if (!string.IsNullOrWhiteSpace(colonia))
            {
                var coloniaTrim = colonia.Trim().ToLower();
                var cp = await _db.CodigosPostales
                    .FirstOrDefaultAsync(c => c.Codigo == codigoTrim && c.Asentamiento.ToLower().Contains(coloniaTrim));

                if (cp == null)
                {
                    cp = await _db.CodigosPostales
                        .FirstOrDefaultAsync(c => c.Codigo == codigoTrim);
                }
                codigoPostalId = cp?.Id;
            }
            else
            {
                var cp = await _db.CodigosPostales
                    .FirstOrDefaultAsync(c => c.Codigo == codigoTrim);
                codigoPostalId = cp?.Id;
            }
        }

        if (codigoPostalId == null)
        {
            var cpDefault = await _db.CodigosPostales.FirstOrDefaultAsync();
            if (cpDefault == null)
                return null;
            codigoPostalId = cpDefault.Id;
        }

        var direccion = new Direccion
        {
            Calle = calle?.Trim(),
            NumeroExterior = numExterior?.Trim(),
            NumeroInterior = numInterior?.Trim(),
            CodigoPostalId = codigoPostalId.Value,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "Sistema-Importacion",
            Status = Core.Enums.StatusEnum.Active
        };

        _db.Direccion.Add(direccion);
        await _db.SaveChangesAsync();

        return direccion;
    }

    private async Task ActualizarDireccionCampusAsync(Campus campus, ImportarCampusDto dto)
    {
        if (campus.IdDireccion.HasValue)
        {
            var direccion = await _db.Direccion.FindAsync(campus.IdDireccion.Value);
            if (direccion != null)
            {
                if (!string.IsNullOrWhiteSpace(dto.Calle))
                    direccion.Calle = dto.Calle.Trim();
                if (!string.IsNullOrWhiteSpace(dto.NumeroExterior))
                    direccion.NumeroExterior = dto.NumeroExterior.Trim();
                if (!string.IsNullOrWhiteSpace(dto.NumeroInterior))
                    direccion.NumeroInterior = dto.NumeroInterior.Trim();
                if (!string.IsNullOrWhiteSpace(dto.CodigoPostal))
                {
                    var codigoTrim = dto.CodigoPostal.Trim();
                    CodigoPostal? cp = null;

                    if (!string.IsNullOrWhiteSpace(dto.Colonia))
                    {
                        var coloniaTrim = dto.Colonia.Trim().ToLower();
                        cp = await _db.CodigosPostales
                            .FirstOrDefaultAsync(c => c.Codigo == codigoTrim && c.Asentamiento.ToLower().Contains(coloniaTrim));
                    }

                    if (cp == null)
                    {
                        cp = await _db.CodigosPostales
                            .FirstOrDefaultAsync(c => c.Codigo == codigoTrim);
                    }

                    if (cp != null)
                        direccion.CodigoPostalId = cp.Id;
                }
            }
        }
        else
        {
            var direccion = await CrearDireccionAsync(dto.Calle, dto.NumeroExterior, dto.NumeroInterior, dto.CodigoPostal, dto.Colonia);
            if (direccion != null)
            {
                campus.IdDireccion = direccion.IdDireccion;
            }
        }
    }

    public async Task<ImportarPlanesEstudiosResponse> ImportarPlanesEstudiosAsync(ImportarPlanesEstudiosRequest request)
    {
        var response = new ImportarPlanesEstudiosResponse();

        var campusDict = await _db.Campus
            .Where(c => c.Activo)
            .ToDictionaryAsync(c => c.ClaveCampus.ToLower(), c => c);

        var periodicidadesDict = await _db.Set<Periodicidad>()
            .ToDictionaryAsync(p => p.DescPeriodicidad.ToLower(), p => p.IdPeriodicidad);

        var nivelesDict = await _db.Set<NivelEducativo>()
            .ToDictionaryAsync(n => n.DescNivelEducativo.ToLower(), n => n.IdNivelEducativo);

        var periodicidadDefault = await _db.Set<Periodicidad>().FirstOrDefaultAsync();
        var nivelDefault = await _db.Set<NivelEducativo>().FirstOrDefaultAsync();

        int fila = 1;
        foreach (var dto in request.Planes)
        {
            var resultado = new ResultadoImportacionPlanEstudios
            {
                Fila = fila++,
                ClavePlanEstudios = dto.ClavePlanEstudios,
                NombrePlanEstudios = dto.NombrePlanEstudios
            };

            try
            {
                if (string.IsNullOrWhiteSpace(dto.ClavePlanEstudios))
                    throw new Exception("La clave del plan es requerida");

                if (string.IsNullOrWhiteSpace(dto.NombrePlanEstudios))
                    throw new Exception("El nombre del plan es requerido");

                if (string.IsNullOrWhiteSpace(dto.ClaveCampus))
                    throw new Exception("La clave del campus es requerida");

                var claveNormalizada = dto.ClavePlanEstudios.Trim().ToUpper();
                var nombreNormalizado = dto.NombrePlanEstudios.Trim();
                var claveCampusNormalizada = dto.ClaveCampus.Trim().ToLower();

                if (!campusDict.TryGetValue(claveCampusNormalizada, out var campus))
                    throw new Exception($"Campus no encontrado con clave: {dto.ClaveCampus}");

                int idPeriodicidad = periodicidadDefault?.IdPeriodicidad ?? 1;
                if (!string.IsNullOrWhiteSpace(dto.Periodicidad))
                {
                    if (periodicidadesDict.TryGetValue(dto.Periodicidad.ToLower().Trim(), out var idPer))
                        idPeriodicidad = idPer;
                    else
                        resultado.Advertencias.Add($"Periodicidad '{dto.Periodicidad}' no encontrada, usando valor por defecto");
                }

                int idNivelEducativo = nivelDefault?.IdNivelEducativo ?? 1;
                if (!string.IsNullOrWhiteSpace(dto.NivelEducativo))
                {
                    if (nivelesDict.TryGetValue(dto.NivelEducativo.ToLower().Trim(), out var idNivel))
                        idNivelEducativo = idNivel;
                    else
                        resultado.Advertencias.Add($"Nivel educativo '{dto.NivelEducativo}' no encontrado, usando valor por defecto");
                }

                var planExistente = await _db.PlanEstudios
                    .FirstOrDefaultAsync(p => p.ClavePlanEstudios.ToLower() == claveNormalizada.ToLower()
                                           && p.IdCampus == campus.IdCampus);

                if (planExistente != null)
                {
                    if (request.ActualizarExistentes)
                    {
                        planExistente.NombrePlanEstudios = nombreNormalizado;
                        planExistente.IdCampus = campus.IdCampus;
                        planExistente.IdPeriodicidad = idPeriodicidad;
                        planExistente.IdNivelEducativo = idNivelEducativo;

                        if (dto.DuracionMeses.HasValue)
                            planExistente.DuracionMeses = dto.DuracionMeses.Value;

                        if (!string.IsNullOrWhiteSpace(dto.RVOE))
                            planExistente.RVOE = dto.RVOE.Trim();

                        if (!string.IsNullOrWhiteSpace(dto.Version))
                            planExistente.Version = dto.Version.Trim();

                        await _db.SaveChangesAsync();

                        resultado.IdPlanEstudios = planExistente.IdPlanEstudios;
                        resultado.Exito = true;
                        resultado.Mensaje = "Plan de estudios actualizado";
                        response.Actualizados++;
                    }
                    else
                    {
                        resultado.Exito = false;
                        resultado.Mensaje = "Plan de estudios ya existe (usar opcion de actualizar)";
                        response.Fallidos++;
                    }
                }
                else
                {
                    var nuevoPlan = new PlanEstudios
                    {
                        ClavePlanEstudios = claveNormalizada,
                        NombrePlanEstudios = nombreNormalizado,
                        IdCampus = campus.IdCampus,
                        IdPeriodicidad = idPeriodicidad,
                        IdNivelEducativo = idNivelEducativo,
                        DuracionMeses = dto.DuracionMeses ?? 48,
                        RVOE = dto.RVOE?.Trim(),
                        Version = dto.Version?.Trim(),
                        MinimaAprobatoriaParcial = 60,
                        MinimaAprobatoriaFinal = 70,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "Sistema-Importacion",
                        Status = Core.Enums.StatusEnum.Active
                    };

                    _db.PlanEstudios.Add(nuevoPlan);
                    await _db.SaveChangesAsync();

                    resultado.IdPlanEstudios = nuevoPlan.IdPlanEstudios;
                    resultado.Exito = true;
                    resultado.Mensaje = "Plan de estudios creado exitosamente";
                    response.Exitosos++;
                }
            }
            catch (DbUpdateException dbEx)
            {
                resultado.Exito = false;
                resultado.Mensaje = dbEx.InnerException?.Message ?? dbEx.Message;
                response.Fallidos++;
            }
            catch (Exception ex)
            {
                resultado.Exito = false;
                resultado.Mensaje = ex.Message;
                response.Fallidos++;
            }

            response.Resultados.Add(resultado);
            response.TotalProcesados++;
        }

        return response;
        }

        public async Task<ValidarMateriasResponse> ValidarMateriasAsync(ValidarMateriasRequest request)
        {
            var response = new ValidarMateriasResponse
            {
                TotalRegistros = request.Materias.Count
            };

            var campusExistentes = await _db.Campus
                .Where(c => c.Activo)
                .ToDictionaryAsync(c => c.ClaveCampus.ToLower(), c => c.IdCampus);

            var planesExistentes = await _db.PlanEstudios
                .Where(p => p.Status == Core.Enums.StatusEnum.Active)
                .Select(p => new {
                    p.IdPlanEstudios,
                    p.IdCampus,
                    Nombre = (p.NombrePlanEstudios ?? p.ClavePlanEstudios).ToLower(),
                    Clave = p.ClavePlanEstudios.ToLower()
                })
                .ToListAsync();

            var materiasExistentes = await _db.Materia
                .Where(m => m.Status == Core.Enums.StatusEnum.Active)
                .Select(m => m.Clave.ToLower())
                .ToListAsync();

            var planesCampusEnArchivo = new HashSet<string>();
            var clavesEnArchivo = new HashSet<string>();
            var clavesDuplicadas = new List<string>();

            int fila = 1;
            foreach (var mat in request.Materias)
            {
                var resultado = new ResultadoImportacionMateria
                {
                    Fila = fila++,
                    Clave = mat.Clave,
                    Nombre = mat.Nombre,
                    PlanEstudios = mat.PlanEstudios,
                    Cuatrimestre = ParseCuatrimestre(mat.Cuatrimestre)
                };

                var errores = new List<string>();

                if (string.IsNullOrWhiteSpace(mat.Clave))
                    errores.Add("Clave es requerida");

                if (string.IsNullOrWhiteSpace(mat.Nombre))
                    errores.Add("Nombre es requerido");

                if (string.IsNullOrWhiteSpace(mat.PlanEstudios))
                    errores.Add("Plan de estudios es requerido");

                if (string.IsNullOrWhiteSpace(mat.ClaveCampus))
                    errores.Add("Clave del campus es requerida");

                if (string.IsNullOrWhiteSpace(mat.Cuatrimestre))
                    errores.Add("Cuatrimestre es requerido");
                else if (resultado.Cuatrimestre == 0)
                    errores.Add($"Cuatrimestre invalido: '{mat.Cuatrimestre}'");

                if (!string.IsNullOrWhiteSpace(mat.ClaveCampus) && !campusExistentes.ContainsKey(mat.ClaveCampus.ToLower()))
                    errores.Add($"Campus no encontrado: '{mat.ClaveCampus}'");

                if (!string.IsNullOrWhiteSpace(mat.Clave))
                {
                    var claveKey = $"{mat.Clave.ToLower()}|{mat.PlanEstudios?.ToLower()}|{mat.ClaveCampus?.ToLower()}|{resultado.Cuatrimestre}";
                    if (clavesEnArchivo.Contains(claveKey))
                    {
                        errores.Add("Combinacion clave/plan/campus/cuatrimestre duplicada en el archivo");
                        clavesDuplicadas.Add(mat.Clave);
                    }
                    else
                    {
                        clavesEnArchivo.Add(claveKey);
                    }

                    if (materiasExistentes.Contains(mat.Clave.ToLower()))
                    {
                        resultado.Advertencias.Add("Materia ya existe (se usara la existente)");
                    }
                }

                if (!string.IsNullOrWhiteSpace(mat.PlanEstudios) && !string.IsNullOrWhiteSpace(mat.ClaveCampus))
                {
                    var planCampusKey = $"{mat.PlanEstudios}|{mat.ClaveCampus}";
                    planesCampusEnArchivo.Add(planCampusKey);
                }

                resultado.Exito = errores.Count == 0;
                resultado.Mensaje = errores.Count > 0 ? string.Join("; ", errores) : "Valido";

                if (resultado.Exito)
                    response.RegistrosValidos++;
                else
                    response.RegistrosConErrores++;

                response.DetalleValidacion.Add(resultado);
            }

            foreach (var planCampus in planesCampusEnArchivo)
            {
                var parts = planCampus.Split('|');
                var planKey = parts[0].ToLower();
                var campusKey = parts[1].ToLower();

                if (campusExistentes.TryGetValue(campusKey, out var idCampus))
                {
                    var planExiste = planesExistentes.Any(p =>
                        p.IdCampus == idCampus &&
                        (p.Nombre == planKey || p.Clave == planKey));

                    if (planExiste)
                        response.PlanesEncontrados.Add($"{parts[0]} (Campus: {parts[1]})");
                    else
                        response.PlanesNoEncontrados.Add($"{parts[0]} (Campus: {parts[1]})");
                }
                else
                {
                    response.PlanesNoEncontrados.Add($"{parts[0]} (Campus: {parts[1]} - NO EXISTE)");
                }
            }

            response.ClavesDuplicadas = clavesDuplicadas;
            response.EsValido = response.RegistrosConErrores == 0 && response.PlanesNoEncontrados.Count == 0;

            return response;
        }

        public async Task<ImportarMateriasResponse> ImportarMateriasAsync(ImportarMateriasRequest request)
        {
            var response = new ImportarMateriasResponse();

            var campusDict = await _db.Campus
                .Where(c => c.Activo)
                .ToDictionaryAsync(c => c.ClaveCampus.ToLower(), c => c);

            var planes = await _db.PlanEstudios
                .Include(p => p.IdCampusNavigation)
                .Where(p => p.Status == Core.Enums.StatusEnum.Active)
                .ToListAsync();

            var materiasDict = await _db.Materia
                .Where(m => m.Status == Core.Enums.StatusEnum.Active)
                .ToDictionaryAsync(m => m.Clave.ToLower(), m => m);

            int fila = 1;
            foreach (var dto in request.Materias)
            {
                var cuatrimestre = ParseCuatrimestre(dto.Cuatrimestre);

                var resultado = new ResultadoImportacionMateria
                {
                    Fila = fila++,
                    Clave = dto.Clave,
                    Nombre = dto.Nombre,
                    PlanEstudios = dto.PlanEstudios,
                    Cuatrimestre = cuatrimestre
                };

                try
                {
                    if (string.IsNullOrWhiteSpace(dto.Clave))
                        throw new Exception("La clave de la materia es requerida");

                    if (string.IsNullOrWhiteSpace(dto.Nombre))
                        throw new Exception("El nombre de la materia es requerido");

                    if (string.IsNullOrWhiteSpace(dto.PlanEstudios))
                        throw new Exception("El plan de estudios es requerido");

                    if (string.IsNullOrWhiteSpace(dto.ClaveCampus))
                        throw new Exception("La clave del campus es requerida");

                    if (cuatrimestre == 0)
                        throw new Exception($"Cuatrimestre invalido: '{dto.Cuatrimestre}'");

                    var claveNormalizada = dto.Clave.Trim().ToUpper();
                    var nombreNormalizado = dto.Nombre.Trim();
                    var planKey = dto.PlanEstudios.ToLower().Trim();
                    var campusKey = dto.ClaveCampus.ToLower().Trim();

                    if (!campusDict.TryGetValue(campusKey, out var campus))
                    {
                        throw new Exception($"Campus no encontrado con clave: {dto.ClaveCampus}");
                    }

                    var plan = planes.FirstOrDefault(p =>
                        p.IdCampus == campus.IdCampus &&
                        ((p.NombrePlanEstudios ?? p.ClavePlanEstudios).ToLower() == planKey ||
                         p.ClavePlanEstudios.ToLower() == planKey));

                    if (plan == null)
                    {
                        var planCampusKey = $"{dto.PlanEstudios} (Campus: {dto.ClaveCampus})";
                        if (!response.PlanesNoEncontrados.Contains(planCampusKey))
                            response.PlanesNoEncontrados.Add(planCampusKey);
                        throw new Exception($"Plan de estudios '{dto.PlanEstudios}' no encontrado en campus '{dto.ClaveCampus}'");
                    }

                    Materia? materia = null;
                    bool materiaCreada = false;

                    if (materiasDict.TryGetValue(claveNormalizada.ToLower(), out var materiaExistente))
                    {
                        materia = materiaExistente;

                        if (request.ActualizarExistentes)
                        {
                            materia.Nombre = nombreNormalizado;
                            materia.Creditos = dto.Creditos ?? 0;
                            materia.HorasTeoria = dto.HorasTeoria ?? 0;
                            materia.HorasPractica = dto.HorasPractica ?? 0;
                            materia.Activa = true;
                            response.MateriasActualizadas++;
                            resultado.Advertencias.Add("Materia actualizada");
                        }
                        else
                        {
                            resultado.Advertencias.Add("Materia ya existe, se usara la existente");
                        }
                    }
                    else
                    {
                        materia = new Materia
                        {
                            Clave = claveNormalizada,
                            Nombre = nombreNormalizado,
                            Creditos = dto.Creditos ?? 0,
                            HorasTeoria = dto.HorasTeoria ?? 0,
                            HorasPractica = dto.HorasPractica ?? 0,
                            Activa = true
                        };
                        _db.Materia.Add(materia);
                        await _db.SaveChangesAsync();

                        materiasDict[claveNormalizada.ToLower()] = materia;
                        materiaCreada = true;
                        response.MateriasCreadas++;
                    }

                    resultado.IdMateria = materia.IdMateria;

                    var materiaPlanExistente = await _db.MateriaPlan
                        .FirstOrDefaultAsync(mp =>
                            mp.IdMateria == materia.IdMateria &&
                            mp.IdPlanEstudios == plan.IdPlanEstudios &&
                            mp.Cuatrimestre == cuatrimestre &&
                            mp.Status == Core.Enums.StatusEnum.Active);

                    if (materiaPlanExistente == null)
                    {
                        var esOptativa = ParseBoolean(dto.EsOptativa);

                        var materiaPlan = new MateriaPlan
                        {
                            IdMateria = materia.IdMateria,
                            IdPlanEstudios = plan.IdPlanEstudios,
                            Cuatrimestre = (byte)cuatrimestre,
                            EsOptativa = esOptativa
                        };
                        _db.MateriaPlan.Add(materiaPlan);
                        await _db.SaveChangesAsync();

                        resultado.IdMateriaPlan = materiaPlan.IdMateriaPlan;
                        response.RelacionesCreadas++;
                    }
                    else
                    {
                        resultado.IdMateriaPlan = materiaPlanExistente.IdMateriaPlan;
                        resultado.Advertencias.Add("Relacion materia-plan ya existia");
                    }

                    resultado.Exito = true;
                    resultado.Mensaje = materiaCreada ? "Materia creada y asignada al plan" : "Materia asignada al plan";
                }
                catch (DbUpdateException dbEx)
                {
                    resultado.Exito = false;
                    resultado.Mensaje = dbEx.InnerException?.Message ?? dbEx.Message;
                    response.Fallidos++;
                }
                catch (Exception ex)
                {
                    resultado.Exito = false;
                    resultado.Mensaje = ex.Message;
                    response.Fallidos++;
                }

                response.Resultados.Add(resultado);
                response.TotalProcesados++;
            }

            return response;
        }

        public async Task<byte[]> GenerarPlantillaMateriasAsync(int? idPlanEstudios = null)
        {
            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Materias");

            var headers = new[] { "Clave", "Nombre", "PlanEstudios", "Cuatrimestre", "Creditos", "HorasTeoria", "HorasPractica", "EsOptativa", "Tipo" };
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
                worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightBlue;
            }

            int row = 2;

            if (idPlanEstudios.HasValue)
            {
                var materiasExistentes = await _db.MateriaPlan
                    .Include(mp => mp.IdMateriaNavigation)
                    .Include(mp => mp.IdPlanEstudiosNavigation)
                    .Where(mp => mp.IdPlanEstudios == idPlanEstudios.Value && mp.Status == Core.Enums.StatusEnum.Active)
                    .OrderBy(mp => mp.Cuatrimestre)
                    .ThenBy(mp => mp.IdMateriaNavigation.Clave)
                    .ToListAsync();

                foreach (var mp in materiasExistentes)
                {
                    worksheet.Cell(row, 1).Value = mp.IdMateriaNavigation.Clave;
                    worksheet.Cell(row, 2).Value = mp.IdMateriaNavigation.Nombre;
                    worksheet.Cell(row, 3).Value = mp.IdPlanEstudiosNavigation.NombrePlanEstudios;
                    worksheet.Cell(row, 4).Value = mp.Cuatrimestre;
                    worksheet.Cell(row, 5).Value = mp.IdMateriaNavigation.Creditos;
                    worksheet.Cell(row, 6).Value = mp.IdMateriaNavigation.HorasTeoria;
                    worksheet.Cell(row, 7).Value = mp.IdMateriaNavigation.HorasPractica;
                    worksheet.Cell(row, 8).Value = mp.EsOptativa ? "Si" : "No";
                    worksheet.Cell(row, 9).Value = "Formacion Academica";
                    row++;
                }
            }
            else
            {
                var ejemplos = new[]
                {
                    new { Clave = "MAT101", Nombre = "Matematicas I", Plan = "Ingenieria en Software", Cuat = "1", Creditos = "6", HT = "4", HP = "2", Optativa = "No", Tipo = "Formacion Academica" },
                    new { Clave = "FIS101", Nombre = "Fisica I", Plan = "Ingenieria en Software", Cuat = "1", Creditos = "6", HT = "4", HP = "2", Optativa = "No", Tipo = "Formacion Academica" },
                    new { Clave = "MAT201", Nombre = "Matematicas II", Plan = "Ingenieria en Software", Cuat = "2", Creditos = "6", HT = "4", HP = "2", Optativa = "No", Tipo = "Formacion Academica" },
                };

                foreach (var ej in ejemplos)
                {
                    worksheet.Cell(row, 1).Value = ej.Clave;
                    worksheet.Cell(row, 2).Value = ej.Nombre;
                    worksheet.Cell(row, 3).Value = ej.Plan;
                    worksheet.Cell(row, 4).Value = ej.Cuat;
                    worksheet.Cell(row, 5).Value = ej.Creditos;
                    worksheet.Cell(row, 6).Value = ej.HT;
                    worksheet.Cell(row, 7).Value = ej.HP;
                    worksheet.Cell(row, 8).Value = ej.Optativa;
                    worksheet.Cell(row, 9).Value = ej.Tipo;
                    row++;
                }
            }

            var instrucciones = workbook.Worksheets.Add("Instrucciones");
            instrucciones.Cell(1, 1).Value = "INSTRUCCIONES PARA IMPORTACION DE MATERIAS";
            instrucciones.Cell(1, 1).Style.Font.Bold = true;
            instrucciones.Cell(1, 1).Style.Font.FontSize = 14;

            instrucciones.Cell(3, 1).Value = "Columnas requeridas:";
            instrucciones.Cell(3, 1).Style.Font.Bold = true;
            instrucciones.Cell(4, 1).Value = "Clave: Clave unica de la materia (ej: MAT101, EECI101)";
            instrucciones.Cell(5, 1).Value = "Nombre: Nombre completo de la materia";
            instrucciones.Cell(6, 1).Value = "PlanEstudios: Nombre exacto del plan de estudios (debe existir en el sistema)";
            instrucciones.Cell(7, 1).Value = "Cuatrimestre: Numero del cuatrimestre (1, 2, 3, etc.) o texto (1ero., 2do., 3ero.)";

            instrucciones.Cell(9, 1).Value = "Columnas opcionales:";
            instrucciones.Cell(9, 1).Style.Font.Bold = true;
            instrucciones.Cell(10, 1).Value = "Creditos: Numero de creditos (default: 0)";
            instrucciones.Cell(11, 1).Value = "HorasTeoria: Horas de teoria por semana (default: 0)";
            instrucciones.Cell(12, 1).Value = "HorasPractica: Horas de practica por semana (default: 0)";
            instrucciones.Cell(13, 1).Value = "EsOptativa: Si/No o 1/0 (default: No)";
            instrucciones.Cell(14, 1).Value = "Tipo: Tipo de materia (informativo, no se guarda)";

            instrucciones.Cell(16, 1).Value = "Notas importantes:";
            instrucciones.Cell(16, 1).Style.Font.Bold = true;
            instrucciones.Cell(17, 1).Value = "El plan de estudios debe existir previamente en el sistema";
            instrucciones.Cell(18, 1).Value = "Si la materia ya existe (por clave), se reutilizara y solo se creara la asignacion al plan";
            instrucciones.Cell(19, 1).Value = "La misma materia puede asignarse a multiples planes de estudio";

            var planesSheet = workbook.Worksheets.Add("Planes Disponibles");
            planesSheet.Cell(1, 1).Value = "Planes de Estudio Disponibles";
            planesSheet.Cell(1, 1).Style.Font.Bold = true;

            var planes = await _db.PlanEstudios
                .Where(p => p.Status == Core.Enums.StatusEnum.Active)
                .OrderBy(p => p.NombrePlanEstudios)
                .Select(p => new { p.ClavePlanEstudios, p.NombrePlanEstudios })
                .ToListAsync();

            planesSheet.Cell(2, 1).Value = "Clave";
            planesSheet.Cell(2, 2).Value = "Nombre";
            planesSheet.Cell(2, 1).Style.Font.Bold = true;
            planesSheet.Cell(2, 2).Style.Font.Bold = true;

            int planRow = 3;
            foreach (var plan in planes)
            {
                planesSheet.Cell(planRow, 1).Value = plan.ClavePlanEstudios;
                planesSheet.Cell(planRow, 2).Value = plan.NombrePlanEstudios;
                planRow++;
            }

            worksheet.Columns().AdjustToContents();
            instrucciones.Column(1).Width = 80;
            planesSheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        private static int ParseCuatrimestre(string? cuatrimestre)
        {
            if (string.IsNullOrWhiteSpace(cuatrimestre))
                return 0;

            var cuatTrim = cuatrimestre.Trim().ToLower();

            if (int.TryParse(cuatTrim, out var num))
                return num;

            return ParsePeriodo(cuatrimestre);
        }

        private static bool ParseBoolean(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            var valueLower = value.Trim().ToLower();
            return valueLower == "si" || valueLower == "si" || valueLower == "yes" ||
                   valueLower == "1" || valueLower == "true" || valueLower == "verdadero";
        }
    }

    internal class InscripcionGrupoResult
    {
        public bool Exito { get; set; }
        public int? IdGrupo { get; set; }
        public List<string> Mensajes { get; set; } = new();
    }
}
