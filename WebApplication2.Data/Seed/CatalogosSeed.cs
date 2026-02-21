using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication2.Configuration.Constants;
using WebApplication2.Core.Models;
using WebApplication2.Data.DbContexts;

namespace WebApplication2.Data.Seed
{
    internal static class CatalogosSeed
    {
        public static void Seed(ApplicationDbContext dbContext, bool isDevelopment, UserManager<ApplicationUser> userManager)
        {
            if (!dbContext.DiaSemana.Any())
            {
                var items = new List<DiaSemana>()
                {
                    new DiaSemana { IdDiaSemana = 1, Nombre = "Lunes" },
                    new DiaSemana { IdDiaSemana = 2, Nombre = "Martes" },
                    new DiaSemana { IdDiaSemana = 3, Nombre = "Miércoles" },
                    new DiaSemana { IdDiaSemana = 4, Nombre = "Jueves" },
                    new DiaSemana { IdDiaSemana = 5, Nombre = "Viernes" },
                    new DiaSemana { IdDiaSemana = 6, Nombre = "Sábado" },
                    new DiaSemana { IdDiaSemana = 7, Nombre = "Domingo" },
                };

                dbContext.DiaSemana.AddRange(items);
                dbContext.SaveChanges();
            }

            if (!dbContext.Genero.Any())
            {
                var items = new List<Genero>()
                {
                    new Genero { DescGenero = "Masculino" },
                    new Genero { DescGenero = "Femenino" },
                    new Genero { DescGenero = "No especifica" },
                };

                dbContext.Genero.AddRange(items);
                dbContext.SaveChanges();
            }

            if (!dbContext.EstadoCivil.Any())
            {
                var items = new List<EstadoCivil>()
                {
                    new EstadoCivil { DescEstadoCivil = "Soltero(a)" },
                    new EstadoCivil { DescEstadoCivil = "Casado(a)" },
                };

                dbContext.EstadoCivil.AddRange(items);
                dbContext.SaveChanges();
            }

            if (!dbContext.AspiranteEstatus.Any())
            {
                var items = new List<AspiranteEstatus>()
                {
                    new AspiranteEstatus { DescEstatus = "Nuevo" },
                    new AspiranteEstatus { DescEstatus = "En Proceso" },
                    new AspiranteEstatus { DescEstatus = "Pagado" },
                    new AspiranteEstatus { DescEstatus = "Admitido" },
                    new AspiranteEstatus { DescEstatus = "Inscrito" },
                    new AspiranteEstatus { DescEstatus = "Rechazado" },
                };

                dbContext.AspiranteEstatus.AddRange(items);
                dbContext.SaveChanges();
            }

            if (!dbContext.MedioContacto.Any())
            {
                var items = new List<MedioContacto>()
                {
                    new MedioContacto { DescMedio = "Web", Activo = true },
                    new MedioContacto { DescMedio = "Referido", Activo = true },
                };

                dbContext.MedioContacto.AddRange(items);
                dbContext.SaveChanges();
            }

            if (!dbContext.Turno.Any())
            {
                var items = new List<Turno>()
                {
                    new Turno { Clave = "MAT", Nombre = "Matutino" },
                    new Turno { Clave = "VES", Nombre = "Vespertino" },
                };

                dbContext.Turno.AddRange(items);
                dbContext.SaveChanges();
            }

            if (!dbContext.Modalidad.Any())
            {
                var items = new List<Modalidad>()
                {
                    new Modalidad { DescModalidad = "Presencial", Activo = true },
                    new Modalidad { DescModalidad = "En línea", Activo = true },
                    new Modalidad { DescModalidad = "Mixta", Activo = true },
                };

                dbContext.Modalidad.AddRange(items);
                dbContext.SaveChanges();
            }

            if (!dbContext.ModalidadPlan.Any())
            {
                var items = new List<ModalidadPlan>()
                {
                    new ModalidadPlan { DescModalidadPlan = "Con Título", Activo = true },
                    new ModalidadPlan { DescModalidadPlan = "Sin Título", Activo = true },
                };

                dbContext.ModalidadPlan.AddRange(items);
                dbContext.SaveChanges();
            }

            if (!dbContext.DocumentoRequisito.Any())
            {
                var items = new List<DocumentoRequisito>()
                {
                    new DocumentoRequisito
                    {
                        Clave = "ACTA-NAC",
                        Descripcion = "Acta de Nacimiento",
                        EsObligatorio = true,
                        Orden = 1,
                        Activo = true
                    },
                    new DocumentoRequisito
                    {
                        Clave = "CURP",
                        Descripcion = "CURP",
                        EsObligatorio = true,
                        Orden = 2,
                        Activo = true
                    },
                    new DocumentoRequisito
                    {
                        Clave = "CERT-BACH",
                        Descripcion = "Certificado de Bachillerato",
                        EsObligatorio = true,
                        Orden = 3,
                        Activo = true
                    },
                    new DocumentoRequisito
                    {
                        Clave = "INE",
                        Descripcion = "Identificación Oficial (INE/Pasaporte)",
                        EsObligatorio = true,
                        Orden = 4,
                        Activo = true
                    },
                    new DocumentoRequisito
                    {
                        Clave = "FOTO",
                        Descripcion = "Fotografía tamaño infantil",
                        EsObligatorio = true,
                        Orden = 5,
                        Activo = true
                    },
                    new DocumentoRequisito
                    {
                        Clave = "COMP-DOM",
                        Descripcion = "Comprobante de Domicilio",
                        EsObligatorio = false,
                        Orden = 6,
                        Activo = true
                    }
                };

                dbContext.DocumentoRequisito.AddRange(items);
                dbContext.SaveChanges();
            }

            if (isDevelopment)
            {
                if (!dbContext.CodigosPostales.Any())
                {
                    Console.WriteLine("AVISO: No hay CodigosPostales cargados. Saltando seed de desarrollo.");
                    return;
                }

                try
                {

                if (!dbContext.Periodicidad.Where(p => p.DescPeriodicidad == "Cuatrimestral").Any())
                {
                    var items = new List<Periodicidad>()
                    {
                        new Periodicidad { DescPeriodicidad = "Cuatrimestral", PeriodosPorAnio = 3, MesesPorPeriodo = 4, Activo = true },
                    };

                    dbContext.Periodicidad.AddRange(items);
                    dbContext.SaveChanges();
                }

                if (!dbContext.PeriodoAcademico.Any())
                {
                    var periodicidadCuatId = dbContext.Periodicidad.FirstOrDefault(p => p.DescPeriodicidad == "Cuatrimestral")!.IdPeriodicidad;

                    var items = new List<PeriodoAcademico>()
                    {
                        new PeriodoAcademico 
                        { 
                            Clave = "2025-Q1", 
                            Nombre = "Cuatrimestre 1 2025", 
                            IdPeriodicidad = periodicidadCuatId, 
                            FechaInicio = DateOnly.Parse("2025-01-06"), 
                            FechaFin = DateOnly.Parse("2025-04-26")
                        },
                    };

                    dbContext.PeriodoAcademico.AddRange(items);
                    dbContext.SaveChanges();
                }

                if (!dbContext.NivelEducativo.Any())
                {
                    var items = new List<NivelEducativo>()
                    {
                        new NivelEducativo { DescNivelEducativo = "Licenciatura", Activo = true },
                    };

                    dbContext.NivelEducativo.AddRange(items);
                    dbContext.SaveChanges();
                }

                if (!dbContext.Campus.Any())
                {
                    var items = new List<Campus>()
                    {
                        new Campus { 
                            ClaveCampus = "CENT",
                            Nombre = "Campus Centro",
                            IdDireccionNavigation = new Direccion
                            {
                                Calle = "Av. Juárez",
                                NumeroExterior = "100",
                                CodigoPostalId = dbContext.CodigosPostales.FirstOrDefault()!.Id
                            } 
                        },
                    };

                    dbContext.Campus.AddRange(items);
                    dbContext.SaveChanges();
                }

                if (!dbContext.PlanEstudios.Any())
                {
                    var items = new List<PlanEstudios>
                    {
                        new PlanEstudios
                        {
                            ClavePlanEstudios = "ISIC-2025",
                            NombrePlanEstudios = "Ing. Sistemas 2025",
                            RVOE = "RVOE-1234",
                            PermiteAdelantar = false,
                            Version = "v1",
                            DuracionMeses = 48,
                            MinimaAprobatoriaParcial = 60,
                            MinimaAprobatoriaFinal = 70,
                            IdPeriodicidad = dbContext.Periodicidad.SingleOrDefault(p => p.DescPeriodicidad == "Cuatrimestral")!.IdPeriodicidad,
                            IdNivelEducativo = dbContext.NivelEducativo.SingleOrDefault(p => p.DescNivelEducativo == "Licenciatura")!.IdNivelEducativo,
                            IdCampus = dbContext.Campus.SingleOrDefault(c => c.ClaveCampus == "CENT")!.IdCampus,
                        }
                    };

                    dbContext.PlanEstudios.AddRange(items);
                    dbContext.SaveChanges();
                }

                if (!dbContext.Materia.Any())
                {
                    var items = new List<Materia>()
                    {
                        new Materia 
                        { 
                            Clave = "ALG101", 
                            Nombre = "Álgebra", 
                            Creditos = 8, 
                            HorasTeoria = 4, 
                            HorasPractica = 2, 
                            Activa = true
                        },
                        new Materia 
                        { 
                            Clave = "BD201", 
                            Nombre = "Bases de Datos", 
                            Creditos = 9, 
                            HorasTeoria = 3, 
                            HorasPractica = 3, 
                            Activa = true
                        },
                        new Materia 
                        { 
                            Clave = "PROG101", 
                            Nombre = "Programación", 
                            Creditos = 9, 
                            HorasTeoria = 3, 
                            HorasPractica = 3, 
                            Activa = true
                        },
                    };

                    dbContext.Materia.AddRange(items);
                    dbContext.SaveChanges();
                }

                if (!dbContext.MateriaPlan.Any())
                {
                    var planEstudiosId = dbContext.PlanEstudios.SingleOrDefault(p => p.ClavePlanEstudios == "ISIC-2025")!.IdPlanEstudios;

                    var items = new List<MateriaPlan>()
                    {
                        new MateriaPlan 
                        { 
                            IdPlanEstudios = planEstudiosId, 
                            IdMateria =  dbContext.Materia.FirstOrDefault(m => m.Clave == "ALG101")!.IdMateria,
                            Cuatrimestre = 1,
                            EsOptativa = false
                        },
                        new MateriaPlan 
                        {
                            IdPlanEstudios = planEstudiosId,
                            IdMateria =  dbContext.Materia.FirstOrDefault(m => m.Clave == "PROG101")!.IdMateria,
                            Cuatrimestre = 1,
                            EsOptativa = false
                        },
                        new MateriaPlan 
                        {
                            IdPlanEstudios = planEstudiosId,
                            IdMateria =  dbContext.Materia.FirstOrDefault(m => m.Clave == "BD201")!.IdMateria,
                            Cuatrimestre = 2,
                            EsOptativa = false
                        },
                    };

                    dbContext.MateriaPlan.AddRange(items);
                    dbContext.SaveChanges();
                }

                if (!dbContext.Grupo.Any())
                {
                    var planEstudiosId = dbContext.PlanEstudios.SingleOrDefault(p => p.ClavePlanEstudios == "ISIC-2025")!.IdPlanEstudios;

                    var periodoAcademico2025Q1 = dbContext.PeriodoAcademico.FirstOrDefault(p => p.Clave == "2025-Q1")!.IdPeriodoAcademico;

                    var turnoMatutino = dbContext.Turno.FirstOrDefault(t => t.Clave == "MAT")!.IdTurno;

                    var items = new List<Grupo>()
                    {
                        new Grupo 
                        {
                            IdPlanEstudios = planEstudiosId,
                            IdPeriodoAcademico = periodoAcademico2025Q1,
                            NumeroCuatrimestre = 1,
                            NumeroGrupo = 1,
                            IdTurno = turnoMatutino,
                            CapacidadMaxima = 40
                        },
                    };

                    dbContext.Grupo.AddRange(items);
                    dbContext.SaveChanges();
                }

                if (!dbContext.Profesor.Any())
                {
                    userManager.InsertUser("ana.lopez@uni.mx", "EMP-001", Rol.DOCENTE);

                    var user = userManager.FindByEmailAsync("ana.lopez@uni.mx").Result;

                    var items = new List<Profesor>()
                    {
                        new Profesor 
                        {
                            NoEmpleado = "EMP-001",
                            IdPersonaNavigation = new Persona
                            {
                                Nombre = "Ana",
                                ApellidoPaterno = "López",
                                ApellidoMaterno = "García",
                                FechaNacimiento = DateOnly.Parse("1985-05-10"),
                                Curp = "LOGA850510MJCXXX01",
                                Rfc = "LOGA850510XXX",
                                IdDireccionNavigation = new Direccion
                                {
                                    Calle = "5 De Mayo",
                                    NumeroExterior = "100",
                                    CodigoPostalId = dbContext.CodigosPostales.FirstOrDefault()!.Id
                                },
                                Correo = "ana.lopez@gmail.com",
                                IdGenero = dbContext.Genero.FirstOrDefault(g => g.DescGenero == "Femenino")!.IdGenero,
                                IdEstadoCivil = dbContext.EstadoCivil.FirstOrDefault(ec => ec.DescEstadoCivil == "Casado(a)")!.IdEstadoCivil,
                            },
                            EmailInstitucional = "ana.lopez@uni.mx",
                            Activo = true,
                            UsuarioId = user!.Id
                        },
                    };

                    dbContext.Profesor.AddRange(items);
                    dbContext.SaveChanges();
                }

                if (!dbContext.GrupoMateria.Any())
                {
                    var grupoDefaultId = dbContext.Grupo.FirstOrDefault()!.IdGrupo;

                    var planEstudiosId = dbContext.PlanEstudios.SingleOrDefault(p => p.ClavePlanEstudios == "ISIC-2025")!.IdPlanEstudios;

                    var materiaPlanAlgebraId = dbContext.MateriaPlan
                        .Include(mp => mp.IdMateriaNavigation)
                        .FirstOrDefault(mp => mp.IdMateriaNavigation.Clave == "ALG101" && mp.IdPlanEstudios == planEstudiosId)!.IdMateriaPlan;

                    var materiaPlanProgramacionId = dbContext.MateriaPlan
                        .Include(mp => mp.IdMateriaNavigation)
                        .FirstOrDefault(mp => mp.IdMateriaNavigation.Clave == "PROG101" && mp.IdPlanEstudios == planEstudiosId)!.IdMateriaPlan;

                    var idProfesoraAna = dbContext.Profesor
                        .Include(p => p.IdPersonaNavigation)
                        .FirstOrDefault(p => p.IdPersonaNavigation.Curp == "LOGA850510MJCXXX01")!.IdProfesor;

                    var items = new List<GrupoMateria>()
                    {
                        new GrupoMateria
                        {
                            IdGrupo = grupoDefaultId,
                            IdMateriaPlan = materiaPlanAlgebraId,
                            IdProfesor = idProfesoraAna,
                            Aula = "Aula 101",
                            Cupo = 40
                        },
                        new GrupoMateria
                        {
                            IdGrupo = grupoDefaultId,
                            IdMateriaPlan = materiaPlanProgramacionId,
                            IdProfesor = idProfesoraAna,
                            Aula = "Aula 102",
                            Cupo = 40
                        },
                    };

                    dbContext.GrupoMateria.AddRange(items);
                    dbContext.SaveChanges();
                }

                if (!dbContext.Horario.Any())
                {
                    var grupoDefaultId = dbContext.Grupo.FirstOrDefault()!.IdGrupo;

                     var planEstudiosId = dbContext.PlanEstudios.SingleOrDefault(p => p.ClavePlanEstudios == "ISIC-2025")!.IdPlanEstudios;

                    var materiaPlanAlgebraId = dbContext.MateriaPlan
                        .Include(mp => mp.IdMateriaNavigation)
                        .FirstOrDefault(mp => mp.IdMateriaNavigation.Clave == "ALG101" && mp.IdPlanEstudios == planEstudiosId)!.IdMateriaPlan;

                    var materiaPlanProgramacionId = dbContext.MateriaPlan
                        .Include(mp => mp.IdMateriaNavigation)
                        .FirstOrDefault(mp => mp.IdMateriaNavigation.Clave == "PROG101" && mp.IdPlanEstudios == planEstudiosId)!.IdMateriaPlan;

                    var idProfesoraAna = dbContext.Profesor
                        .Include(p => p.IdPersonaNavigation)
                        .FirstOrDefault(p => p.IdPersonaNavigation.Curp == "LOGA850510MJCXXX01")!.IdProfesor;

                    var grupoAlgebraAnaId = dbContext.GrupoMateria
                        .FirstOrDefault(gm => gm.IdGrupo == grupoDefaultId && gm.IdMateriaPlan == materiaPlanAlgebraId && gm.IdProfesor == idProfesoraAna)!.IdMateriaPlan;

                    var grupoProgramacionAnaId = dbContext.GrupoMateria
                        .FirstOrDefault(gm => gm.IdGrupo == grupoDefaultId && gm.IdMateriaPlan == materiaPlanProgramacionId && gm.IdProfesor == idProfesoraAna)!.IdMateriaPlan;

                    var items = new List<Horario>()
                    {
                        new Horario {
                            IdGrupoMateria = grupoAlgebraAnaId,
                            IdDiaSemana = dbContext.DiaSemana.FirstOrDefault(ds => ds.Nombre == "Lunes")!.IdDiaSemana,
                            HoraInicio = TimeOnly.Parse("08:00"),
                            HoraFin = TimeOnly.Parse("10:00"),
                            Aula = "Aula 101",
                        },
                        new Horario {
                            IdGrupoMateria = grupoAlgebraAnaId,
                            IdDiaSemana = dbContext.DiaSemana.FirstOrDefault(ds => ds.Nombre == "Miércoles")!.IdDiaSemana,
                            HoraInicio = TimeOnly.Parse("08:00"),
                            HoraFin = TimeOnly.Parse("10:00"),
                            Aula = "Aula 101",
                        },
                        new Horario {
                            IdGrupoMateria = grupoProgramacionAnaId,
                            IdDiaSemana = dbContext.DiaSemana.FirstOrDefault(ds => ds.Nombre == "Martes")!.IdDiaSemana,
                            HoraInicio = TimeOnly.Parse("10:00"),
                            HoraFin = TimeOnly.Parse("12:00"),
                            Aula = "Aula 102",
                        },
                        new Horario {
                            IdGrupoMateria = grupoProgramacionAnaId,
                            IdDiaSemana = dbContext.DiaSemana.FirstOrDefault(ds => ds.Nombre == "Jueves")!.IdDiaSemana,
                            HoraInicio = TimeOnly.Parse("10:00"),
                            HoraFin = TimeOnly.Parse("12:00"),
                            Aula = "Aula 102",
                        },
                    };

                    dbContext.Horario.AddRange(items);
                    dbContext.SaveChanges();
                }

                if (!dbContext.Estudiante.Any())
                {
                    userManager.InsertUser("A0000001@usag.com", "A0000001", Rol.ALUMNO);

                    var user = userManager.FindByEmailAsync("A0000001@usag.com").Result;

                    var planEstudiosISIC2025 = dbContext.PlanEstudios.FirstOrDefault(pe => pe.ClavePlanEstudios == "ISIC-2025")!.IdPlanEstudios;

                    var items = new List<Estudiante>()
                    {
                        new Estudiante {
                            Matricula = "A0000001",
                            IdPersonaNavigation = new Persona
                            {
                                Nombre = "Carlos",
                                ApellidoPaterno = "Pérez",
                                ApellidoMaterno = "Santos",
                                FechaNacimiento = DateOnly.Parse("2005-02-20"),
                                Curp = "PESC050220HJCXXX02",
                                Rfc = "PESC050220XXX",
                                IdDireccionNavigation = new Direccion
                                {
                                    Calle = "Avenida México",
                                    NumeroExterior = "252 B",
                                    CodigoPostalId = dbContext.CodigosPostales.FirstOrDefault()!.Id
                                },
                                Correo = "carlos.perez@gmail.com",
                                IdGenero = dbContext.Genero.FirstOrDefault(g => g.DescGenero == "Masculino")!.IdGenero,
                                IdEstadoCivil = dbContext.EstadoCivil.FirstOrDefault(ec => ec.DescEstadoCivil == "Soltero(a)")!.IdEstadoCivil,
                            },
                            Email = "carlos.alumno@uni.mx",
                            FechaIngreso = DateOnly.Parse("2025-01-01"),
                            IdPlanActual = planEstudiosISIC2025,
                            Activo = true,
                            UsuarioId = user!.Id
                        },
                    };

                    dbContext.Estudiante.AddRange(items);
                    dbContext.SaveChanges();
                }

                if (!dbContext.EstudiantePlan.Any())
                {
                    var estudianteA0000001 = dbContext.Estudiante.FirstOrDefault(e => e.Matricula == "A0000001")!.IdEstudiante;

                    var planEstudiosISIC2025 = dbContext.PlanEstudios.FirstOrDefault(pe => pe.ClavePlanEstudios == "ISIC-2025")!.IdPlanEstudios;

                    var items = new List<EstudiantePlan>()
                    {
                        new EstudiantePlan {
                            IdEstudiante = estudianteA0000001,
                            IdPlanEstudios = planEstudiosISIC2025,
                            FechaInicio = DateOnly.Parse("2025-01-01")
                        },
                    };

                    dbContext.EstudiantePlan.AddRange(items);
                    dbContext.SaveChanges();
                }

                if (!dbContext.Inscripcion.Any())
                {
                    var estudianteA0000001 = dbContext.Estudiante.FirstOrDefault(e => e.Matricula == "A0000001")!.IdEstudiante;

                    var planEstudiosISIC2025 = dbContext.PlanEstudios.FirstOrDefault(pe => pe.ClavePlanEstudios == "ISIC-2025")!.IdPlanEstudios;

                    var grupoDefaultId = dbContext.Grupo.FirstOrDefault()!.IdGrupo;

                    var planEstudiosId = dbContext.PlanEstudios.SingleOrDefault(p => p.ClavePlanEstudios == "ISIC-2025")!.IdPlanEstudios;

                    var materiaPlanAlgebraId = dbContext.MateriaPlan
                        .Include(mp => mp.IdMateriaNavigation)
                        .FirstOrDefault(mp => mp.IdMateriaNavigation.Clave == "ALG101" && mp.IdPlanEstudios == planEstudiosId)!.IdMateriaPlan;

                    var materiaPlanProgramacionId = dbContext.MateriaPlan
                        .Include(mp => mp.IdMateriaNavigation)
                        .FirstOrDefault(mp => mp.IdMateriaNavigation.Clave == "PROG101" && mp.IdPlanEstudios == planEstudiosId)!.IdMateriaPlan;

                    var idProfesoraAna = dbContext.Profesor
                        .Include(p => p.IdPersonaNavigation)
                        .FirstOrDefault(p => p.IdPersonaNavigation.Curp == "LOGA850510MJCXXX01")!.IdProfesor;

                    var grupoAlgebraAnaId = dbContext.GrupoMateria
                        .FirstOrDefault(gm => gm.IdGrupo == grupoDefaultId && gm.IdMateriaPlan == materiaPlanAlgebraId && gm.IdProfesor == idProfesoraAna)!.IdMateriaPlan;

                    var grupoProgramacionAnaId = dbContext.GrupoMateria
                        .FirstOrDefault(gm => gm.IdGrupo == grupoDefaultId && gm.IdMateriaPlan == materiaPlanProgramacionId && gm.IdProfesor == idProfesoraAna)!.IdMateriaPlan;

                    var items = new List<Inscripcion>()
                    {
                        new Inscripcion {
                            IdEstudiante = estudianteA0000001,
                            IdGrupoMateria = grupoAlgebraAnaId,
                            Estado = "Inscrito"
                        },
                        new Inscripcion {
                            IdEstudiante = estudianteA0000001,
                            IdGrupoMateria = grupoProgramacionAnaId,
                            Estado = "Inscrito"
                        },
                    };

                    dbContext.Inscripcion.AddRange(items);
                    dbContext.SaveChanges();
                }

                if (!dbContext.Convenio.Any())
                {
                    var items = new List<Convenio>()
                    {
                        new Convenio
                        {
                            ClaveConvenio = "EMP-2025",
                            Nombre = "Convenio Empresas 2025",
                            TipoBeneficio = "PORCENTAJE",
                            DescuentoPct = 20m,
                            VigenteDesde = DateOnly.Parse("2025-01-01"),
                            VigenteHasta = DateOnly.Parse("2025-12-31"),
                            Activo = true
                        },
                        new Convenio
                        {
                            ClaveConvenio = "ALUMNI",
                            Nombre = "Beneficio Egresados",
                            TipoBeneficio = "MONTO",
                            DescuentoPct = 900m,
                            Activo = true
                        },
                    };

                    dbContext.Convenio.AddRange(items);
                    dbContext.SaveChanges();
                }

                if (!dbContext.ConvenioAlcance.Any())
                {
                    var convenioEMP2025Id = dbContext.Convenio.FirstOrDefault(c => c.ClaveConvenio == "EMP-2025")!.IdConvenio;

                    var campusCENTId = dbContext.Campus.FirstOrDefault(c => c.ClaveCampus == "CENT")!.IdCampus;

                    var planEstudiosISIC2025Id = dbContext.PlanEstudios.FirstOrDefault(pe => pe.ClavePlanEstudios == "ISIC-2025")!.IdPlanEstudios;

                    var items = new List<ConvenioAlcance>()
                    {
                        new ConvenioAlcance
                        {
                            IdConvenio = convenioEMP2025Id,
                            IdCampus = campusCENTId, 
                            IdPlanEstudios = planEstudiosISIC2025Id, 
                            VigenteDesde = DateOnly.Parse("2025-01-01"),
                            VigenteHasta = DateOnly.Parse("2025-12-31"),
                        },
                    };

                    dbContext.ConvenioAlcance.AddRange(items);
                    dbContext.SaveChanges();
                }

                if (!dbContext.Aspirante.Any())
                {
                    var convenioEMP2025Id = dbContext.Convenio.FirstOrDefault(c => c.ClaveConvenio == "EMP-2025")!.IdConvenio;

                    var campusCENTId = dbContext.Campus.FirstOrDefault(c => c.ClaveCampus == "CENT")!.IdCampus;

                    var planEstudiosISIC2025Id = dbContext.PlanEstudios.FirstOrDefault(pe => pe.ClavePlanEstudios == "ISIC-2025")!.IdPlanEstudios;

                    var items = new List<Aspirante>()
                    {
                        new Aspirante
                        {
                            IdPersona = dbContext.Estudiante.FirstOrDefault(e => e.Matricula == "A0000001")!.IdPersona,
                            IdAspiranteEstatus = dbContext.AspiranteEstatus.FirstOrDefault(ae => ae.DescEstatus == "Nuevo")!.IdAspiranteEstatus,
                            IdPlan = planEstudiosISIC2025Id,
                            IdMedioContacto = dbContext.MedioContacto.FirstOrDefault(mc => mc.DescMedio == "Web")!.IdMedioContacto,
                            Observaciones = "Interesado en turno matutino"
                        },
                    };

                    dbContext.Aspirante.AddRange(items);
                    dbContext.SaveChanges();
                }

                if (!dbContext.AspiranteConvenio.Any())
                {
                    var convenioEMP2025Id = dbContext.Convenio.FirstOrDefault(c => c.ClaveConvenio == "EMP-2025")!.IdConvenio;

                    var aspiranteA0000001 = dbContext.Aspirante.Include(a => a.IdPersonaNavigation).FirstOrDefault(a => a.IdPersonaNavigation!.Curp == "PESC050220HJCXXX02")!.IdAspirante;

                    var items = new List<AspiranteConvenio>()
                    {
                        new AspiranteConvenio
                        {
                            IdAspirante = aspiranteA0000001,
                            IdConvenio = convenioEMP2025Id,
                            Estatus = "Pendiente",
                            Evidencia = "Folio EMP-XYZ-123"
                        },
                    };

                    dbContext.AspiranteConvenio.AddRange(items);
                    dbContext.SaveChanges();
                }

                if (!dbContext.ConceptoPago.Any(c => c.Clave == "INSCRIPCION"))
                {
                    var conceptoInscripcion = new ConceptoPago
                    {
                        Clave = "INSCRIPCION",
                        Nombre = "Inscripción",
                        Descripcion = "Cuota de Inscripción",
                        Tipo = Core.Enums.ConceptoTipoEnum.INSCRIPCION,
                        AplicaA = Core.Enums.ConceptoAplicaAEnum.Aspirante,
                        EsObligatorio = true,
                        PermiteBeca = true,
                        Activo = true
                    };

                    dbContext.ConceptoPago.Add(conceptoInscripcion);
                    dbContext.SaveChanges();

                    var precioInscripcion = new ConceptoPrecio
                    {
                        IdConceptoPago = conceptoInscripcion.IdConceptoPago,
                        Moneda = "MXN",
                        Importe = 2500.00m, 
                        VigenciaDesde = DateOnly.Parse("2025-01-01"),
                        VigenciaHasta = null, 
                        Activo = true
                    };

                    dbContext.ConceptoPrecio.Add(precioInscripcion);
                    dbContext.SaveChanges();
                }

                if (!dbContext.ConceptoPago.Any(c => c.Clave == "FICHA"))
                {
                    var conceptoFicha = new ConceptoPago
                    {
                        Clave = "FICHA",
                        Nombre = "Ficha de Admisión",
                        Descripcion = "Ficha de Admisión",
                        Tipo = Core.Enums.ConceptoTipoEnum.OTRO,
                        AplicaA = Core.Enums.ConceptoAplicaAEnum.Aspirante,
                        EsObligatorio = true,
                        PermiteBeca = false,
                        Activo = true
                    };

                    dbContext.ConceptoPago.Add(conceptoFicha);
                    dbContext.SaveChanges();

                    var precioFicha = new ConceptoPrecio
                    {
                        IdConceptoPago = conceptoFicha.IdConceptoPago,
                        Moneda = "MXN",
                        Importe = 500.00m, 
                        VigenciaDesde = DateOnly.Parse("2025-01-01"),
                        VigenciaHasta = null,
                        Activo = true
                    };

                    dbContext.ConceptoPrecio.Add(precioFicha);
                    dbContext.SaveChanges();
                }
                }
                catch (NullReferenceException ex)
                {
                    Console.WriteLine($"AVISO: Error en seed de desarrollo (datos faltantes): {ex.Message}");
                    Console.WriteLine("Saltando seed de desarrollo. Esto es normal si ya eliminaste datos de prueba.");
                }
            }
        }

        private static void InsertUser(this UserManager<ApplicationUser> userManager, string email, string password, string rol)
        {
            if (userManager.FindByEmailAsync(email).Result == null)
            {
                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email
                };

                var result = userManager.CreateAsync(user, password).Result;

                if (result.Succeeded)
                {
                    userManager.AddClaimAsync(user, new Claim(ClaimTypes.Role, rol)).Wait();
                    userManager.AddToRoleAsync(user, rol).Wait();
                }
            }
        }
    }
}
