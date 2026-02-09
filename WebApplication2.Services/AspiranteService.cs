using Microsoft.EntityFrameworkCore;
using WebApplication2.Core.Common;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.DTOs.Admision;
using WebApplication2.Core.DTOs.Inscripcion;
using WebApplication2.Core.DTOs.PlantillaCobro;
using WebApplication2.Core.Enums;
using WebApplication2.Core.Models;
using WebApplication2.Data.DbContexts;
using WebApplication2.Data.Migrations;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Services
{
    public class AspiranteService : IAspiranteService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IMatriculaService? _matriculaService;
        private readonly IEstudianteService? _estudianteService;
        private readonly IAuthService? _authService;
        private readonly IPlantillaCobroService? _plantillaCobroService;
        private readonly IConvenioService? _convenioService;
        private readonly IReciboService? _reciboService;

        public AspiranteService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public AspiranteService(
            ApplicationDbContext dbContext,
            IMatriculaService matriculaService,
            IEstudianteService estudianteService,
            IAuthService authService)
        {
            _dbContext = dbContext;
            _matriculaService = matriculaService;
            _estudianteService = estudianteService;
            _authService = authService;
        }

        public AspiranteService(
            ApplicationDbContext dbContext,
            IMatriculaService matriculaService,
            IEstudianteService estudianteService,
            IAuthService authService,
            IPlantillaCobroService plantillaCobroService,
            IConvenioService convenioService,
            IReciboService reciboService)
        {
            _dbContext = dbContext;
            _matriculaService = matriculaService;
            _estudianteService = estudianteService;
            _authService = authService;
            _plantillaCobroService = plantillaCobroService;
            _convenioService = convenioService;
            _reciboService = reciboService;
        }

        public async Task<PagedResult<Aspirante>> GetAspirantes(int page, int pageSize, string filter)
        {
            var baseQuery = _dbContext.Aspirante
                .Include(a => a.IdPlanNavigation)
                .Include(a => a.IdAspiranteEstatusNavigation)
                .Include(a => a.IdPersonaNavigation)
                    .ThenInclude(p => p.IdDireccionNavigation)
                        .ThenInclude(d => d.CodigoPostal)
                            .ThenInclude(cp => cp.Municipio)
                                .ThenInclude(m => m.Estado)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter))
            {
                var filterLower = filter.ToLower();
                baseQuery = baseQuery.Where(a =>
                    a.IdPersonaNavigation.Nombre.ToLower().Contains(filterLower) ||
                    a.IdPersonaNavigation.ApellidoPaterno.ToLower().Contains(filterLower) ||
                    (a.IdPersonaNavigation.ApellidoMaterno != null && a.IdPersonaNavigation.ApellidoMaterno.ToLower().Contains(filterLower)) ||
                    (a.IdPersonaNavigation.Curp != null && a.IdPersonaNavigation.Curp.ToLower().Contains(filterLower)));
            }

            baseQuery = baseQuery.Where(a =>
                a.IdAspiranteEstatusNavigation == null ||
                a.IdAspiranteEstatusNavigation.DescEstatus != "Cancelado");

            var totalItems = await baseQuery.CountAsync();

            var aspirantes = await baseQuery
                .OrderByDescending(a => a.IdAspirante)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Aspirante>
            {
                TotalItems = totalItems,
                Items = aspirantes,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        public async Task<Aspirante> GetAspiranteByPersonaId(int id)
        {
            var aspirante = await _dbContext.Aspirante
                .FirstOrDefaultAsync(a => a.IdPersona == id);

            if (aspirante == null)
            {
                throw new Exception("No se ha encontrado aspirante con el id ingresado.");
            }

            return aspirante;
        }

        public async Task<Aspirante> GetAspiranteById(int id)
        {
            var aspirante = await _dbContext.Aspirante
                .Include(a => a.IdPlanNavigation)
                .Include(a => a.IdAspiranteEstatusNavigation)
                .Include(a => a.IdPersonaNavigation)
                .ThenInclude(a => a.IdDireccionNavigation)
                .ThenInclude(d => d.CodigoPostal)
                .ThenInclude(cp => cp.Municipio)
                .ThenInclude(m => m.Estado)
                .FirstOrDefaultAsync(a => a.IdAspirante == id);

            if (aspirante == null)
            {
                throw new Exception("No se ha encontrado aspirante con el id ingresado.");
            }

            return aspirante;
        }

        public async Task<Aspirante> CrearAspirante(Aspirante aspirante)
        {
            var curpValida = (await _dbContext.Persona
                .SingleOrDefaultAsync(p => p.Curp == aspirante.IdPersonaNavigation!.Curp)) == null;

            var correoValido = (await _dbContext.Persona
                .SingleOrDefaultAsync(p => p.Correo == aspirante.IdPersonaNavigation!.Correo)) == null;


            if (!curpValida)
                throw new Exception("Ya existe un aspirante con la curp ingresada.");

            if (!correoValido)
                throw new Exception("Ya existe un aspirante con el correo ingresado.");


            await using var tx = await _dbContext.Database.BeginTransactionAsync();
            try
            {

                await _dbContext.Aspirante.AddAsync(aspirante);
                await _dbContext.SaveChangesAsync();

                await InicializarDocumentosAspiranteAsync(aspirante.IdAspirante);

                await GenerarReciboInscripcionAspiranteAsync(aspirante.IdAspirante);

                await tx.CommitAsync();
                return aspirante;
            }
            catch (DbUpdateException ex)
            {
                var inner = ex.InnerException?.Message ?? ex.Message;
                throw new InvalidOperationException($"[DB] Error al guardar: {inner}", ex);
            }
        }


        public async Task<Aspirante> ActualizarAspirante(Aspirante newAspirante)
        {
            var aspirante = await _dbContext.Aspirante
                .Include(a => a.IdPersonaNavigation)
                .ThenInclude(p => p.IdDireccionNavigation)
                .SingleOrDefaultAsync(a => a.IdAspirante == newAspirante.IdAspirante);

            if (aspirante == null)
            {
                throw new Exception("No existe aspirante con el id ingresado");
            }

            if (newAspirante.IdPersonaNavigation != null)
            {
                var persona = await _dbContext.Persona.SingleOrDefaultAsync(p => p.IdPersona == aspirante.IdPersona);

                persona.Nombre = newAspirante.IdPersonaNavigation.Nombre;
                persona.ApellidoPaterno = newAspirante.IdPersonaNavigation.ApellidoPaterno;
                persona.ApellidoMaterno = newAspirante.IdPersonaNavigation.ApellidoMaterno;
                persona.FechaNacimiento = newAspirante.IdPersonaNavigation.FechaNacimiento;
                persona.IdGenero = newAspirante.IdPersonaNavigation.IdGenero;
                persona.Curp = newAspirante.IdPersonaNavigation.Curp;
                persona.Correo = newAspirante.IdPersonaNavigation.Correo;
                persona.Telefono = newAspirante.IdPersonaNavigation.Telefono;

                _dbContext.Persona.Update(persona);
            }

            if (newAspirante.IdPersonaNavigation != null && newAspirante.IdPersonaNavigation.IdDireccionNavigation != null)
            {
                var direccion = await _dbContext.Direccion.SingleOrDefaultAsync(d => d.IdDireccion == aspirante.IdPersonaNavigation.IdDireccion);

                direccion.Calle = newAspirante.IdPersonaNavigation.IdDireccionNavigation.Calle;
                direccion.NumeroExterior = newAspirante.IdPersonaNavigation.IdDireccionNavigation.NumeroExterior;
                direccion.NumeroInterior = newAspirante.IdPersonaNavigation.IdDireccionNavigation.NumeroInterior;
                direccion.CodigoPostalId = newAspirante.IdPersonaNavigation.IdDireccionNavigation.CodigoPostalId;

                _dbContext.Direccion.Update(direccion);
            }

            aspirante.IdPlan = newAspirante.IdPlan;
            aspirante.IdMedioContacto = newAspirante.IdMedioContacto;
            aspirante.FechaRegistro = newAspirante.FechaRegistro;
            aspirante.Observaciones = newAspirante.Observaciones;
            aspirante.TurnoId = newAspirante.TurnoId;
            aspirante.IdAspiranteEstatus = newAspirante.IdAspiranteEstatus;
            aspirante.IdAtendidoPorUsuario = newAspirante.IdAtendidoPorUsuario;
            aspirante.CuatrimestreInteres = newAspirante.CuatrimestreInteres;

            if (!string.IsNullOrEmpty(aspirante.IdAtendidoPorUsuario) && !int.TryParse(aspirante.IdAtendidoPorUsuario, out _))
            {
                throw new Exception("El campo IdAtendidoPorUsuario debe ser un numero entero.");
            }

            _dbContext.Aspirante.Update(aspirante);

            await _dbContext.SaveChangesAsync();

            return aspirante;
        }

        public async Task<IEnumerable<AspiranteBitacoraSeguimiento>> GetBitacoraSeguimiento(int aspiranteId)
        {
            return await _dbContext.AspiranteBitacoraSeguimiento
                .Include(a => a.UsuarioAtiende)
                .Where(a => a.AspiranteId == aspiranteId)
                .OrderBy(a => a.Fecha)
                .ToListAsync();
        }

        public async Task<AspiranteBitacoraSeguimiento> CrearSeguimiento(AspiranteBitacoraSeguimiento seguimiento)
        {
            await _dbContext.AspiranteBitacoraSeguimiento.AddAsync(seguimiento);
            await _dbContext.SaveChangesAsync();

            return seguimiento;
        }

        private async Task InicializarDocumentosAspiranteAsync(int idAspirante)
        {
            var reqs = await _dbContext.DocumentoRequisito
        .Where(r => r.Activo)
        .Select(r => r.IdDocumentoRequisito)
        .ToListAsync();

            if (reqs.Count == 0) return;

            var existentes = await _dbContext.AspiranteDocumento
                .Where(d => d.IdAspirante == idAspirante)
                .Select(d => d.IdDocumentoRequisito)
                .ToListAsync();

            var pendientes = reqs.Except(existentes).ToList();
            if (pendientes.Count == 0) return;

            var nuevos = pendientes.Select(idReq => new AspiranteDocumento
            {
                IdAspirante = idAspirante,
                IdDocumentoRequisito = idReq,
                Estatus = EstatusDocumentoEnum.PENDIENTE,
                FechaSubidoUtc = null,
                UrlArchivo = null,
                Notas = null
            });

            await _dbContext.AspiranteDocumento.AddRangeAsync(nuevos);
            await _dbContext.SaveChangesAsync();
        }

        private async Task GenerarReciboInscripcionAspiranteAsync(int idAspirante)
        {
            var hoy = DateOnly.FromDateTime(DateTime.UtcNow);

            var aplicaA_Aspirante = (int)ConceptoAplicaAEnum.Aspirante;
            var aplicaA_Ambos = (int)ConceptoAplicaAEnum.Ambos;

            var vence = hoy.AddDays(7);

            var concepto = await _dbContext.ConceptoPago
                .Where(c => c.Activo
                    && (c.Clave == "INSCRIPCION" || c.Clave == "FICHA")
                    && (c.AplicaA == ConceptoAplicaAEnum.Aspirante || c.AplicaA == ConceptoAplicaAEnum.Ambos))
                .OrderBy(c => c.Clave == "INSCRIPCION" ? 0 : 1)
                .FirstOrDefaultAsync();

            if (concepto == null)
                throw new InvalidOperationException("No existe ConceptoPago activo para Aspirante (INSCRIPCION/FICHA).");

            var precio = await _dbContext.ConceptoPrecio
                .Where(p => p.Activo
                    && p.IdConceptoPago == concepto.IdConceptoPago
                    && p.VigenciaDesde <= hoy
                    && (p.VigenciaHasta == null || p.VigenciaHasta >= hoy))
                .OrderByDescending(p => p.VigenciaDesde)
                .FirstOrDefaultAsync();

            if (precio == null)
                throw new InvalidOperationException($"No existe precio vigente para el concepto {concepto.Clave}.");

            var recibo = new Recibo
            {
                Folio = await GenerarFolioAsync(),
                IdAspirante = idAspirante,
                FechaEmision = hoy,
                FechaVencimiento = vence,
                Estatus = EstatusRecibo.PENDIENTE,
                Subtotal = precio.Importe,
                Descuento = 0m,
                Saldo = precio.Importe
            };

            await _dbContext.Recibo.AddAsync(recibo);
            await _dbContext.SaveChangesAsync();

            var detalle = new ReciboDetalle
            {
                IdRecibo = recibo.IdRecibo,
                IdConceptoPago = concepto.IdConceptoPago,
                Descripcion = concepto.Clave,
                Cantidad = 1,
                PrecioUnitario = precio.Importe,
                RefTabla = "Aspirante",
                RefId = idAspirante
            };

            await _dbContext.ReciboDetalle.AddAsync(detalle);
            await _dbContext.SaveChangesAsync();
        }

        private async Task<string> GenerarFolioAsync(CancellationToken ct = default)
        {
            var anio = DateTime.UtcNow.Year;
            var prefijo = $"REC-{anio}-";

            var ultimoFolio = await _dbContext.Recibo
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

        private static DateOnly CalcularVencimientoDia5(DateOnly hoy)
        {
            if (hoy.Day <= 5) return new DateOnly(hoy.Year, hoy.Month, 5);

            var nextMonth = hoy.Month == 12 ? 1 : hoy.Month + 1;
            var nextYear = hoy.Month == 12 ? hoy.Year + 1 : hoy.Year;
            return new DateOnly(nextYear, nextMonth, 5);
        }

        public async Task<bool> CancelarAspiranteAsync(int idAspirante, string motivo)
        {
            var aspirante = await _dbContext.Aspirante
                .Include(a => a.IdAspiranteEstatusNavigation)
                .FirstOrDefaultAsync(a => a.IdAspirante == idAspirante);

            if (aspirante == null)
                return false;

            var estatusRechazado = await _dbContext.AspiranteEstatus
                .FirstOrDefaultAsync(e => e.DescEstatus == "Rechazado");

            if (estatusRechazado == null)
                throw new InvalidOperationException("No se encontro el estatus 'Rechazado' en el catalogo");

            var estatusAnterior = aspirante.IdAspiranteEstatusNavigation.DescEstatus;

            aspirante.IdAspiranteEstatus = estatusRechazado.IdAspiranteEstatus;
            _dbContext.Aspirante.Update(aspirante);

            var seguimiento = new AspiranteBitacoraSeguimiento
            {
                AspiranteId = idAspirante,
                UsuarioAtiendeId = aspirante.UpdatedBy ?? "SYSTEM",
                Fecha = DateTime.UtcNow,
                MedioContacto = "Sistema",
                Resumen = $"Aspirante cancelado. Estatus anterior: {estatusAnterior}",
                ProximaAccion = $"Motivo: {motivo}"
            };

            await _dbContext.AspiranteBitacoraSeguimiento.AddAsync(seguimiento);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<EstadisticasAspirantesDto> ObtenerEstadisticasAsync(int? periodoId)
        {
            var query = _dbContext.Aspirante
                .Include(a => a.IdAspiranteEstatusNavigation)
                .Include(a => a.IdPlanNavigation)
                .Include(a => a.IdMedioContactoNavigation)
                .AsQueryable();


            var aspirantes = await query.ToListAsync();

            var stats = new EstadisticasAspirantesDto
            {
                TotalAspirantes = aspirantes.Count,
                AspirantesPorEstatus = aspirantes
                    .GroupBy(a => a.IdAspiranteEstatusNavigation.DescEstatus)
                    .ToDictionary(g => g.Key, g => g.Count()),
                AspirantesPorPrograma = aspirantes
                    .GroupBy(a => a.IdPlanNavigation.NombrePlanEstudios ?? "Sin programa")
                    .ToDictionary(g => g.Key, g => g.Count()),
                AspirantesPorMedioContacto = aspirantes
                    .GroupBy(a => a.IdMedioContactoNavigation.DescMedio)
                    .ToDictionary(g => g.Key, g => g.Count())
            };

            var aspirantesConDocsPendientes = await _dbContext.AspiranteDocumento
                .Where(d => d.Estatus == EstatusDocumentoEnum.PENDIENTE)
                .Select(d => d.IdAspirante)
                .Distinct()
                .CountAsync();

            stats.AspirantesConDocumentosPendientes = aspirantesConDocsPendientes;

            var aspirantesIds = aspirantes.Select(a => a.IdAspirante).ToList();
            var aspirantesConDocsCompletos = 0;

            foreach (var id in aspirantesIds)
            {
                var totalDocs = await _dbContext.AspiranteDocumento.CountAsync(d => d.IdAspirante == id);
                var docsValidados = await _dbContext.AspiranteDocumento
                    .CountAsync(d => d.IdAspirante == id && d.Estatus == EstatusDocumentoEnum.VALIDADO);

                if (totalDocs > 0 && totalDocs == docsValidados)
                    aspirantesConDocsCompletos++;
            }

            stats.AspirantesConDocumentosCompletos = aspirantesConDocsCompletos;

            var aspirantesConPagosPendientes = await _dbContext.Recibo
                .Where(r => r.IdAspirante != null && r.Estatus == EstatusRecibo.PENDIENTE)
                .Select(r => r.IdAspirante!.Value)
                .Distinct()
                .CountAsync();

            stats.AspirantesConPagosPendientes = aspirantesConPagosPendientes;

            var aspirantesConPagosCompletos = await _dbContext.Recibo
                .Where(r => r.IdAspirante != null && r.Estatus == EstatusRecibo.PAGADO)
                .Select(r => r.IdAspirante!.Value)
                .Distinct()
                .CountAsync();

            stats.AspirantesConPagosCompletos = aspirantesConPagosCompletos;

            return stats;
        }

        public async Task<FichaAdmisionDto?> ObtenerFichaCompleta(int aspiranteId, string? usuarioGeneraId = null)
        {
            var aspirante = await _dbContext.Aspirante
                .Include(a => a.IdAspiranteEstatusNavigation)
                .Include(a => a.IdMedioContactoNavigation)
                .Include(a => a.Turno)
                .Include(a => a.IdPlanNavigation)
                    .ThenInclude(p => p.IdNivelEducativoNavigation)
                .Include(a => a.IdPlanNavigation)
                    .ThenInclude(p => p.IdCampusNavigation)
                .Include(a => a.IdPlanNavigation)
                    .ThenInclude(p => p.IdPeriodicidadNavigation)
                .Include(a => a.IdPersonaNavigation)
                    .ThenInclude(p => p.IdGeneroNavigation)
                .Include(a => a.IdPersonaNavigation)
                    .ThenInclude(p => p.IdEstadoCivilNavigation)
                .Include(a => a.IdPersonaNavigation)
                    .ThenInclude(p => p.IdDireccionNavigation)
                        .ThenInclude(d => d.CodigoPostal)
                            .ThenInclude(cp => cp.Municipio)
                                .ThenInclude(m => m.Estado)
                .Include(a => a.Documentos)
                    .ThenInclude(d => d.Requisito)
                .FirstOrDefaultAsync(a => a.IdAspirante == aspiranteId);

            if (aspirante == null)
                return null;

            var persona = aspirante.IdPersonaNavigation;
            var direccion = persona?.IdDireccionNavigation;
            var codigoPostal = direccion?.CodigoPostal;
            var municipio = codigoPostal?.Municipio;
            var estado = municipio?.Estado;
            var plan = aspirante.IdPlanNavigation;

            var recibos = await _dbContext.Recibo
                .Include(r => r.Detalles)
                    .ThenInclude(d => d.ConceptoPago)
                .Where(r => r.IdAspirante == aspiranteId)
                .OrderBy(r => r.FechaEmision)
                .ToListAsync();

            var bitacora = await _dbContext.AspiranteBitacoraSeguimiento
                .Include(b => b.UsuarioAtiende)
                .Where(b => b.AspiranteId == aspiranteId)
                .OrderBy(b => b.Fecha)
                .ToListAsync();

            ApplicationUser? asesor = null;
            if (!string.IsNullOrEmpty(aspirante.IdAtendidoPorUsuario))
            {
                asesor = await _dbContext.Users.FindAsync(aspirante.IdAtendidoPorUsuario);
            }

            ApplicationUser? usuarioGenera = null;
            if (!string.IsNullOrEmpty(usuarioGeneraId))
            {
                usuarioGenera = await _dbContext.Users.FindAsync(usuarioGeneraId);
            }


            int? edad = null;
            if (persona?.FechaNacimiento != null)
            {
                var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
                edad = hoy.Year - persona.FechaNacimiento.Value.Year;
                if (hoy < persona.FechaNacimiento.Value.AddYears(edad.Value))
                    edad--;
            }

            var ficha = new FichaAdmisionDto
            {
                IdAspirante = aspirante.IdAspirante,
                Folio = $"ASP-{aspirante.IdAspirante:D6}",
                FechaRegistro = aspirante.FechaRegistro,
                EstatusActual = aspirante.IdAspiranteEstatusNavigation?.DescEstatus ?? "N/A",
                Observaciones = aspirante.Observaciones,

                DatosPersonales = new DatosPersonalesDto
                {
                    NombreCompleto = $"{persona?.Nombre} {persona?.ApellidoPaterno} {persona?.ApellidoMaterno}".Trim(),
                    Nombre = persona?.Nombre,
                    ApellidoPaterno = persona?.ApellidoPaterno,
                    ApellidoMaterno = persona?.ApellidoMaterno,
                    CURP = persona?.Curp,
                    RFC = persona?.Rfc,
                    FechaNacimiento = persona?.FechaNacimiento,
                    Edad = edad,
                    Genero = persona?.IdGeneroNavigation?.DescGenero,
                    EstadoCivil = persona?.IdEstadoCivilNavigation?.DescEstadoCivil,
                    FotoUrl = null
                },

                DatosContacto = new DatosContactoDto
                {
                    Email = persona?.Correo,
                    Telefono = persona?.Telefono,
                    Celular = persona?.Celular,
                    Direccion = direccion != null ? new DireccionDto
                    {
                        Calle = direccion.Calle,
                        NumeroExterior = direccion.NumeroExterior,
                        NumeroInterior = direccion.NumeroInterior,
                        Colonia = codigoPostal?.Asentamiento,
                        CodigoPostal = codigoPostal?.Codigo,
                        Municipio = municipio?.Nombre,
                        Estado = estado?.Nombre,
                        DireccionCompleta = $"{direccion.Calle} {direccion.NumeroExterior}{(string.IsNullOrEmpty(direccion.NumeroInterior) ? "" : " Int. " + direccion.NumeroInterior)}, {codigoPostal?.Asentamiento}, CP {codigoPostal?.Codigo}, {municipio?.Nombre}, {estado?.Nombre}".Trim()
                    } : null
                },

                InformacionAcademica = new InformacionAcademicaDto
                {
                    ClavePlan = plan?.ClavePlanEstudios,
                    NombrePlan = plan?.NombrePlanEstudios,
                    NivelEducativo = plan?.IdNivelEducativoNavigation?.DescNivelEducativo,
                    RVOE = plan?.RVOE,
                    DuracionMeses = plan?.DuracionMeses,
                    Turno = aspirante.Turno?.Nombre,
                    Campus = plan?.IdCampusNavigation?.Nombre,
                    Periodicidad = plan?.IdPeriodicidadNavigation?.DescPeriodicidad
                },

                Documentos = aspirante.Documentos.Select(d => new DocumentoDto
                {
                    Clave = d.Requisito?.Clave ?? "N/A",
                    Descripcion = d.Requisito?.Descripcion ?? "N/A",
                    EsObligatorio = d.Requisito?.EsObligatorio ?? false,
                    Estatus = d.Estatus.ToString(),
                    FechaSubida = d.FechaSubidoUtc,
                    UrlArchivo = d.UrlArchivo,
                    Notas = d.Notas
                }).OrderBy(d => d.Descripcion).ToList(),

                InformacionPagos = new InformacionPagosDto
                {
                    TotalAPagar = recibos.Sum(r => r.Total),
                    TotalPagado = recibos.Where(r => r.Estatus == EstatusRecibo.PAGADO).Sum(r => r.Total),
                    SaldoPendiente = recibos.Sum(r => r.Saldo),
                    Recibos = recibos.Select(r => new ReciboResumenDto
                    {
                        IdRecibo = r.IdRecibo,
                        Folio = r.Folio,
                        FechaEmision = r.FechaEmision,
                        FechaVencimiento = r.FechaVencimiento,
                        Estatus = r.Estatus.ToString(),
                        Subtotal = r.Subtotal,
                        Descuento = r.Descuento,
                        Recargos = r.Recargos,
                        Total = r.Total,
                        Saldo = r.Saldo,
                        Conceptos = r.Detalles.Select(d => new ConceptoReciboDto
                        {
                            Concepto = d.ConceptoPago?.Descripcion ?? d.Descripcion ?? "N/A",
                            Cantidad = (int)d.Cantidad,
                            PrecioUnitario = d.PrecioUnitario,
                            Subtotal = d.Importe
                        }).ToList()
                    }).ToList()
                },

                Seguimiento = new SeguimientoDto
                {
                    AsesorAsignado = asesor != null ? new AsesorDto
                    {
                        Id = asesor.Id,
                        NombreCompleto = $"{asesor.Nombres} {asesor.Apellidos}".Trim(),
                        Email = asesor.Email,
                        Telefono = asesor.PhoneNumber
                    } : null,
                    MedioContacto = aspirante.IdMedioContactoNavigation?.DescMedio,
                    Bitacora = bitacora.Select(b => new BitacoraSeguimientoDto
                    {
                        Fecha = b.Fecha,
                        UsuarioAtiende = b.UsuarioAtiende != null ? $"{b.UsuarioAtiende.Nombres} {b.UsuarioAtiende.Apellidos}".Trim() : "N/A",
                        MedioContacto = b.MedioContacto,
                        Resumen = b.Resumen,
                        ProximaAccion = b.ProximaAccion
                    }).ToList()
                },

                Metadata = new MetadataGeneracionDto
                {
                    FechaGeneracion = DateTime.UtcNow,
                    UsuarioGenero = usuarioGeneraId,
                    NombreUsuarioGenero = usuarioGenera != null ? $"{usuarioGenera.Nombres} {usuarioGenera.Apellidos}".Trim() : null
                }
            };

            return ficha;
        }

        public async Task<InscripcionAspiranteResultDto> InscribirAspiranteComoEstudianteAsync(
            int aspiranteId,
            Core.Requests.Aspirante.InscribirAspiranteRequest request,
            string? usuarioProcesa = null)
        {
            if (_matriculaService == null || _estudianteService == null || _authService == null)
            {
                throw new InvalidOperationException(
                    "El servicio de inscripcion requiere IMatriculaService, IEstudianteService y IAuthService. " +
                    "Asegurate de usar el constructor con todas las dependencias.");
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var aspirante = await _dbContext.Aspirante
                    .Include(a => a.IdAspiranteEstatusNavigation)
                    .Include(a => a.IdPersonaNavigation)
                    .Include(a => a.IdPlanNavigation)
                    .Include(a => a.Documentos)
                        .ThenInclude(d => d.Requisito)
                    .FirstOrDefaultAsync(a => a.IdAspirante == aspiranteId);

                if (aspirante == null)
                    throw new InvalidOperationException($"No se encontro el aspirante con ID {aspiranteId}");

                if (aspirante.IdPersonaNavigation == null)
                    throw new InvalidOperationException("El aspirante no tiene informacion de persona asociada");

                if (aspirante.IdPlanNavigation == null)
                    throw new InvalidOperationException("El aspirante no tiene un plan de estudios asignado");

                var persona = aspirante.IdPersonaNavigation;
                var plan = aspirante.IdPlanNavigation;

                var validaciones = new ValidacionesInscripcionDto();
                var advertencias = new List<string>();

                var estatusValido = aspirante.IdAspiranteEstatusNavigation?.DescEstatus == "En Proceso";
                validaciones.EstatusAspiranteValido = estatusValido;
                if (!estatusValido && !request.ForzarInscripcion)
                {
                    throw new InvalidOperationException(
                        $"El aspirante debe estar en estatus 'En Proceso' para ser inscrito. " +
                        $"Estatus actual: {aspirante.IdAspiranteEstatusNavigation?.DescEstatus}");
                }
                if (!estatusValido)
                    advertencias.Add($"Estatus del aspirante: {aspirante.IdAspiranteEstatusNavigation?.DescEstatus} (se forzo la inscripcion)");

                var documentosObligatorios = aspirante.Documentos
                    .Where(d => d.Requisito != null && d.Requisito.EsObligatorio)
                    .ToList();

                var documentosValidados = documentosObligatorios
                    .Where(d => d.Estatus == EstatusDocumentoEnum.VALIDADO)
                    .Count();

                validaciones.DocumentosCompletos = documentosValidados == documentosObligatorios.Count;
                validaciones.DetalleDocumentos = aspirante.Documentos
                    .Where(d => d.Requisito != null && d.Requisito.EsObligatorio)
                    .Select(d => new DocumentoValidacionDto
                    {
                        Descripcion = d.Requisito?.Descripcion ?? "N/A",
                        EsObligatorio = true,
                        Estatus = d.Estatus.ToString(),
                        Cumple = d.Estatus == EstatusDocumentoEnum.VALIDADO
                    }).ToList();

                if (!validaciones.DocumentosCompletos && !request.ForzarInscripcion)
                {
                    throw new InvalidOperationException(
                        $"El aspirante debe tener todos los documentos obligatorios validados. " +
                        $"Documentos validados: {documentosValidados}/{documentosObligatorios.Count}");
                }
                if (!validaciones.DocumentosCompletos)
                    advertencias.Add($"Documentos incompletos: {documentosValidados}/{documentosObligatorios.Count} (se forzo la inscripcion)");

                var recibos = await _dbContext.Recibo
                    .Where(r => r.IdAspirante == aspiranteId)
                    .ToListAsync();

                var todosPagados = recibos.All(r => r.Estatus == EstatusRecibo.PAGADO);
                validaciones.PagoInscripcionRealizado = todosPagados || !recibos.Any();

                if (!validaciones.PagoInscripcionRealizado && !request.ForzarInscripcion)
                {
                    var recibosPendientes = recibos.Where(r => r.Estatus != EstatusRecibo.PAGADO).Count();
                    throw new InvalidOperationException(
                        $"El aspirante tiene {recibosPendientes} recibo(s) pendiente(s) de pago");
                }
                if (!validaciones.PagoInscripcionRealizado)
                    advertencias.Add("Existen recibos pendientes de pago (se forzo la inscripcion)");

                validaciones.Advertencias = advertencias;

                var matricula = await _matriculaService.GenerarMatriculaAsync(plan.NombrePlanEstudios ?? "");

                var estudiante = new Estudiante
                {
                    Matricula = matricula,
                    IdPersona = aspirante.IdPersona!.Value,
                    IdPlanActual = aspirante.IdPlan,
                    FechaIngreso = DateOnly.FromDateTime(DateTime.UtcNow),
                    Activo = true,
                    Email = null,
                    UsuarioId = null
                };

                var estudianteCreado = await _estudianteService.CrearEstudiante(estudiante);

                var dominioEmail = "@usag.edu.mx";
                var emailUsuario = $"{matricula}{dominioEmail}";
                var passwordTemporal = matricula;

                var nuevoUsuario = new ApplicationUser
                {
                    UserName = emailUsuario,
                    Email = emailUsuario,
                    Nombres = persona.Nombre ?? "",
                    Apellidos = $"{persona.ApellidoPaterno} {persona.ApellidoMaterno}".Trim(),
                    PhoneNumber = persona.Celular ?? persona.Telefono
                };

                Console.WriteLine($"=== CREANDO USUARIO ===");
                Console.WriteLine($"Email: {emailUsuario}, Matricula: {matricula}");

                try
                {
                    await _authService.Signup(nuevoUsuario, passwordTemporal, [Configuration.Constants.Rol.ALUMNO]);
                    Console.WriteLine("Usuario creado exitosamente");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al crear usuario: {ex.Message}");
                    Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                    throw new InvalidOperationException($"Error al crear usuario del sistema: {ex.Message}", ex);
                }

                var usuarioCreado = await _authService.GetUserByEmail(emailUsuario);
                if (usuarioCreado == null)
                    throw new InvalidOperationException("No se pudo recuperar el usuario recien creado");

                Console.WriteLine($"Usuario recuperado: {usuarioCreado.Id}");

                estudianteCreado.UsuarioId = usuarioCreado.Id;
                estudianteCreado.Email = emailUsuario;

                Console.WriteLine($"=== ACTUALIZANDO ESTUDIANTE ===");
                try
                {
                    await _estudianteService.ActualizarEstudiante(estudianteCreado);
                    Console.WriteLine("Estudiante actualizado exitosamente");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al actualizar estudiante: {ex.Message}");
                    Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                    throw new InvalidOperationException($"Error al actualizar estudiante: {ex.Message}", ex);
                }

                Console.WriteLine("=== ACTUALIZANDO ESTATUS DEL ASPIRANTE ===");

                var estatusInscrito = await _dbContext.AspiranteEstatus
                    .Where(e => e.Status == Core.Enums.StatusEnum.Active)
                    .FirstOrDefaultAsync(e => e.DescEstatus == "Inscrito" || e.DescEstatus == "Admitido");

                if (estatusInscrito == null)
                {
                    Console.WriteLine("No se encontro estatus 'Inscrito' o 'Admitido', buscando alternativas...");
                    estatusInscrito = await _dbContext.AspiranteEstatus
                        .Where(e => e.Status == Core.Enums.StatusEnum.Active)
                        .FirstOrDefaultAsync(e => e.DescEstatus.Contains("Inscrit") || e.DescEstatus.Contains("Admit"));
                }

                if (estatusInscrito != null)
                {
                    Console.WriteLine($"Cambiando estatus de '{aspirante.IdAspiranteEstatusNavigation?.DescEstatus}' -> '{estatusInscrito.DescEstatus}'");
                    aspirante.IdAspiranteEstatus = estatusInscrito.IdAspiranteEstatus;
                    _dbContext.Aspirante.Update(aspirante);
                    await _dbContext.SaveChangesAsync();
                    Console.WriteLine("Estatus actualizado exitosamente");
                }
                else
                {
                    Console.WriteLine("ADVERTENCIA: No se encontro ningun estatus valido para 'Inscrito'");
                    Console.WriteLine("   El aspirante mantendra su estatus actual.");

                    var estatusDisponibles = await _dbContext.AspiranteEstatus
                        .Where(e => e.Status == Core.Enums.StatusEnum.Active)
                        .Select(e => e.DescEstatus)
                        .ToListAsync();
                    Console.WriteLine($"   Estatus disponibles: {string.Join(", ", estatusDisponibles)}");
                }

                if (!string.IsNullOrWhiteSpace(usuarioProcesa))
                {
                    var seguimiento = new AspiranteBitacoraSeguimiento
                    {
                        AspiranteId = aspiranteId,
                        UsuarioAtiendeId = usuarioProcesa,
                        Fecha = DateTime.UtcNow,
                        MedioContacto = "Sistema",
                        Resumen = $"Aspirante inscrito como estudiante. Matricula generada: {matricula}",
                        ProximaAccion = request.ForzarInscripcion
                            ? $"Inscripcion forzada. Observaciones: {request.Observaciones ?? "Ninguna"}"
                            : "Proceso de inscripcion completado exitosamente"
                    };

                    await _dbContext.AspiranteBitacoraSeguimiento.AddAsync(seguimiento);
                    await _dbContext.SaveChangesAsync();
                    Console.WriteLine("Bitacora registrada exitosamente");
                }
                else
                {
                    Console.WriteLine("No se registro bitacora: usuario no identificado");
                }

                var recibosGenerados = new List<ReciboGeneradoDto>();

                await transaction.CommitAsync();

                var resultado = new InscripcionAspiranteResultDto
                {
                    IdAspirante = aspiranteId,
                    NombreCompleto = $"{persona.Nombre} {persona.ApellidoPaterno} {persona.ApellidoMaterno}".Trim(),
                    NuevoEstatusAspirante = estatusInscrito?.DescEstatus ?? "Inscrito",
                    IdEstudiante = estudianteCreado.IdEstudiante,
                    Matricula = matricula,
                    Email = emailUsuario,
                    FechaIngreso = estudianteCreado.FechaIngreso,
                    PlanEstudios = plan.NombrePlanEstudios ?? "N/A",
                    Credenciales = new CredencialesAccesoDto
                    {
                        Usuario = emailUsuario,
                        PasswordTemporal = passwordTemporal,
                        UrlAcceso = "https://portal.usag.edu.mx",
                        Mensaje = "Credenciales generadas. El estudiante debe cambiar su contrasena en el primer acceso."
                    },
                    RecibosGenerados = recibosGenerados,
                    Validaciones = validaciones,
                    FechaProceso = DateTime.UtcNow,
                    UsuarioQueProceso = usuarioProcesa,
                    InscripcionForzada = request.ForzarInscripcion
                };

                return resultado;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<AspiranteEstatus?> ObtenerEstatusEnProcesoAsync()
        {
            return await _dbContext.AspiranteEstatus
                .FirstOrDefaultAsync(e => e.DescEstatus == "En Proceso" && e.Status == Core.Enums.StatusEnum.Active);
        }

        public async Task<PlantillaCobroDto?> BuscarPlantillaParaAspiranteAsync(int idAspirante, CancellationToken ct)
        {
            if (_plantillaCobroService == null)
                throw new InvalidOperationException("IPlantillaCobroService no está disponible.");

            var aspirante = await _dbContext.Aspirante
                .Include(a => a.IdPlanNavigation)
                .FirstOrDefaultAsync(a => a.IdAspirante == idAspirante, ct);

            if (aspirante == null)
                throw new InvalidOperationException($"No se encontró el aspirante con ID {idAspirante}");

            var cuatrimestre = aspirante.CuatrimestreInteres ?? 1;

            return await _plantillaCobroService.BuscarPlantillaActivaAsync(
                aspirante.IdPlan,
                cuatrimestre,
                idTurno: aspirante.TurnoId,
                ct: ct);
        }

        public async Task<IReadOnlyList<ReciboDto>> GenerarRecibosDesdeePlantillaParaAspiranteAsync(
            int idAspirante, int idPlantillaCobro, bool eliminarPendientes, CancellationToken ct)
        {
            if (_plantillaCobroService == null || _reciboService == null)
                throw new InvalidOperationException("IPlantillaCobroService e IReciboService son requeridos.");

            var plantilla = await _plantillaCobroService.ObtenerPlantillaPorIdAsync(idPlantillaCobro, ct);
            if (plantilla == null)
                throw new InvalidOperationException($"No se encontró la plantilla de cobro con ID {idPlantillaCobro}");

            if (eliminarPendientes)
            {
                var recibosPendientes = await _dbContext.Recibo
                    .Where(r => r.IdAspirante == idAspirante && r.Estatus == EstatusRecibo.PENDIENTE)
                    .ToListAsync(ct);

                foreach (var recibo in recibosPendientes)
                {
                    await _reciboService.EliminarReciboAsync(recibo.IdRecibo, ct);
                }
            }

            var recibosGenerados = new List<ReciboDto>();

            if (plantilla.Detalles != null)
            {
                foreach (var detalle in plantilla.Detalles.OrderBy(d => d.Orden))
                {
                    var monto = detalle.PrecioUnitario * detalle.Cantidad;
                    var descripcion = detalle.Descripcion ?? detalle.NombreConcepto ?? "Concepto";

                    var recibo = await _reciboService.GenerarReciboAspiranteAsync(
                        idAspirante,
                        monto,
                        descripcion,
                        plantilla.DiaVencimiento,
                        ct);

                    recibosGenerados.Add(recibo);
                }
            }

            return recibosGenerados;
        }

    }
}
