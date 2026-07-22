using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Core.DTOs.EstudioSocioeconomico;
using WebApplication2.Core.Models;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;
using StatusEnum = WebApplication2.Core.Enums.StatusEnum;

namespace WebApplication2.Services
{
    public class EstudioSocioeconomicoService : IEstudioSocioeconomicoService
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        private static readonly string[] RolesAnalista =
            { "superadmin", "admin", "controlescolar", "academico", "coordinador", "director" };

        public EstudioSocioeconomicoService(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<CatalogosEstudioDto> GetCatalogosAsync()
        {
            return new CatalogosEstudioDto
            {
                Parentescos = await _db.CatParentescoVivienda.Where(c => c.Status == StatusEnum.Active)
                    .OrderBy(c => c.Orden).Select(c => new CatalogoItemDto { Id = c.IdParentescoVivienda, Nombre = c.Nombre }).ToListAsync(),
                ServiciosVivienda = await _db.CatServicioVivienda.Where(c => c.Status == StatusEnum.Active)
                    .OrderBy(c => c.Orden).Select(c => new CatalogoItemDto { Id = c.IdServicioVivienda, Nombre = c.Nombre }).ToListAsync(),
                ServiciosMedicos = await _db.CatServicioMedico.Where(c => c.Status == StatusEnum.Active)
                    .OrderBy(c => c.Orden).Select(c => new CatalogoItemDto { Id = c.IdServicioMedico, Nombre = c.Nombre }).ToListAsync(),
                RecursosTecnologicos = await _db.CatRecursoTecnologico.Where(c => c.Status == StatusEnum.Active)
                    .OrderBy(c => c.Orden).Select(c => new CatalogoItemDto { Id = c.IdRecursoTecnologico, Nombre = c.Nombre }).ToListAsync()
            };
        }

        public async Task<List<AnalistaDto>> GetAnalistasAsync()
        {
            var resultado = new Dictionary<string, AnalistaDto>();
            foreach (var rol in RolesAnalista)
            {
                var usuarios = await _userManager.GetUsersInRoleAsync(rol);
                foreach (var u in usuarios)
                {
                    if (!resultado.ContainsKey(u.Id))
                    {
                        var nombre = $"{u.Nombres} {u.Apellidos}".Trim();
                        resultado[u.Id] = new AnalistaDto { Id = u.Id, Nombre = string.IsNullOrWhiteSpace(nombre) ? (u.Email ?? u.Id) : nombre };
                    }
                }
            }
            return resultado.Values.OrderBy(a => a.Nombre).ToList();
        }

        private async Task<EstudioSocioeconomico?> CargarEstudioAsync(int idAspirante)
        {
            return await _db.EstudioSocioeconomico
                .Include(e => e.EstudioServicioVivienda)
                .Include(e => e.EstudioRecursoTecnologico)
                .FirstOrDefaultAsync(e => e.IdAspirante == idAspirante);
        }

        private async Task<EstudioSocioeconomicoDto> MapAsync(EstudioSocioeconomico e)
        {
            string? analistaNombre = null;
            if (!string.IsNullOrWhiteSpace(e.AnalistaId))
            {
                var u = await _userManager.FindByIdAsync(e.AnalistaId);
                if (u != null) analistaNombre = $"{u.Nombres} {u.Apellidos}".Trim();
            }

            return new EstudioSocioeconomicoDto
            {
                IdEstudioSocioeconomico = e.IdEstudioSocioeconomico,
                IdAspirante = e.IdAspirante,
                IdParentescoVivienda = e.IdParentescoVivienda,
                ConQuienViveOtro = e.ConQuienViveOtro,
                NumeroPersonasHogar = e.NumeroPersonasHogar,
                PrincipalSostenEconomico = e.PrincipalSostenEconomico,
                PersonasAportanIngresos = e.PersonasAportanIngresos,
                Trabaja = e.Trabaja,
                EmpresaActividad = e.EmpresaActividad,
                HorarioLaboral = e.HorarioLaboral,
                QuienCubreGastos = e.QuienCubreGastos,
                DificultadesEconomicas = e.DificultadesEconomicas,
                IdServicioMedico = e.IdServicioMedico,
                PadeceEnfermedad = e.PadeceEnfermedad,
                PadeceEnfermedadDetalle = e.PadeceEnfermedadDetalle,
                TieneDiscapacidad = e.TieneDiscapacidad,
                TieneDiscapacidadDetalle = e.TieneDiscapacidadDetalle,
                EscuelaProcedencia = e.EscuelaProcedencia,
                PromedioNivelAnterior = e.PromedioNivelAnterior,
                AnalistaId = e.AnalistaId,
                AnalistaNombre = analistaNombre,
                FechaLlenado = e.FechaLlenado,
                LlenadoPorAspirante = e.LlenadoPorAspirante,
                Token = e.Token,
                ServiciosViviendaIds = e.EstudioServicioVivienda.Select(s => s.IdServicioVivienda).ToList(),
                RecursosTecnologicosIds = e.EstudioRecursoTecnologico.Select(s => s.IdRecursoTecnologico).ToList()
            };
        }

        public async Task<EstudioSocioeconomicoDto?> GetByAspiranteAsync(int idAspirante)
        {
            var e = await CargarEstudioAsync(idAspirante);
            return e == null ? null : await MapAsync(e);
        }

        private void AplicarEscalares(EstudioSocioeconomico e, EstudioSocioeconomicoRequest req)
        {
            e.IdParentescoVivienda = req.IdParentescoVivienda;
            e.ConQuienViveOtro = req.ConQuienViveOtro;
            e.NumeroPersonasHogar = req.NumeroPersonasHogar;
            e.PrincipalSostenEconomico = req.PrincipalSostenEconomico;
            e.PersonasAportanIngresos = req.PersonasAportanIngresos;
            e.Trabaja = req.Trabaja;
            e.EmpresaActividad = req.EmpresaActividad;
            e.HorarioLaboral = req.HorarioLaboral;
            e.QuienCubreGastos = req.QuienCubreGastos;
            e.DificultadesEconomicas = req.DificultadesEconomicas;
            e.IdServicioMedico = req.IdServicioMedico;
            e.PadeceEnfermedad = req.PadeceEnfermedad;
            e.PadeceEnfermedadDetalle = req.PadeceEnfermedadDetalle;
            e.TieneDiscapacidad = req.TieneDiscapacidad;
            e.TieneDiscapacidadDetalle = req.TieneDiscapacidadDetalle;
            e.EscuelaProcedencia = req.EscuelaProcedencia;
            e.PromedioNivelAnterior = req.PromedioNivelAnterior;
        }

        private async Task SincronizarJuntasAsync(EstudioSocioeconomico e, EstudioSocioeconomicoRequest req)
        {
            var serviciosValidos = await _db.CatServicioVivienda.Where(c => c.Status == StatusEnum.Active)
                .Select(c => c.IdServicioVivienda).ToListAsync();
            var recursosValidos = await _db.CatRecursoTecnologico.Where(c => c.Status == StatusEnum.Active)
                .Select(c => c.IdRecursoTecnologico).ToListAsync();

            _db.EstudioServicioVivienda.RemoveRange(e.EstudioServicioVivienda);
            _db.EstudioRecursoTecnologico.RemoveRange(e.EstudioRecursoTecnologico);

            e.EstudioServicioVivienda = req.ServiciosViviendaIds.Distinct().Where(serviciosValidos.Contains)
                .Select(id => new EstudioServicioVivienda { IdServicioVivienda = id }).ToList();
            e.EstudioRecursoTecnologico = req.RecursosTecnologicosIds.Distinct().Where(recursosValidos.Contains)
                .Select(id => new EstudioRecursoTecnologico { IdRecursoTecnologico = id }).ToList();
        }

        public async Task<EstudioSocioeconomicoDto> UpsertAsync(int idAspirante, EstudioSocioeconomicoRequest req, string? analistaId, bool porAspirante)
        {
            var aspiranteExiste = await _db.Aspirante.AnyAsync(a => a.IdAspirante == idAspirante);
            if (!aspiranteExiste) throw new Exception("Aspirante no encontrado.");

            var e = await CargarEstudioAsync(idAspirante);
            if (e == null)
            {
                e = new EstudioSocioeconomico
                {
                    IdAspirante = idAspirante,
                    Token = Guid.NewGuid().ToString("N"),
                    Status = StatusEnum.Active
                };
                _db.EstudioSocioeconomico.Add(e);
            }

            AplicarEscalares(e, req);
            await SincronizarJuntasAsync(e, req);

            e.FechaLlenado = DateTime.UtcNow;
            e.LlenadoPorAspirante = porAspirante;
            if (!porAspirante)
                e.AnalistaId = analistaId ?? req.AnalistaId ?? e.AnalistaId;

            await _db.SaveChangesAsync();
            return await MapAsync(e);
        }

        public async Task<string> GenerarTokenAsync(int idAspirante)
        {
            var aspiranteExiste = await _db.Aspirante.AnyAsync(a => a.IdAspirante == idAspirante);
            if (!aspiranteExiste) throw new Exception("Aspirante no encontrado.");

            var e = await CargarEstudioAsync(idAspirante);
            if (e == null)
            {
                e = new EstudioSocioeconomico
                {
                    IdAspirante = idAspirante,
                    Token = Guid.NewGuid().ToString("N"),
                    Status = StatusEnum.Active
                };
                _db.EstudioSocioeconomico.Add(e);
                await _db.SaveChangesAsync();
            }
            else if (string.IsNullOrWhiteSpace(e.Token))
            {
                e.Token = Guid.NewGuid().ToString("N");
                await _db.SaveChangesAsync();
            }

            return e.Token!;
        }

        public async Task<EstudioPublicoDto?> GetByTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return null;

            var e = await _db.EstudioSocioeconomico
                .Include(x => x.EstudioServicioVivienda)
                .Include(x => x.EstudioRecursoTecnologico)
                .Include(x => x.IdAspiranteNavigation).ThenInclude(a => a.IdPersonaNavigation)
                .Include(x => x.IdAspiranteNavigation).ThenInclude(a => a.IdPlanNavigation).ThenInclude(p => p.IdCampusNavigation)
                .FirstOrDefaultAsync(x => x.Token == token);

            if (e == null) return null;

            var per = e.IdAspiranteNavigation?.IdPersonaNavigation;
            var nombre = per == null ? "Aspirante" : $"{per.Nombre} {per.ApellidoPaterno} {per.ApellidoMaterno}".Trim();

            return new EstudioPublicoDto
            {
                AspiranteNombre = nombre,
                Carrera = e.IdAspiranteNavigation?.IdPlanNavigation?.NombrePlanEstudios,
                Campus = e.IdAspiranteNavigation?.IdPlanNavigation?.IdCampusNavigation?.Nombre,
                YaEnviado = e.LlenadoPorAspirante,
                Catalogos = await GetCatalogosAsync(),
                Estudio = await MapAsync(e)
            };
        }

        public async Task<EstudioSocioeconomicoDto?> UpsertByTokenAsync(string token, EstudioSocioeconomicoRequest req)
        {
            if (string.IsNullOrWhiteSpace(token)) return null;

            var e = await _db.EstudioSocioeconomico
                .Include(x => x.EstudioServicioVivienda)
                .Include(x => x.EstudioRecursoTecnologico)
                .FirstOrDefaultAsync(x => x.Token == token);

            if (e == null) return null;

            AplicarEscalares(e, req);
            await SincronizarJuntasAsync(e, req);
            e.FechaLlenado = DateTime.UtcNow;
            e.LlenadoPorAspirante = true;

            await _db.SaveChangesAsync();
            return await MapAsync(e);
        }
    }
}
