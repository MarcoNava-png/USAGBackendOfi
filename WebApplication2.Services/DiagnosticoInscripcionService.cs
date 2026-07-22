using Microsoft.EntityFrameworkCore;
using WebApplication2.Core.DTOs.Diagnostico;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;
using StatusEnum = WebApplication2.Core.Enums.StatusEnum;

namespace WebApplication2.Services
{
    public class DiagnosticoInscripcionService : IDiagnosticoInscripcionService
    {
        private readonly ApplicationDbContext _db;
        private readonly IGrupoService _grupoService;

        public DiagnosticoInscripcionService(ApplicationDbContext db, IGrupoService grupoService)
        {
            _db = db;
            _grupoService = grupoService;
        }

        public async Task<List<InconsistenciaInscripcionDto>> DetectarAsync(int idPeriodoAcademico)
        {
            var resultado = new List<InconsistenciaInscripcionDto>();

            var esPeriodoVigente = await _db.PeriodoAcademico
                .Where(p => p.IdPeriodoAcademico == idPeriodoAcademico)
                .Select(p => p.EsPeriodoActual)
                .FirstOrDefaultAsync();

            var grupos = await _db.Grupo
                .Where(g => g.IdPeriodoAcademico == idPeriodoAcademico && g.Status == StatusEnum.Active)
                .Select(g => new { g.IdGrupo, g.CodigoGrupo, g.IdPlanEstudios })
                .ToListAsync();

            foreach (var g in grupos)
            {
                var materias = await _db.GrupoMateria
                    .Where(gm => gm.IdGrupo == g.IdGrupo && gm.Status == StatusEnum.Active)
                    .Select(gm => gm.IdGrupoMateria).ToListAsync();

                var conVinculo = (await _db.EstudianteGrupo
                    .Where(eg => eg.IdGrupo == g.IdGrupo && eg.Status == StatusEnum.Active)
                    .Select(eg => eg.IdEstudiante).ToListAsync()).ToHashSet();

                var conMaterias = (materias.Count == 0 ? new List<int>() : await _db.Inscripcion
                    .Where(i => materias.Contains(i.IdGrupoMateria) && i.Status == StatusEnum.Active)
                    .Select(i => i.IdEstudiante).Distinct().ToListAsync()).ToHashSet();

                foreach (var idEst in conMaterias.Where(x => !conVinculo.Contains(x)))
                    resultado.Add(new InconsistenciaInscripcionDto
                    {
                        IdEstudiante = idEst,
                        Tipo = "MateriasSinVinculo",
                        IdGrupo = g.IdGrupo,
                        CodigoGrupo = g.CodigoGrupo,
                        Descripcion = $"Tiene materias del grupo {g.CodigoGrupo} pero sin vínculo al grupo (sale 'sin grupo' en su panel)"
                    });

                if (materias.Count > 0)
                    foreach (var idEst in conVinculo.Where(x => !conMaterias.Contains(x)))
                        resultado.Add(new InconsistenciaInscripcionDto
                        {
                            IdEstudiante = idEst,
                            Tipo = "VinculoSinMaterias",
                            IdGrupo = g.IdGrupo,
                            CodigoGrupo = g.CodigoGrupo,
                            Descripcion = $"Está en el grupo {g.CodigoGrupo} pero sin materias inscritas"
                        });

                var planCruzados = !esPeriodoVigente ? new List<int>() : await _db.Estudiante
                    .Where(e => conVinculo.Contains(e.IdEstudiante)
                        && e.IdPlanActual != g.IdPlanEstudios
                        && e.Status == StatusEnum.Active)
                    .Select(e => e.IdEstudiante).ToListAsync();
                foreach (var idEst in planCruzados)
                    resultado.Add(new InconsistenciaInscripcionDto
                    {
                        IdEstudiante = idEst,
                        Tipo = "PlanCruzado",
                        IdGrupo = g.IdGrupo,
                        CodigoGrupo = g.CodigoGrupo,
                        Descripcion = $"Su plan/campus registrado no coincide con el del grupo {g.CodigoGrupo}"
                    });
            }

            var idsGrupoPeriodo = grupos.Select(g => g.IdGrupo).ToList();
            var estConGrupoPeriodo = (await _db.EstudianteGrupo
                .Where(eg => idsGrupoPeriodo.Contains(eg.IdGrupo) && eg.Status == StatusEnum.Active)
                .Select(eg => eg.IdEstudiante).Distinct().ToListAsync()).ToHashSet();

            var apartadosColgados = await _db.PreInscripcion
                .Where(pi => pi.IdPeriodoAcademicoDestino == idPeriodoAcademico
                    && pi.Estado == "Pendiente"
                    && pi.Status == StatusEnum.Active)
                .Select(pi => pi.IdEstudiante).Distinct().ToListAsync();
            foreach (var idEst in apartadosColgados.Where(estConGrupoPeriodo.Contains))
                resultado.Add(new InconsistenciaInscripcionDto
                {
                    IdEstudiante = idEst,
                    Tipo = "ApartadoColgado",
                    Descripcion = "Tiene un apartado 'pendiente' de este periodo aunque ya está inscrito en un grupo"
                });

            var final = new List<InconsistenciaInscripcionDto>();
            foreach (var grp in resultado.GroupBy(r => r.IdEstudiante))
            {
                var materiasSinV = grp.Where(r => r.Tipo == "MateriasSinVinculo").ToList();
                var vinculoSinM = grp.Where(r => r.Tipo == "VinculoSinMaterias").ToList();

                if (materiasSinV.Count > 0 && vinculoSinM.Count > 0)
                {
                    var correcto = materiasSinV[0];
                    foreach (var sobrante in vinculoSinM)
                        final.Add(new InconsistenciaInscripcionDto
                        {
                            IdEstudiante = grp.Key,
                            Tipo = "PartidoEntreGrupos",
                            IdGrupo = correcto.IdGrupo,
                            CodigoGrupo = correcto.CodigoGrupo,
                            IdGrupoSobrante = sobrante.IdGrupo,
                            CodigoGrupoSobrante = sobrante.CodigoGrupo,
                            Descripcion = $"Partido entre 2 grupos: estudia en el {correcto.CodigoGrupo} (tiene materias) pero le quedó un vínculo sobrante en el {sobrante.CodigoGrupo}. Al reparar se conserva el {correcto.CodigoGrupo} y se quita el sobrante."
                        });
                    final.AddRange(materiasSinV.Skip(1));
                    final.AddRange(grp.Where(r => r.Tipo != "MateriasSinVinculo" && r.Tipo != "VinculoSinMaterias"));
                }
                else
                {
                    final.AddRange(grp);
                }
            }

            var ids = final.Select(r => r.IdEstudiante).Distinct().ToList();
            var estudiantes = await _db.Estudiante.Include(e => e.IdPersonaNavigation)
                .Where(e => ids.Contains(e.IdEstudiante))
                .ToDictionaryAsync(e => e.IdEstudiante);
            foreach (var r in final)
            {
                if (estudiantes.TryGetValue(r.IdEstudiante, out var e))
                {
                    r.Matricula = e.Matricula;
                    var p = e.IdPersonaNavigation;
                    r.NombreCompleto = p != null ? $"{p.Nombre} {p.ApellidoPaterno} {p.ApellidoMaterno}".Trim() : "";
                }
            }

            return final.OrderBy(r => r.Matricula).ThenBy(r => r.Tipo).ToList();
        }

        public async Task<RepararInscripcionResultDto> RepararAsync(RepararInscripcionRequest req)
        {
            try
            {
                if (req.Tipo == "ApartadoColgado")
                {
                    var apartados = await _db.PreInscripcion
                        .Where(pi => pi.IdEstudiante == req.IdEstudiante && pi.Estado == "Pendiente" && pi.Status == StatusEnum.Active)
                        .ToListAsync();
                    int cerrados = 0;
                    foreach (var ap in apartados)
                    {
                        var tieneGrupo = await _db.EstudianteGrupo
                            .AnyAsync(eg => eg.IdEstudiante == req.IdEstudiante && eg.Status == StatusEnum.Active
                                && eg.IdGrupoNavigation.IdPeriodoAcademico == ap.IdPeriodoAcademicoDestino);
                        if (tieneGrupo)
                        {
                            ap.Estado = "Completada";
                            ap.Status = StatusEnum.Deleted;
                            ap.UpdatedAt = DateTime.UtcNow;
                            cerrados++;
                        }
                    }
                    await _db.SaveChangesAsync();
                    return new RepararInscripcionResultDto
                    {
                        Exitoso = cerrados > 0,
                        Mensaje = cerrados > 0 ? $"{cerrados} apartado(s) cerrado(s)" : "No había apartados colgados que cerrar"
                    };
                }

                if (req.Tipo == "PartidoEntreGrupos")
                {
                    string codigoCorrecto = "";
                    if (req.IdGrupo.HasValue)
                    {
                        var r = await _grupoService.InscribirEstudianteGrupoAsync(req.IdGrupo.Value, req.IdEstudiante, true, "Reparación (partido)");
                        codigoCorrecto = r.CodigoGrupo;
                    }

                    if (req.IdGrupoSobrante.HasValue)
                    {
                        var vinculos = await _db.EstudianteGrupo
                            .Where(eg => eg.IdEstudiante == req.IdEstudiante && eg.IdGrupo == req.IdGrupoSobrante.Value && eg.Status == StatusEnum.Active)
                            .ToListAsync();
                        foreach (var v in vinculos) { v.Status = StatusEnum.Deleted; v.UpdatedAt = DateTime.UtcNow; }

                        var materiasSobrante = await _db.GrupoMateria
                            .Where(gm => gm.IdGrupo == req.IdGrupoSobrante.Value)
                            .Select(gm => gm.IdGrupoMateria).ToListAsync();
                        var inscSobrante = await _db.Inscripcion
                            .Where(i => i.IdEstudiante == req.IdEstudiante && materiasSobrante.Contains(i.IdGrupoMateria) && i.Status == StatusEnum.Active)
                            .ToListAsync();
                        foreach (var ins in inscSobrante) { ins.Status = StatusEnum.Deleted; ins.UpdatedAt = DateTime.UtcNow; }

                        await _db.SaveChangesAsync();
                    }

                    return new RepararInscripcionResultDto
                    {
                        Exitoso = true,
                        Mensaje = $"Conservado en grupo {codigoCorrecto}, vínculo sobrante quitado"
                    };
                }

                if (req.IdGrupo.HasValue)
                {
                    var r = await _grupoService.InscribirEstudianteGrupoAsync(req.IdGrupo.Value, req.IdEstudiante, true, "Reparación (diagnóstico)");
                    return new RepararInscripcionResultDto
                    {
                        Exitoso = true,
                        Mensaje = $"Reparado: vínculo y materias del grupo {r.CodigoGrupo} (plan/campus sincronizado)"
                    };
                }

                return new RepararInscripcionResultDto { Exitoso = false, Mensaje = "No se pudo determinar la reparación" };
            }
            catch (Exception ex)
            {
                return new RepararInscripcionResultDto { Exitoso = false, Mensaje = ex.Message };
            }
        }
    }
}
