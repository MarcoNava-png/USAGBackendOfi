using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using WebApplication2.Configuration.Constants;
using WebApplication2.Core.Common;
using WebApplication2.Core.Models;
using WebApplication2.Core.Requests.MateriaPlan;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Services
{
    public class MateriaPlanService : IMateriaPlanService
    {
        private readonly ApplicationDbContext _dbContext;

        public MateriaPlanService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PagedResult<MateriaPlan>> GetMateriaPlanes(int page, int pageSize)
        {
            var totalItems = await _dbContext.MateriaPlan
                .Include(mp => mp.IdMateriaNavigation)
                .Include(mp => mp.IdPlanEstudiosNavigation)
                .Where(d => d.Status == Core.Enums.StatusEnum.Active)
                .CountAsync();

            var items = await _dbContext.MateriaPlan
                .Include(mp => mp.IdMateriaNavigation)
                .Include(mp => mp.IdPlanEstudiosNavigation)
                .Where(d => d.Status == Core.Enums.StatusEnum.Active)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<MateriaPlan>
            {
                TotalItems = totalItems,
                Items = items,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        public async Task<MateriaPlan> GetMateriaPlanDetalle(int id)
        {
            var materiaPlan = await _dbContext.MateriaPlan
                .Include(mp => mp.IdMateriaNavigation)
                .Include(mp => mp.IdPlanEstudiosNavigation)
                .Where(d => d.Status == Core.Enums.StatusEnum.Active)
                .FirstOrDefaultAsync(e => e.IdMateriaPlan == id && e.Status == Core.Enums.StatusEnum.Active);

            if (materiaPlan == null)
            {
                throw new Exception(ErrorConstants.RECORD_NOTFOUND);
            }

            return materiaPlan;
        }

        public async Task<MateriaPlan> CrearMateriaPlan(MateriaPlan materiaPlan)
        {
            await _dbContext.MateriaPlan.AddAsync(materiaPlan);
            await _dbContext.SaveChangesAsync();

            return materiaPlan;
        }

        public async Task<MateriaPlan> ActualizarMateriaPlan(MateriaPlan newMateriaPlan)
        {
            var materiaPlan = await _dbContext.MateriaPlan
                .FirstOrDefaultAsync(e => e.IdMateriaPlan == newMateriaPlan.IdMateriaPlan);

            if (materiaPlan == null)
            {
                throw new Exception(ErrorConstants.RECORD_NOTFOUND);
            }

            materiaPlan.IdPlanEstudios = newMateriaPlan.IdPlanEstudios;
            materiaPlan.IdMateria = newMateriaPlan.IdMateria;
            materiaPlan.Cuatrimestre = newMateriaPlan.Cuatrimestre;
            materiaPlan.EsOptativa = newMateriaPlan.EsOptativa;
            materiaPlan.Status = newMateriaPlan.Status;

            _dbContext.MateriaPlan.Update(materiaPlan);

            await _dbContext.SaveChangesAsync();

            return materiaPlan;
        }

        public async Task<(bool Exito, string Mensaje)> EliminarMateriaPlan(int id)
        {
            var materiaPlan = await _dbContext.MateriaPlan
                .Include(mp => mp.GrupoMateria)
                .FirstOrDefaultAsync(mp => mp.IdMateriaPlan == id);

            if (materiaPlan == null)
            {
                return (false, "No se encontro la materia");
            }

            var gruposMateriasActivos = materiaPlan.GrupoMateria
                .Count(gm => gm.Status == Core.Enums.StatusEnum.Active);

            if (gruposMateriasActivos > 0)
            {
                return (false, $"No se puede eliminar la materia porque esta asignada a {gruposMateriasActivos} grupo(s). Primero elimine la materia de los grupos.");
            }

            materiaPlan.Status = Core.Enums.StatusEnum.Deleted;
            materiaPlan.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return (true, "Materia eliminada correctamente");
        }

        public async Task<List<MateriaPlan>> GetMateriasPorPlanAsync(int idPlanEstudios)
        {
            return await _dbContext.MateriaPlan
                .Include(mp => mp.IdMateriaNavigation)
                .Where(mp => mp.IdPlanEstudios == idPlanEstudios && mp.Status == Core.Enums.StatusEnum.Active)
                .OrderBy(mp => mp.Cuatrimestre)
                .ThenBy(mp => mp.IdMateriaNavigation.Nombre)
                .ToListAsync();
        }

        public async Task<ImportarMateriasResponse> ImportarMateriasAsync(ImportarMateriasRequest request)
        {
            var response = new ImportarMateriasResponse();
            var detalle = new List<ImportarMateriaResultItem>();

            try
            {
                PlanEstudios? planEstudios = null;

                if (request.IdPlanEstudios.HasValue)
                {
                    planEstudios = await _dbContext.PlanEstudios
                        .FirstOrDefaultAsync(p => p.IdPlanEstudios == request.IdPlanEstudios.Value
                            && p.Status == Core.Enums.StatusEnum.Active);
                }
                else if (!string.IsNullOrWhiteSpace(request.ClavePlanEstudios))
                {
                    var claveBusqueda = request.ClavePlanEstudios.Trim();

                    planEstudios = await _dbContext.PlanEstudios
                        .FirstOrDefaultAsync(p =>
                            p.Status == Core.Enums.StatusEnum.Active &&
                            p.ClavePlanEstudios.ToLower() == claveBusqueda.ToLower());

                    if (planEstudios == null)
                    {
                        planEstudios = await _dbContext.PlanEstudios
                            .FirstOrDefaultAsync(p =>
                                p.Status == Core.Enums.StatusEnum.Active &&
                                p.ClavePlanEstudios.ToLower().Contains(claveBusqueda.ToLower()));
                    }

                    if (planEstudios == null)
                    {
                        planEstudios = await _dbContext.PlanEstudios
                            .FirstOrDefaultAsync(p =>
                                p.Status == Core.Enums.StatusEnum.Active &&
                                p.NombrePlanEstudios != null &&
                                p.NombrePlanEstudios.ToLower().Contains(claveBusqueda.ToLower()));
                    }
                }

                if (planEstudios == null)
                {
                    var planesDisponibles = await _dbContext.PlanEstudios
                        .Where(p => p.Status == Core.Enums.StatusEnum.Active)
                        .Select(p => new { p.IdPlanEstudios, p.ClavePlanEstudios, p.NombrePlanEstudios })
                        .Take(10)
                        .ToListAsync();

                    var listaPlanes = string.Join(", ", planesDisponibles.Select(p => $"{p.ClavePlanEstudios}"));

                    return new ImportarMateriasResponse
                    {
                        Exito = false,
                        Mensaje = $"No se encontro el Plan de Estudios '{request.ClavePlanEstudios ?? request.IdPlanEstudios?.ToString()}'. Planes disponibles: {listaPlanes}"
                    };
                }

                response.IdPlanEstudios = planEstudios.IdPlanEstudios;
                response.ClavePlanEstudios = planEstudios.ClavePlanEstudios;
                response.NombrePlanEstudios = planEstudios.NombrePlanEstudios;

                using var transaction = await _dbContext.Database.BeginTransactionAsync();

                try
                {
                    foreach (var item in request.Materias)
                    {
                        var cuatrimestre = (byte)ParseCuatrimestre(item.Grado);

                        var resultado = new ImportarMateriaResultItem
                        {
                            Clave = item.Clave?.Trim() ?? "",
                            Nombre = item.Nombre?.Trim() ?? "",
                            Cuatrimestre = cuatrimestre
                        };

                        try
                        {
                            if (string.IsNullOrWhiteSpace(item.Clave))
                            {
                                response.Errores++;
                                resultado.Estado = "Error";
                                resultado.MensajeError = "La clave de la materia es requerida";
                                detalle.Add(resultado);
                                response.TotalProcesadas++;
                                continue;
                            }

                            if (string.IsNullOrWhiteSpace(item.Nombre))
                            {
                                response.Errores++;
                                resultado.Estado = "Error";
                                resultado.MensajeError = "El nombre de la materia es requerido";
                                detalle.Add(resultado);
                                response.TotalProcesadas++;
                                continue;
                            }

                            var claveLimpia = item.Clave.Trim();
                            var nombreLimpio = item.Nombre.Trim();

                            var materia = await _dbContext.Materia
                                .FirstOrDefaultAsync(m => m.Clave.ToLower() == claveLimpia.ToLower());

                            if (materia == null)
                            {
                                materia = new Materia
                                {
                                    Clave = claveLimpia,
                                    Nombre = nombreLimpio,
                                    Creditos = item.Creditos,
                                    HorasTeoria = item.HorasTeoria,
                                    HorasPractica = item.HorasPractica,
                                    Activa = true,
                                    CreatedAt = DateTime.UtcNow,
                                    CreatedBy = "Sistema-Importacion",
                                    Status = Core.Enums.StatusEnum.Active
                                };

                                _dbContext.Materia.Add(materia);
                                await _dbContext.SaveChangesAsync();

                                response.MateriasCreadas++;
                                resultado.Estado = "Materia creada";
                            }
                            else
                            {
                                response.MateriasExistentes++;
                                resultado.Estado = "Materia existente";
                            }

                            resultado.IdMateria = materia.IdMateria;

                            var materiaPlanExistente = await _dbContext.MateriaPlan
                                .FirstOrDefaultAsync(mp =>
                                    mp.IdPlanEstudios == planEstudios.IdPlanEstudios &&
                                    mp.IdMateria == materia.IdMateria &&
                                    mp.Cuatrimestre == cuatrimestre &&
                                    mp.Status == Core.Enums.StatusEnum.Active);

                            if (materiaPlanExistente == null)
                            {
                                var nuevaMateriaPlan = new MateriaPlan
                                {
                                    IdPlanEstudios = planEstudios.IdPlanEstudios,
                                    IdMateria = materia.IdMateria,
                                    Cuatrimestre = cuatrimestre,
                                    EsOptativa = item.EsOptativa,
                                    CreatedAt = DateTime.UtcNow,
                                    CreatedBy = "Sistema-Importacion",
                                    Status = Core.Enums.StatusEnum.Active
                                };

                                _dbContext.MateriaPlan.Add(nuevaMateriaPlan);
                                await _dbContext.SaveChangesAsync();

                                response.AsignacionesCreadas++;
                                resultado.Estado += " | Asignacion creada";
                                resultado.IdMateriaPlan = nuevaMateriaPlan.IdMateriaPlan;
                            }
                            else
                            {
                                response.AsignacionesExistentes++;
                                resultado.Estado += " | Asignacion existente";
                                resultado.IdMateriaPlan = materiaPlanExistente.IdMateriaPlan;
                            }
                        }
                        catch (Exception ex)
                        {
                            response.Errores++;
                            resultado.Estado = "Error";
                            resultado.MensajeError = ex.Message;
                        }

                        detalle.Add(resultado);
                        response.TotalProcesadas++;
                    }

                    await transaction.CommitAsync();

                    response.Exito = true;
                    response.Mensaje = $"Importacion completada. {response.MateriasCreadas} materias creadas, " +
                                       $"{response.AsignacionesCreadas} asignaciones creadas.";
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception($"Error durante la importacion: {ex.Message}", ex);
                }
            }
            catch (Exception ex)
            {
                response.Exito = false;
                response.Mensaje = $"Error: {ex.Message}";
            }

            response.Detalle = detalle;
            return response;
        }

        private static int ParseCuatrimestre(string grado)
        {
            if (string.IsNullOrWhiteSpace(grado))
                return 1;

            if (int.TryParse(grado, out int numero))
                return numero;

            grado = grado.ToLower().Trim();

            var match = Regex.Match(grado, @"(\d+)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int num))
                return num;

            return grado switch
            {
                "primero" or "primer" or "1ero" or "1ero." => 1,
                "segundo" or "2do" or "2do." => 2,
                "tercero" or "tercer" or "3ero" or "3ero." or "3ro" => 3,
                "cuarto" or "4to" or "4to." => 4,
                "quinto" or "5to" or "5to." => 5,
                "sexto" or "6to" or "6to." => 6,
                "septimo" or "septimo" or "7mo" or "7mo." => 7,
                "octavo" or "8vo" or "8vo." => 8,
                "noveno" or "9no" or "9no." => 9,
                "decimo" or "decimo" or "10mo" or "10mo." => 10,
                _ => 1
            };
        }
    }
}
