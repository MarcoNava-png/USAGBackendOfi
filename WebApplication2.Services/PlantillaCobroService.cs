using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.DTOs.PlantillaCobro;
using WebApplication2.Core.DTOs.Recibo;
using WebApplication2.Core.Enums;
using WebApplication2.Core.Requests.Recibo;
using WebApplication2.Core.Responses.Recibo;
using WebApplication2.Core.Models;
using WebApplication2.Core.Requests.PlantillaCobro;
using WebApplication2.Core.Responses.PlantillaCobro;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Services
{
    public class PlantillaCobroService : IPlantillaCobroService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        private readonly IBecaService _becaService;

        public PlantillaCobroService(ApplicationDbContext db, IMapper mapper, IBecaService becaService)
        {
            _db = db;
            _mapper = mapper;
            _becaService = becaService;
        }

        public async Task<IReadOnlyList<PlantillaCobroDto>> ListarPlantillasAsync(
            int? idPlanEstudios = null,
            int? numeroCuatrimestre = null,
            bool? soloActivas = null,
            int? idPeriodoAcademico = null,
            CancellationToken ct = default)
        {
            var query = _db.PlantillasCobro
                .Include(p => p.IdPlanEstudiosNavigation)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.IdConceptoPagoNavigation)
                .AsNoTracking();

            if (idPlanEstudios.HasValue)
                query = query.Where(p => p.IdPlanEstudios == idPlanEstudios.Value);

            if (numeroCuatrimestre.HasValue)
                query = query.Where(p => p.NumeroCuatrimestre == numeroCuatrimestre.Value);

            if (soloActivas.HasValue && soloActivas.Value)
                query = query.Where(p => p.EsActiva);

            if (idPeriodoAcademico.HasValue)
                query = query.Where(p => p.IdPeriodoAcademico == idPeriodoAcademico.Value || p.IdPeriodoAcademico == null);

            var plantillas = await query
                .OrderBy(p => p.IdPlanEstudios)
                .ThenBy(p => p.NumeroCuatrimestre)
                .ThenByDescending(p => p.Version)
                .ToListAsync(ct);

            return _mapper.Map<IReadOnlyList<PlantillaCobroDto>>(plantillas);
        }

        public async Task<PlantillaCobroDto?> ObtenerPlantillaPorIdAsync(int id, CancellationToken ct = default)
        {
            var plantilla = await _db.PlantillasCobro
                .Include(p => p.IdPlanEstudiosNavigation)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.IdConceptoPagoNavigation)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.IdPlantillaCobro == id, ct);

            if (plantilla == null)
                return null;

            return _mapper.Map<PlantillaCobroDto>(plantilla);
        }

        public async Task<PlantillaCobroDto?> BuscarPlantillaActivaAsync(
            int idPlanEstudios,
            int numeroCuatrimestre,
            int? idPeriodoAcademico = null,
            int? idTurno = null,
            int? idModalidad = null,
            CancellationToken ct = default)
        {
            var query = _db.PlantillasCobro
                .Include(p => p.IdPlanEstudiosNavigation)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.IdConceptoPagoNavigation)
                .AsNoTracking()
                .Where(p => p.EsActiva
                    && p.IdPlanEstudios == idPlanEstudios
                    && p.NumeroCuatrimestre == numeroCuatrimestre);

            if (idPeriodoAcademico.HasValue)
                query = query.Where(p => p.IdPeriodoAcademico == idPeriodoAcademico.Value || p.IdPeriodoAcademico == null);

            if (idTurno.HasValue)
                query = query.Where(p => p.IdTurno == idTurno.Value || p.IdTurno == null);

            if (idModalidad.HasValue)
                query = query.Where(p => p.IdModalidad == idModalidad.Value || p.IdModalidad == null);

            var plantilla = await query
                .OrderByDescending(p =>
                    (p.IdPeriodoAcademico.HasValue ? 1 : 0) +
                    (p.IdTurno.HasValue ? 1 : 0) +
                    (p.IdModalidad.HasValue ? 1 : 0))
                .ThenByDescending(p => p.Version)
                .FirstOrDefaultAsync(ct);

            if (plantilla == null)
                return null;

            return _mapper.Map<PlantillaCobroDto>(plantilla);
        }

        public async Task<PlantillaCobroDto> CrearPlantillaAsync(
            CreatePlantillaCobroDto dto,
            string usuarioCreador,
            CancellationToken ct = default)
        {
            var planEstudios = await _db.PlanEstudios.FindAsync(new object[] { dto.IdPlanEstudios }, ct);
            if (planEstudios == null)
                throw new InvalidOperationException($"Plan de estudios con ID {dto.IdPlanEstudios} no encontrado");

            var idsConceptos = dto.Detalles.Select(d => d.IdConceptoPago).Distinct().ToList();
            var conceptosExistentes = await _db.ConceptoPago
                .Where(c => idsConceptos.Contains(c.IdConceptoPago))
                .CountAsync(ct);

            if (conceptosExistentes != idsConceptos.Count)
                throw new InvalidOperationException("Uno o más conceptos de pago no existen");

            var plantilla = _mapper.Map<PlantillaCobro>(dto);
            plantilla.CreadoPor = usuarioCreador;
            plantilla.FechaCreacion = DateTime.UtcNow;

            _db.PlantillasCobro.Add(plantilla);
            await _db.SaveChangesAsync(ct);

            await _db.Entry(plantilla)
                .Reference(p => p.IdPlanEstudiosNavigation)
                .LoadAsync(ct);

            await _db.Entry(plantilla)
                .Collection(p => p.Detalles)
                .LoadAsync(ct);

            foreach (var detalle in plantilla.Detalles)
            {
                await _db.Entry(detalle)
                    .Reference(d => d.IdConceptoPagoNavigation)
                    .LoadAsync(ct);
            }

            return _mapper.Map<PlantillaCobroDto>(plantilla);
        }

        public async Task<PlantillaCobroDto> ActualizarPlantillaAsync(
            int id,
            UpdatePlantillaCobroDto dto,
            string usuarioModificador,
            CancellationToken ct = default)
        {
            var plantilla = await _db.PlantillasCobro
                .Include(p => p.Detalles)
                .FirstOrDefaultAsync(p => p.IdPlantillaCobro == id, ct);

            if (plantilla == null)
                throw new InvalidOperationException($"Plantilla con ID {id} no encontrada");

            if (dto.NombrePlantilla != null)
                plantilla.NombrePlantilla = dto.NombrePlantilla;

            if (dto.FechaVigenciaInicio.HasValue)
                plantilla.FechaVigenciaInicio = dto.FechaVigenciaInicio.Value;

            if (dto.FechaVigenciaFin.HasValue)
                plantilla.FechaVigenciaFin = dto.FechaVigenciaFin;

            if (dto.EstrategiaEmision.HasValue)
                plantilla.EstrategiaEmision = dto.EstrategiaEmision.Value;

            if (dto.NumeroRecibos.HasValue)
                plantilla.NumeroRecibos = dto.NumeroRecibos.Value;

            if (dto.DiaVencimiento.HasValue)
                plantilla.DiaVencimiento = dto.DiaVencimiento.Value;

            if (dto.Detalles != null && dto.Detalles.Any())
            {
                var existentes = plantilla.Detalles.OrderBy(d => d.Orden).ToList();
                var nuevos = dto.Detalles.OrderBy(d => d.Orden).ToList();

                int minCount = Math.Min(existentes.Count, nuevos.Count);

                for (int i = 0; i < minCount; i++)
                {
                    existentes[i].IdConceptoPago = nuevos[i].IdConceptoPago;
                    existentes[i].Descripcion = nuevos[i].Descripcion;
                    existentes[i].Cantidad = nuevos[i].Cantidad;
                    existentes[i].PrecioUnitario = nuevos[i].PrecioUnitario;
                    existentes[i].Orden = nuevos[i].Orden;
                    existentes[i].AplicaEnRecibo = nuevos[i].AplicaEnRecibo;
                }

                if (nuevos.Count > existentes.Count)
                {
                    for (int i = existentes.Count; i < nuevos.Count; i++)
                    {
                        var nuevoDetalle = _mapper.Map<PlantillaCobroDetalle>(nuevos[i]);
                        nuevoDetalle.IdPlantillaCobro = id;
                        plantilla.Detalles.Add(nuevoDetalle);
                    }
                }
                else if (existentes.Count > nuevos.Count)
                {
                    var sobrantes = existentes.Skip(nuevos.Count).ToList();
                    _db.PlantillasCobroDetalles.RemoveRange(sobrantes);
                }
            }

            plantilla.ModificadoPor = usuarioModificador;
            plantilla.FechaModificacion = DateTime.UtcNow;
            plantilla.Version++;

            await _db.SaveChangesAsync(ct);

            await _db.Entry(plantilla)
                .Reference(p => p.IdPlanEstudiosNavigation)
                .LoadAsync(ct);

            foreach (var detalle in plantilla.Detalles)
            {
                await _db.Entry(detalle)
                    .Reference(d => d.IdConceptoPagoNavigation)
                    .LoadAsync(ct);
            }

            return _mapper.Map<PlantillaCobroDto>(plantilla);
        }

        public async Task CambiarEstadoPlantillaAsync(int id, bool esActiva, CancellationToken ct = default)
        {
            var plantilla = await _db.PlantillasCobro.FindAsync(new object[] { id }, ct);

            if (plantilla == null)
                throw new InvalidOperationException($"Plantilla con ID {id} no encontrada");

            plantilla.EsActiva = esActiva;
            await _db.SaveChangesAsync(ct);
        }

        public async Task EliminarPlantillaAsync(int id, CancellationToken ct = default)
        {
            var plantilla = await _db.PlantillasCobro
                .Include(p => p.Detalles)
                .FirstOrDefaultAsync(p => p.IdPlantillaCobro == id, ct);

            if (plantilla == null)
                throw new InvalidOperationException($"Plantilla con ID {id} no encontrada");

            _db.PlantillasCobroDetalles.RemoveRange(plantilla.Detalles);
            _db.PlantillasCobro.Remove(plantilla);
            await _db.SaveChangesAsync(ct);
        }

        public async Task<PlantillaCobroDto> DuplicarPlantillaAsync(
            int id,
            CreatePlantillaCobroDto? cambios,
            string usuarioCreador,
            CancellationToken ct = default)
        {
            var plantillaOriginal = await _db.PlantillasCobro
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.IdConceptoPagoNavigation)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.IdPlantillaCobro == id, ct);

            if (plantillaOriginal == null)
                throw new InvalidOperationException($"Plantilla con ID {id} no encontrada");

            CreatePlantillaCobroDto nuevaPlantillaDto;

            if (cambios != null)
            {
                nuevaPlantillaDto = cambios;
            }
            else
            {
                nuevaPlantillaDto = new CreatePlantillaCobroDto
                {
                    NombrePlantilla = plantillaOriginal.NombrePlantilla + " (Copia)",
                    IdPlanEstudios = plantillaOriginal.IdPlanEstudios,
                    NumeroCuatrimestre = plantillaOriginal.NumeroCuatrimestre,
                    IdPeriodoAcademico = plantillaOriginal.IdPeriodoAcademico,
                    IdTurno = plantillaOriginal.IdTurno,
                    IdModalidad = plantillaOriginal.IdModalidad,
                    FechaVigenciaInicio = DateTime.UtcNow.Date,
                    FechaVigenciaFin = plantillaOriginal.FechaVigenciaFin,
                    EstrategiaEmision = plantillaOriginal.EstrategiaEmision,
                    NumeroRecibos = plantillaOriginal.NumeroRecibos,
                    DiaVencimiento = plantillaOriginal.DiaVencimiento,
                    Detalles = plantillaOriginal.Detalles.Select(d => new CreatePlantillaCobroDetalleDto
                    {
                        IdConceptoPago = d.IdConceptoPago,
                        Descripcion = d.Descripcion,
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.PrecioUnitario,
                        Orden = d.Orden,
                        AplicaEnRecibo = d.AplicaEnRecibo
                    }).ToList()
                };
            }

            return await CrearPlantillaAsync(nuevaPlantillaDto, usuarioCreador, ct);
        }

        public async Task<IReadOnlyList<int>> ObtenerCuatrimestresPorPlanAsync(int idPlanEstudios, CancellationToken ct = default)
        {
            var plan = await _db.PlanEstudios
                .Include(p => p.IdPeriodicidadNavigation)
                .FirstOrDefaultAsync(p => p.IdPlanEstudios == idPlanEstudios, ct);

            if (plan == null)
                throw new InvalidOperationException($"Plan de estudios con ID {idPlanEstudios} no encontrado");

            int numeroCuatrimestres = 9;

            if (plan.DuracionMeses.HasValue)
            {
                numeroCuatrimestres = (int)Math.Ceiling(plan.DuracionMeses.Value / 4.0);
            }

            return Enumerable.Range(1, numeroCuatrimestres).ToList();
        }

        public async Task<GenerarRecibosMasivosResult> GenerarRecibosMasivosAsync(
            GenerarRecibosMasivosRequest request,
            string usuarioCreador,
            CancellationToken ct = default)
        {
            var result = new GenerarRecibosMasivosResult
            {
                Errores = new List<string>(),
                DetalleEstudiantes = new List<ReciboEstudianteResumen>()
            };

            var plantilla = await _db.PlantillasCobro
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.IdConceptoPagoNavigation)
                .Include(p => p.IdPlanEstudiosNavigation)
                .FirstOrDefaultAsync(p => p.IdPlantillaCobro == request.IdPlantillaCobro, ct);

            if (plantilla == null)
            {
                result.Mensaje = "Plantilla de cobro no encontrada";
                return result;
            }

            if (!plantilla.EsActiva)
            {
                result.Mensaje = "La plantilla no está activa";
                return result;
            }

            var periodo = await _db.PeriodoAcademico
                .FirstOrDefaultAsync(p => p.IdPeriodoAcademico == request.IdPeriodoAcademico, ct);

            if (periodo == null)
            {
                result.Mensaje = "Periodo académico no encontrado";
                return result;
            }

            var estudiantesQuery = _db.Estudiante
                .Include(e => e.IdPersonaNavigation)
                .Where(e => e.Activo
                         && e.Status == StatusEnum.Active
                         && e.IdPlanActual == plantilla.IdPlanEstudios);

            if (request.IdEstudiantes != null && request.IdEstudiantes.Any())
            {
                estudiantesQuery = estudiantesQuery.Where(e => request.IdEstudiantes.Contains(e.IdEstudiante));
            }
            else
            {
                var estudiantesPorInscripcion = await _db.Inscripcion
                    .Include(i => i.IdGrupoMateriaNavigation)
                        .ThenInclude(gm => gm.IdGrupoNavigation)
                    .Where(i => i.IdGrupoMateriaNavigation.IdGrupoNavigation.IdPeriodoAcademico == request.IdPeriodoAcademico
                             && i.IdGrupoMateriaNavigation.IdGrupoNavigation.NumeroCuatrimestre == plantilla.NumeroCuatrimestre
                             && i.Status == StatusEnum.Active)
                    .Select(i => i.IdEstudiante)
                    .Distinct()
                    .ToListAsync(ct);

                var estudiantesPorGrupo = await _db.EstudianteGrupo
                    .Include(eg => eg.IdGrupoNavigation)
                    .Where(eg => eg.IdGrupoNavigation.IdPeriodoAcademico == request.IdPeriodoAcademico
                              && eg.IdGrupoNavigation.NumeroCuatrimestre == plantilla.NumeroCuatrimestre
                              && eg.IdGrupoNavigation.IdPlanEstudios == plantilla.IdPlanEstudios
                              && eg.Estado == "Inscrito"
                              && eg.Status == StatusEnum.Active)
                    .Select(eg => eg.IdEstudiante)
                    .Distinct()
                    .ToListAsync(ct);

                var estudiantesEnPeriodo = estudiantesPorInscripcion
                    .Union(estudiantesPorGrupo)
                    .Distinct()
                    .ToList();

                estudiantesQuery = estudiantesQuery.Where(e => estudiantesEnPeriodo.Contains(e.IdEstudiante));
            }

            var estudiantes = await estudiantesQuery.ToListAsync(ct);

            if (!estudiantes.Any())
            {
                result.Mensaje = "No se encontraron estudiantes que cumplan los criterios";
                result.Exitoso = true;
                return result;
            }

            if (request.SoloSimular)
            {
                int estudiantesConRecibos = 0;

                foreach (var estudiante in estudiantes)
                {
                    var yaExistenRecibos = await _db.Recibo
                        .AnyAsync(r => r.IdEstudiante == estudiante.IdEstudiante
                                    && r.IdPeriodoAcademico == request.IdPeriodoAcademico
                                    && r.Status == StatusEnum.Active, ct);

                    if (yaExistenRecibos)
                    {
                        estudiantesConRecibos++;
                        result.Errores.Add($"Estudiante {estudiante.Matricula} ({estudiante.IdPersonaNavigation.Nombre} {estudiante.IdPersonaNavigation.ApellidoPaterno}) ya tiene recibos generados");
                        continue;
                    }

                    var montoTotal = plantilla.Detalles.Sum(d =>
                    {
                        var importe = d.Cantidad * d.PrecioUnitario;
                        if (d.AplicaEnRecibo == null)
                            return importe * plantilla.NumeroRecibos;
                        return importe;
                    });

                    var descuento = await CalcularDescuentoTotalAsync(estudiante.IdEstudiante, plantilla.Detalles, periodo.FechaInicio, ct);

                    result.DetalleEstudiantes.Add(new ReciboEstudianteResumen
                    {
                        IdEstudiante = estudiante.IdEstudiante,
                        Matricula = estudiante.Matricula,
                        NombreCompleto = $"{estudiante.IdPersonaNavigation.Nombre} {estudiante.IdPersonaNavigation.ApellidoPaterno}",
                        RecibosGenerados = plantilla.NumeroRecibos,
                        MontoTotal = montoTotal,
                        DescuentoBecas = descuento,
                        SaldoFinal = montoTotal - descuento
                    });
                }

                result.EstudiantesOmitidos = estudiantesConRecibos;
                result.Exitoso = true;
                result.TotalEstudiantes = result.DetalleEstudiantes.Count;
                result.TotalRecibosGenerados = result.DetalleEstudiantes.Count * plantilla.NumeroRecibos;
                result.MontoTotal = result.DetalleEstudiantes.Sum(d => d.MontoTotal);
                result.TotalDescuentosBecas = result.DetalleEstudiantes.Sum(d => d.DescuentoBecas);

                if (estudiantesConRecibos > 0)
                {
                    result.Mensaje = $"Simulación: Se generarían {result.TotalRecibosGenerados} recibos para {result.TotalEstudiantes} estudiantes. ({estudiantesConRecibos} estudiantes ya tienen recibos y serán omitidos)";
                }
                else
                {
                    result.Mensaje = $"Simulación: Se generarían {result.TotalRecibosGenerados} recibos para {result.TotalEstudiantes} estudiantes";
                }

                return result;
            }

            using var transaction = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                foreach (var estudiante in estudiantes)
                {
                    var recibosExistentes = await _db.Recibo
                        .Include(r => r.Detalles)
                        .Where(r => r.IdEstudiante == estudiante.IdEstudiante
                                    && r.IdPeriodoAcademico == request.IdPeriodoAcademico
                                    && r.Status == StatusEnum.Active)
                        .ToListAsync(ct);

                    bool yaExistenRecibosDeEstaPlantilla = recibosExistentes
                        .Any(r => r.Detalles.Any(d =>
                            d.RefTabla == "PlantillaCobroDetalle" &&
                            plantilla.Detalles.Any(pd => pd.IdPlantillaDetalle == d.RefId)));

                    if (yaExistenRecibosDeEstaPlantilla)
                    {
                        result.EstudiantesOmitidos++;
                        result.Errores.Add($"Estudiante {estudiante.Matricula} ya tiene recibos generados de esta plantilla para este periodo");
                        continue;
                    }

                    if (recibosExistentes.Any())
                    {
                        result.EstudiantesOmitidos++;
                        result.Errores.Add($"Estudiante {estudiante.Matricula} ya tiene {recibosExistentes.Count} recibo(s) en este periodo (de otra plantilla)");
                        continue;
                    }

                    var resumenEstudiante = await GenerarRecibosParaEstudianteAsync(
                        estudiante,
                        plantilla,
                        periodo,
                        usuarioCreador,
                        ct);

                    result.DetalleEstudiantes.Add(resumenEstudiante);
                    result.TotalRecibosGenerados += resumenEstudiante.RecibosGenerados;
                    result.MontoTotal += resumenEstudiante.MontoTotal;
                    result.TotalDescuentosBecas += resumenEstudiante.DescuentoBecas;
                }

                await transaction.CommitAsync(ct);

                result.Exitoso = true;
                result.TotalEstudiantes = estudiantes.Count;
                result.Mensaje = $"Se generaron {result.TotalRecibosGenerados} recibos para {result.TotalEstudiantes - result.EstudiantesOmitidos} estudiantes";

                if (result.EstudiantesOmitidos > 0)
                    result.Mensaje += $" ({result.EstudiantesOmitidos} omitidos por ya tener recibos)";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                result.Exitoso = false;
                result.Mensaje = "Error al generar recibos";
                result.Errores.Add(ex.Message);
            }

            return result;
        }

        private async Task<ReciboEstudianteResumen> GenerarRecibosParaEstudianteAsync(
            Estudiante estudiante,
            PlantillaCobro plantilla,
            PeriodoAcademico periodo,
            string usuarioCreador,
            CancellationToken ct)
        {
            var resumen = new ReciboEstudianteResumen
            {
                IdEstudiante = estudiante.IdEstudiante,
                Matricula = estudiante.Matricula,
                NombreCompleto = $"{estudiante.IdPersonaNavigation.Nombre} {estudiante.IdPersonaNavigation.ApellidoPaterno}"
            };

            if (plantilla.EstrategiaEmision == 0)
            {
                for (int numeroRecibo = 1; numeroRecibo <= plantilla.NumeroRecibos; numeroRecibo++)
                {
                    var detallesParaEsteRecibo = plantilla.Detalles
                        .Where(d =>
                        {
                            if (d.AplicaEnRecibo == null || d.AplicaEnRecibo == 0) return true;
                            if (d.AplicaEnRecibo == 1 && numeroRecibo == 1) return true;
                            if (d.AplicaEnRecibo == -1 && numeroRecibo == plantilla.NumeroRecibos) return true;
                            if (d.AplicaEnRecibo == numeroRecibo) return true;
                            return false;
                        })
                        .OrderBy(d => d.Orden)
                        .ToList();

                    if (!detallesParaEsteRecibo.Any()) continue;

                    var fechaVencimiento = CalcularFechaVencimiento(
                        periodo.FechaInicio,
                        (numeroRecibo - 1),
                        (byte)plantilla.DiaVencimiento);

                    await CrearReciboAsync(
                        estudiante.IdEstudiante,
                        periodo.IdPeriodoAcademico,
                        detallesParaEsteRecibo,
                        fechaVencimiento,
                        usuarioCreador,
                        resumen,
                        ct);
                }
            }
            else
            {
                var detallesOrdenados = plantilla.Detalles
                    .OrderBy(d => d.Orden)
                    .ToList();

                var fechaVencimiento = CalcularFechaVencimiento(
                    periodo.FechaInicio,
                    0,
                    (byte)plantilla.DiaVencimiento);

                await CrearReciboAsync(
                    estudiante.IdEstudiante,
                    periodo.IdPeriodoAcademico,
                    detallesOrdenados,
                    fechaVencimiento,
                    usuarioCreador,
                    resumen,
                    ct);
            }

            return resumen;
        }

        private async Task CrearReciboAsync(
            int idEstudiante,
            int idPeriodoAcademico,
            List<PlantillaCobroDetalle> detalles,
            DateOnly fechaVencimiento,
            string usuarioCreador,
            ReciboEstudianteResumen resumen,
            CancellationToken ct)
        {
            decimal subtotal = detalles.Sum(d => d.Cantidad * d.PrecioUnitario);

            decimal descuentoTotal = 0;
            foreach (var detalle in detalles)
            {
                var descuento = await _becaService.CalcularDescuentoPorBecasAsync(
                    idEstudiante,
                    detalle.IdConceptoPago,
                    detalle.Cantidad * detalle.PrecioUnitario,
                    fechaVencimiento,
                    ct);
                descuentoTotal += descuento;
            }

            var recibo = new Recibo
            {
                Folio = await GenerarFolioAsync(ct),
                IdEstudiante = idEstudiante,
                IdPeriodoAcademico = idPeriodoAcademico,
                FechaEmision = DateOnly.FromDateTime(DateTime.UtcNow),
                FechaVencimiento = fechaVencimiento,
                Estatus = EstatusRecibo.PENDIENTE,
                Subtotal = subtotal,
                Descuento = descuentoTotal,
                Recargos = 0,
                Saldo = subtotal - descuentoTotal,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = usuarioCreador,
                Status = StatusEnum.Active
            };

            await _db.Recibo.AddAsync(recibo, ct);
            await _db.SaveChangesAsync(ct);

            foreach (var detalle in detalles)
            {
                var descripcionProcesada = ProcesarDescripcionConPlaceholders(
                    detalle.Descripcion,
                    fechaVencimiento);

                var reciboDetalle = new ReciboDetalle
                {
                    IdRecibo = recibo.IdRecibo,
                    IdConceptoPago = detalle.IdConceptoPago,
                    Descripcion = descripcionProcesada,
                    Cantidad = (int)detalle.Cantidad,
                    PrecioUnitario = detalle.PrecioUnitario,
                    RefTabla = "PlantillaCobroDetalle",
                    RefId = detalle.IdPlantillaDetalle,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = usuarioCreador,
                    Status = StatusEnum.Active
                };

                await _db.ReciboDetalle.AddAsync(reciboDetalle, ct);
            }

            await _db.SaveChangesAsync(ct);

            resumen.RecibosGenerados++;
            resumen.MontoTotal += subtotal;
            resumen.DescuentoBecas += descuentoTotal;
            resumen.SaldoFinal += (subtotal - descuentoTotal);
        }

        private async Task<decimal> CalcularDescuentoTotalAsync(
            int idEstudiante,
            ICollection<PlantillaCobroDetalle> detalles,
            DateOnly fechaReferencia,
            CancellationToken ct)
        {
            decimal descuentoTotal = 0;

            foreach (var detalle in detalles)
            {
                var descuento = await _becaService.CalcularDescuentoPorBecasAsync(
                    idEstudiante,
                    detalle.IdConceptoPago,
                    detalle.Cantidad * detalle.PrecioUnitario,
                    fechaReferencia,
                    ct);
                descuentoTotal += descuento;
            }

            return descuentoTotal;
        }

        private static string ProcesarDescripcionConPlaceholders(string? descripcion, DateOnly fechaVencimiento)
        {
            if (string.IsNullOrEmpty(descripcion))
                return descripcion ?? string.Empty;

            var nombresMeses = new[]
            {
                "", "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio",
                "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre"
            };

            var mes = fechaVencimiento.Month;
            var año = fechaVencimiento.Year;
            var nombreMes = nombresMeses[mes];

            var resultado = descripcion
                .Replace("{Mes}", nombreMes, StringComparison.OrdinalIgnoreCase)
                .Replace("{MesAño}", $"{nombreMes} {año}", StringComparison.OrdinalIgnoreCase)
                .Replace("{MesAnio}", $"{nombreMes} {año}", StringComparison.OrdinalIgnoreCase)
                .Replace("{Año}", año.ToString(), StringComparison.OrdinalIgnoreCase)
                .Replace("{Anio}", año.ToString(), StringComparison.OrdinalIgnoreCase)
                .Replace("{NumeroMes}", mes.ToString("D2"), StringComparison.OrdinalIgnoreCase);

            return resultado;
        }

        private async Task<string> GenerarFolioAsync(CancellationToken ct)
        {
            var año = DateTime.UtcNow.Year;
            var prefijo = $"REC-{año}-";

            var ultimoFolio = await _db.Recibo
                .Where(r => r.Folio != null && r.Folio.StartsWith(prefijo))
                .OrderByDescending(r => r.Folio)
                .Select(r => r.Folio)
                .FirstOrDefaultAsync(ct);

            int siguienteNumero = 1;

            if (ultimoFolio != null)
            {
                var numeroStr = ultimoFolio.Substring(prefijo.Length);
                if (int.TryParse(numeroStr, out int numero))
                {
                    siguienteNumero = numero + 1;
                }
            }

            return $"{prefijo}{siguienteNumero:D6}";
        }

        private DateOnly CalcularFechaVencimiento(DateOnly fechaInicio, int mesOffset, byte diaPago)
        {
            var fecha = fechaInicio.AddMonths(mesOffset);
            int diaMaximo = DateTime.DaysInMonth(fecha.Year, fecha.Month);
            int diaFinal = Math.Min(diaPago, diaMaximo);

            return new DateOnly(fecha.Year, fecha.Month, diaFinal);
        }

        public PreviewRecibosResponse GenerarPreviewRecibos(GenerarPreviewRecibosRequest request)
        {
            var response = new PreviewRecibosResponse();

            var fechaInicio = request.FechaInicioPeriodo.HasValue
                ? DateOnly.FromDateTime(request.FechaInicioPeriodo.Value)
                : DateOnly.FromDateTime(DateTime.Now);

            var nombresMeses = new[] {
                "", "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio",
                "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre"
            };

            for (int i = 1; i <= request.NumeroRecibos; i++)
            {
                var fechaVencimiento = CalcularFechaVencimiento(
                    fechaInicio,
                    i - 1,
                    (byte)request.DiaVencimiento);

                var recibo = new ReciboPreviewDto
                {
                    NumeroRecibo = i,
                    FechaVencimiento = fechaVencimiento.ToString("dd/MM/yyyy"),
                    MesCorrespondiente = $"{nombresMeses[fechaVencimiento.Month]} {fechaVencimiento.Year}",
                    Conceptos = new List<ReciboPreviewDetalleDto>()
                };

                foreach (var concepto in request.Conceptos)
                {
                    bool incluir = concepto.AplicaEnRecibo == null
                        || concepto.AplicaEnRecibo == i
                        || (concepto.AplicaEnRecibo == -1 && i == request.NumeroRecibos);

                    if (incluir)
                    {
                        var descripcionProcesada = ProcesarDescripcionConPlaceholders(
                            concepto.Descripcion,
                            fechaVencimiento);

                        recibo.Conceptos.Add(new ReciboPreviewDetalleDto
                        {
                            Concepto = descripcionProcesada,
                            Cantidad = concepto.Cantidad,
                            PrecioUnitario = concepto.PrecioUnitario,
                            Importe = concepto.Cantidad * concepto.PrecioUnitario
                        });
                    }
                }

                recibo.Subtotal = recibo.Conceptos.Sum(c => c.Importe);
                response.Recibos.Add(recibo);
            }

            if (response.Recibos.Any())
            {
                response.TotalPrimerRecibo = response.Recibos.First().Subtotal;
                response.TotalRecibosRegulares = response.Recibos.Skip(1).FirstOrDefault()?.Subtotal ?? 0;
                response.TotalGeneral = response.Recibos.Sum(r => r.Subtotal);
            }

            return response;
        }
    }
}
