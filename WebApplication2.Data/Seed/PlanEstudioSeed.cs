using WebApplication2.Core.Models;
using WebApplication2.Data.DbContexts;

namespace WebApplication2.Data.Seed
{
    public static class PlanEstudioSeed
    {
        //public static void Seed(ApplicationDbContext dbContext)
        //{
        //    if (!dbContext.Departamentos.Any())
        //    {
        //        var items = new List<Departamento>()
        //        {
        //            new Departamento { Nombre = "General" },
        //            new Departamento { Nombre = "Ciencias e ingeniería" },
        //            new Departamento { Nombre = "Salud" },
        //            new Departamento { Nombre = "Económico-administrativas" },
        //            new Departamento { Nombre = "Sociales y jurídicas" },
        //            new Departamento { Nombre = "Humanidades, artes y arquitectura" },
        //            new Departamento { Nombre = "Educación, deporte y comunicación" },
        //            new Departamento { Nombre = "Agro y veterinaria" },
        //            new Departamento { Nombre = "Transversales / apoyo académico" }
        //        };

        //        dbContext.Departamentos.AddRange(items);
        //        dbContext.SaveChanges();
        //    }

        //    if (!dbContext.Programas.Any())
        //    {
        //        var programas = new List<Programa>()
        //        {
        //            new Programa
        //            {
        //                Nombre = "BACHILLERATO",
        //                DepartamentoId = dbContext.Departamentos.First(d => d.Nombre == "General").Id,
        //                Nivel = 1,
        //            },
        //            new Programa
        //            {
        //                Nombre = "LICENCIATURA EN RADIOLOGÍA E IMAGEN",
        //                DepartamentoId = dbContext.Departamentos.First(d => d.Nombre == "Ciencias e ingeniería").Id,
        //                Nivel = 2,
        //            },
        //            new Programa
        //            {
        //                Nombre = "LICENCIATURA EN ENFERMERÍA",
        //                DepartamentoId = dbContext.Departamentos.First(d => d.Nombre == "Salud").Id,
        //                Nivel = 2,
        //            },
        //            new Programa
        //            {
        //                Nombre = "LICENCIATURA EN INGENIERÍA MECÁNICA Y ELECTRÓNICA AUTOMOTRIZ",
        //                DepartamentoId = dbContext.Departamentos.First(d => d.Nombre == "Ciencias e ingeniería").Id,
        //                Nivel = 2,
        //            },
        //            new Programa
        //            {
        //                Nombre = "LICENCIATURA EN INGENIERÍA INDUSTRIAL",
        //                DepartamentoId = dbContext.Departamentos.First(d => d.Nombre == "Ciencias e ingeniería").Id,
        //                Nivel = 2,
        //            },
        //            new Programa
        //            {
        //                Nombre = "LICENCIATURA EN INGENIERÍA EN SOFTWARE Y SISTEMAS",
        //                DepartamentoId = dbContext.Departamentos.First(d => d.Nombre == "Ciencias e ingeniería").Id,
        //                Nivel = 2,
        //            }
        //        };

        //        dbContext.Programas.AddRange(programas);
        //        dbContext.SaveChanges();
        //    }

        //    if (!dbContext.PlanEstudios.Any())
        //    {
        //        var planesEstudios = new List<PlanEstudios>()
        //        {
        //            new PlanEstudios
        //            {
        //                Nombre = "BACHILLERATO",
        //                Rvoe = "",
        //                PermiteAdelantar = 1,
        //                Version = "1",
        //                ProgramaId = dbContext.Programas.First(d => d.Nombre == "BACHILLERATO").Id,
        //                DuracionMeses = 15,
        //                Periodicidad = 1
        //            },
        //            new PlanEstudios
        //            {
        //                Nombre = "LICENCIATURA EN RADIOLOGÍA E IMAGEN",
        //                Rvoe = "",
        //                PermiteAdelantar = 1,
        //                Version = "1",
        //                ProgramaId = dbContext.Programas.First(d => d.Nombre == "LICENCIATURA EN RADIOLOGÍA E IMAGEN").Id,
        //                DuracionMeses = 15,
        //                Periodicidad = 1
        //            },
        //            new PlanEstudios
        //            {
        //                Nombre = "LICENCIATURA EN ENFERMERÍA",
        //                Rvoe = "",
        //                PermiteAdelantar = 1,
        //                Version = "1",
        //                ProgramaId = dbContext.Programas.First(d => d.Nombre == "LICENCIATURA EN ENFERMERÍA").Id,
        //                DuracionMeses = 15,
        //                Periodicidad = 1
        //            },
        //            new PlanEstudios
        //            {
        //                Nombre = "LICENCIATURA EN INGENIERÍA MECÁNICA Y ELECTRÓNICA AUTOMOTRIZ",
        //                Rvoe = "",
        //                PermiteAdelantar = 1,
        //                Version = "1",
        //                ProgramaId = dbContext.Programas.First(d => d.Nombre == "LICENCIATURA EN INGENIERÍA MECÁNICA Y ELECTRÓNICA AUTOMOTRIZ").Id,
        //                DuracionMeses = 15,
        //                Periodicidad = 1
        //            },
        //            new PlanEstudios
        //            {
        //                Nombre = "LICENCIATURA EN INGENIERÍA INDUSTRIAL",
        //                Rvoe = "",
        //                PermiteAdelantar = 1,
        //                Version = "1",
        //                ProgramaId = dbContext.Programas.First(d => d.Nombre == "LICENCIATURA EN INGENIERÍA INDUSTRIAL").Id,
        //                DuracionMeses = 15,
        //                Periodicidad = 1
        //            },
        //            new PlanEstudios
        //            {
        //                Nombre = "LICENCIATURA EN INGENIERÍA EN SOFTWARE Y SISTEMAS",
        //                Rvoe = "",
        //                PermiteAdelantar = 1,
        //                Version = "1",
        //                ProgramaId = dbContext.Programas.First(d => d.Nombre == "LICENCIATURA EN INGENIERÍA EN SOFTWARE Y SISTEMAS").Id,
        //                DuracionMeses = 15,
        //                Periodicidad = 1
        //            },
        //        };

        //        dbContext.PlanEstudios.AddRange(planesEstudios);
        //        dbContext.SaveChanges();
        //    }
        //}
    }
}
