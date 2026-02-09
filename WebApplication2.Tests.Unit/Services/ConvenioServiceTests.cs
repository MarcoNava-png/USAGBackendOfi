using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.DTOs.Convenio;
using WebApplication2.Core.Enums;
using WebApplication2.Core.Models;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services;

namespace WebApplication2.Tests.Unit.Services;

public class ConvenioServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly ConvenioService _service;

    public ConvenioServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var loggerMock = new Mock<ILogger<ApplicationDbContext>>();

        _context = new ApplicationDbContext(options, httpContextAccessorMock.Object, loggerMock.Object);
        _service = new ConvenioService(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task ObtenerPorIdAsync_ConConvenioExistente_DebeRetornarConvenio()
    {
        var convenio = new Convenio
        {
            ClaveConvenio = "CONV-001",
            Nombre = "Convenio Empresa Test",
            TipoBeneficio = "PORCENTAJE",
            DescuentoPct = 20m,
            Activo = true,
            Status = StatusEnum.Active
        };
        _context.Convenio.Add(convenio);
        await _context.SaveChangesAsync();

        var resultado = await _service.ObtenerPorIdAsync(convenio.IdConvenio, CancellationToken.None);

        resultado.Should().NotBeNull();
        resultado!.ClaveConvenio.Should().Be("CONV-001");
        resultado.Nombre.Should().Be("Convenio Empresa Test");
        resultado.TipoBeneficio.Should().Be("PORCENTAJE");
    }

    [Fact]
    public async Task ObtenerPorIdAsync_ConConvenioInexistente_DebeRetornarNull()
    {
        var idInexistente = 99999;

        var resultado = await _service.ObtenerPorIdAsync(idInexistente, CancellationToken.None);

        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerPorIdAsync_ConConvenioEliminado_DebeRetornarNull()
    {
        var convenio = new Convenio
        {
            ClaveConvenio = "CONV-DELETED",
            Nombre = "Convenio Eliminado",
            TipoBeneficio = "PORCENTAJE",
            DescuentoPct = 10m,
            Status = StatusEnum.Deleted
        };
        _context.Convenio.Add(convenio);
        await _context.SaveChangesAsync();

        var resultado = await _service.ObtenerPorIdAsync(convenio.IdConvenio, CancellationToken.None);

        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ListarConveniosAsync_SinFiltros_DebeRetornarTodosLosActivos()
    {
        var convenio1 = new Convenio
        {
            ClaveConvenio = "CONV-A",
            Nombre = "Convenio A",
            TipoBeneficio = "PORCENTAJE",
            DescuentoPct = 15m,
            Activo = true,
            Status = StatusEnum.Active
        };
        var convenio2 = new Convenio
        {
            ClaveConvenio = "CONV-B",
            Nombre = "Convenio B",
            TipoBeneficio = "MONTO",
            Monto = 500m,
            Activo = false,
            Status = StatusEnum.Active
        };
        _context.Convenio.AddRange(convenio1, convenio2);
        await _context.SaveChangesAsync();

        var resultado = await _service.ListarConveniosAsync(ct: CancellationToken.None);

        resultado.Should().HaveCount(2);
    }

    [Fact]
    public async Task ListarConveniosAsync_FiltroSoloActivos_DebeRetornarSoloActivos()
    {
        var convenioActivo = new Convenio
        {
            ClaveConvenio = "CONV-ACTIVO",
            Nombre = "Convenio Activo",
            TipoBeneficio = "PORCENTAJE",
            DescuentoPct = 20m,
            Activo = true,
            Status = StatusEnum.Active
        };
        var convenioInactivo = new Convenio
        {
            ClaveConvenio = "CONV-INACTIVO",
            Nombre = "Convenio Inactivo",
            TipoBeneficio = "MONTO",
            Monto = 1000m,
            Activo = false,
            Status = StatusEnum.Active
        };
        _context.Convenio.AddRange(convenioActivo, convenioInactivo);
        await _context.SaveChangesAsync();

        var resultado = await _service.ListarConveniosAsync(soloActivos: true, ct: CancellationToken.None);

        resultado.Should().HaveCount(1);
        resultado.First().ClaveConvenio.Should().Be("CONV-ACTIVO");
    }

    [Fact]
    public async Task CrearConvenioAsync_ConDatosValidos_DebeCrearConvenio()
    {
        var dto = new CrearConvenioDto
        {
            ClaveConvenio = "NUEVO-CONV",
            Nombre = "Nuevo Convenio Test",
            TipoBeneficio = "PORCENTAJE",
            DescuentoPct = 25m,
            Activo = true,
            Alcances = new List<CrearConvenioAlcanceDto>()
        };

        var resultado = await _service.CrearConvenioAsync(dto, "admin-test", CancellationToken.None);

        resultado.Should().NotBeNull();
        resultado.ClaveConvenio.Should().Be("NUEVO-CONV");
        resultado.Nombre.Should().Be("Nuevo Convenio Test");
        resultado.DescuentoPct.Should().Be(25m);

        var convenioEnBd = await _context.Convenio.FirstOrDefaultAsync(c => c.ClaveConvenio == "NUEVO-CONV");
        convenioEnBd.Should().NotBeNull();
    }

    [Fact]
    public async Task CrearConvenioAsync_ConClaveDuplicada_DebeLanzarExcepcion()
    {
        var convenioExistente = new Convenio
        {
            ClaveConvenio = "CLAVE-DUPLICADA",
            Nombre = "Convenio Existente",
            TipoBeneficio = "PORCENTAJE",
            DescuentoPct = 10m,
            Status = StatusEnum.Active
        };
        _context.Convenio.Add(convenioExistente);
        await _context.SaveChangesAsync();

        var dto = new CrearConvenioDto
        {
            ClaveConvenio = "CLAVE-DUPLICADA",
            Nombre = "Nuevo Convenio",
            TipoBeneficio = "PORCENTAJE",
            DescuentoPct = 15m,
            Alcances = new List<CrearConvenioAlcanceDto>()
        };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CrearConvenioAsync(dto, "admin", CancellationToken.None)
        );
    }

    [Fact]
    public async Task CrearConvenioAsync_ConTipoBeneficioInvalido_DebeLanzarExcepcion()
    {
        var dto = new CrearConvenioDto
        {
            ClaveConvenio = "CONV-INVALIDO",
            Nombre = "Convenio Invalido",
            TipoBeneficio = "INVALIDO",
            DescuentoPct = 10m,
            Alcances = new List<CrearConvenioAlcanceDto>()
        };

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CrearConvenioAsync(dto, "admin", CancellationToken.None)
        );
    }

    [Fact]
    public async Task CrearConvenioAsync_TipoPorcentajeSinValor_DebeLanzarExcepcion()
    {
        var dto = new CrearConvenioDto
        {
            ClaveConvenio = "CONV-SIN-PCT",
            Nombre = "Convenio Sin Porcentaje",
            TipoBeneficio = "PORCENTAJE",
            DescuentoPct = null,
            Alcances = new List<CrearConvenioAlcanceDto>()
        };

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CrearConvenioAsync(dto, "admin", CancellationToken.None)
        );
    }

    [Fact]
    public async Task CrearConvenioAsync_TipoMontoSinValor_DebeLanzarExcepcion()
    {
        var dto = new CrearConvenioDto
        {
            ClaveConvenio = "CONV-SIN-MONTO",
            Nombre = "Convenio Sin Monto",
            TipoBeneficio = "MONTO",
            Monto = null,
            Alcances = new List<CrearConvenioAlcanceDto>()
        };

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CrearConvenioAsync(dto, "admin", CancellationToken.None)
        );
    }

    [Fact]
    public async Task EliminarConvenioAsync_ConConvenioSinAspirantes_DebeEliminar()
    {
        var convenio = new Convenio
        {
            ClaveConvenio = "CONV-ELIMINAR",
            Nombre = "Convenio para Eliminar",
            TipoBeneficio = "PORCENTAJE",
            DescuentoPct = 10m,
            Activo = true,
            Status = StatusEnum.Active
        };
        _context.Convenio.Add(convenio);
        await _context.SaveChangesAsync();

        var resultado = await _service.EliminarConvenioAsync(convenio.IdConvenio, CancellationToken.None);

        resultado.Should().BeTrue();

        var convenioEliminado = await _context.Convenio.FindAsync(convenio.IdConvenio);
        convenioEliminado!.Status.Should().Be(StatusEnum.Deleted);
        convenioEliminado.Activo.Should().BeFalse();
    }

    [Fact]
    public async Task EliminarConvenioAsync_ConAspirantesAprobados_DebeLanzarExcepcion()
    {
        var convenio = new Convenio
        {
            ClaveConvenio = "CONV-CON-ASP",
            Nombre = "Convenio con Aspirantes",
            TipoBeneficio = "PORCENTAJE",
            DescuentoPct = 15m,
            Activo = true,
            Status = StatusEnum.Active
        };
        _context.Convenio.Add(convenio);
        await _context.SaveChangesAsync();

        var persona = new Persona { Nombre = "Test", ApellidoPaterno = "Usuario" };
        _context.Persona.Add(persona);
        await _context.SaveChangesAsync();

        var aspirante = new Aspirante { IdPersona = persona.IdPersona, Status = StatusEnum.Active };
        _context.Aspirante.Add(aspirante);
        await _context.SaveChangesAsync();

        var asignacion = new AspiranteConvenio
        {
            IdAspirante = aspirante.IdAspirante,
            IdConvenio = convenio.IdConvenio,
            Estatus = "Aprobado"
        };
        _context.AspiranteConvenio.Add(asignacion);
        await _context.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.EliminarConvenioAsync(convenio.IdConvenio, CancellationToken.None)
        );
    }

    [Fact]
    public async Task EliminarConvenioAsync_ConConvenioInexistente_DebeRetornarFalse()
    {
        var idInexistente = 99999;

        var resultado = await _service.EliminarConvenioAsync(idInexistente, CancellationToken.None);

        resultado.Should().BeFalse();
    }

    [Fact]
    public async Task CambiarEstadoConvenioAsync_ActivarConvenio_DebeActivar()
    {
        var convenio = new Convenio
        {
            ClaveConvenio = "CONV-INACTIVO",
            Nombre = "Convenio Inactivo",
            TipoBeneficio = "PORCENTAJE",
            DescuentoPct = 10m,
            Activo = false,
            Status = StatusEnum.Active
        };
        _context.Convenio.Add(convenio);
        await _context.SaveChangesAsync();

        var resultado = await _service.CambiarEstadoConvenioAsync(convenio.IdConvenio, true, CancellationToken.None);

        resultado.Should().BeTrue();

        var convenioActualizado = await _context.Convenio.FindAsync(convenio.IdConvenio);
        convenioActualizado!.Activo.Should().BeTrue();
    }

    [Fact]
    public async Task CambiarEstadoConvenioAsync_ConvenioInexistente_DebeRetornarFalse()
    {
        var idInexistente = 99999;

        var resultado = await _service.CambiarEstadoConvenioAsync(idInexistente, true, CancellationToken.None);

        resultado.Should().BeFalse();
    }

    [Fact]
    public async Task AsignarConvenioAspiranteAsync_ConDatosValidos_DebeCrearAsignacion()
    {
        var persona = new Persona { Nombre = "Maria", ApellidoPaterno = "Garcia", ApellidoMaterno = "Lopez" };
        _context.Persona.Add(persona);
        await _context.SaveChangesAsync();

        var aspirante = new Aspirante { IdPersona = persona.IdPersona, Status = StatusEnum.Active };
        _context.Aspirante.Add(aspirante);
        await _context.SaveChangesAsync();

        var convenio = new Convenio
        {
            ClaveConvenio = "CONV-ASIGNAR",
            Nombre = "Convenio para Asignar",
            TipoBeneficio = "PORCENTAJE",
            DescuentoPct = 20m,
            Activo = true,
            Status = StatusEnum.Active
        };
        _context.Convenio.Add(convenio);
        await _context.SaveChangesAsync();

        var dto = new AsignarConvenioAspiranteDto
        {
            IdAspirante = aspirante.IdAspirante,
            IdConvenio = convenio.IdConvenio,
            Evidencia = "documento_evidencia.pdf"
        };

        var resultado = await _service.AsignarConvenioAspiranteAsync(dto, "admin-test", CancellationToken.None);

        resultado.Should().NotBeNull();
        resultado.IdAspirante.Should().Be(aspirante.IdAspirante);
        resultado.IdConvenio.Should().Be(convenio.IdConvenio);
        resultado.Estatus.Should().Be("Pendiente");
        resultado.Evidencia.Should().Be("documento_evidencia.pdf");
    }

    [Fact]
    public async Task AsignarConvenioAspiranteAsync_ConAspiranteInexistente_DebeLanzarExcepcion()
    {
        var convenio = new Convenio
        {
            ClaveConvenio = "CONV-TEST",
            Nombre = "Convenio Test",
            TipoBeneficio = "PORCENTAJE",
            DescuentoPct = 10m,
            Activo = true,
            Status = StatusEnum.Active
        };
        _context.Convenio.Add(convenio);
        await _context.SaveChangesAsync();

        var dto = new AsignarConvenioAspiranteDto
        {
            IdAspirante = 99999,
            IdConvenio = convenio.IdConvenio
        };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.AsignarConvenioAspiranteAsync(dto, "admin", CancellationToken.None)
        );
    }

    [Fact]
    public async Task AsignarConvenioAspiranteAsync_ConConvenioYaAsignado_DebeLanzarExcepcion()
    {
        var persona = new Persona { Nombre = "Test", ApellidoPaterno = "User" };
        _context.Persona.Add(persona);
        await _context.SaveChangesAsync();

        var aspirante = new Aspirante { IdPersona = persona.IdPersona, Status = StatusEnum.Active };
        _context.Aspirante.Add(aspirante);
        await _context.SaveChangesAsync();

        var convenio = new Convenio
        {
            ClaveConvenio = "CONV-DUPLICADO",
            Nombre = "Convenio Duplicado",
            TipoBeneficio = "PORCENTAJE",
            DescuentoPct = 15m,
            Activo = true,
            Status = StatusEnum.Active
        };
        _context.Convenio.Add(convenio);
        await _context.SaveChangesAsync();

        var asignacionExistente = new AspiranteConvenio
        {
            IdAspirante = aspirante.IdAspirante,
            IdConvenio = convenio.IdConvenio,
            Estatus = "Pendiente"
        };
        _context.AspiranteConvenio.Add(asignacionExistente);
        await _context.SaveChangesAsync();

        var dto = new AsignarConvenioAspiranteDto
        {
            IdAspirante = aspirante.IdAspirante,
            IdConvenio = convenio.IdConvenio
        };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.AsignarConvenioAspiranteAsync(dto, "admin", CancellationToken.None)
        );
    }

    [Fact]
    public async Task CambiarEstatusConvenioAspiranteAsync_AprobarConvenio_DebeActualizar()
    {
        var persona = new Persona { Nombre = "Test", ApellidoPaterno = "User" };
        _context.Persona.Add(persona);
        await _context.SaveChangesAsync();

        var aspirante = new Aspirante { IdPersona = persona.IdPersona, Status = StatusEnum.Active };
        _context.Aspirante.Add(aspirante);
        await _context.SaveChangesAsync();

        var convenio = new Convenio
        {
            ClaveConvenio = "CONV-APROBAR",
            Nombre = "Convenio Aprobar",
            TipoBeneficio = "PORCENTAJE",
            DescuentoPct = 20m,
            Activo = true,
            Status = StatusEnum.Active
        };
        _context.Convenio.Add(convenio);
        await _context.SaveChangesAsync();

        var asignacion = new AspiranteConvenio
        {
            IdAspirante = aspirante.IdAspirante,
            IdConvenio = convenio.IdConvenio,
            Estatus = "Pendiente"
        };
        _context.AspiranteConvenio.Add(asignacion);
        await _context.SaveChangesAsync();

        var resultado = await _service.CambiarEstatusConvenioAspiranteAsync(
            asignacion.IdAspiranteConvenio, "Aprobado", CancellationToken.None);

        resultado.Should().BeTrue();

        var asignacionActualizada = await _context.AspiranteConvenio.FindAsync(asignacion.IdAspiranteConvenio);
        asignacionActualizada!.Estatus.Should().Be("Aprobado");
    }

    [Fact]
    public async Task CambiarEstatusConvenioAspiranteAsync_ConEstatusInvalido_DebeLanzarExcepcion()
    {
        var persona = new Persona { Nombre = "Test", ApellidoPaterno = "User" };
        _context.Persona.Add(persona);
        await _context.SaveChangesAsync();

        var aspirante = new Aspirante { IdPersona = persona.IdPersona, Status = StatusEnum.Active };
        _context.Aspirante.Add(aspirante);
        await _context.SaveChangesAsync();

        var convenio = new Convenio
        {
            ClaveConvenio = "CONV-ESTATUS",
            Nombre = "Convenio Estatus",
            TipoBeneficio = "PORCENTAJE",
            DescuentoPct = 10m,
            Activo = true,
            Status = StatusEnum.Active
        };
        _context.Convenio.Add(convenio);
        await _context.SaveChangesAsync();

        var asignacion = new AspiranteConvenio
        {
            IdAspirante = aspirante.IdAspirante,
            IdConvenio = convenio.IdConvenio,
            Estatus = "Pendiente"
        };
        _context.AspiranteConvenio.Add(asignacion);
        await _context.SaveChangesAsync();

        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CambiarEstatusConvenioAspiranteAsync(
                asignacion.IdAspiranteConvenio, "INVALIDO", CancellationToken.None)
        );
    }

    [Fact]
    public async Task CalcularDescuentoConvenioAsync_TipoPorcentaje_DebeCalcularCorrectamente()
    {
        var convenio = new Convenio
        {
            ClaveConvenio = "CONV-PCT",
            Nombre = "Convenio Porcentaje",
            TipoBeneficio = "PORCENTAJE",
            DescuentoPct = 20m,
            Activo = true,
            Status = StatusEnum.Active
        };
        _context.Convenio.Add(convenio);
        await _context.SaveChangesAsync();

        var resultado = await _service.CalcularDescuentoConvenioAsync(convenio.IdConvenio, 1000m, CancellationToken.None);

        resultado.MontoOriginal.Should().Be(1000m);
        resultado.Descuento.Should().Be(200m);
        resultado.MontoFinal.Should().Be(800m);
    }

    [Fact]
    public async Task CalcularDescuentoConvenioAsync_TipoMonto_DebeCalcularCorrectamente()
    {
        var convenio = new Convenio
        {
            ClaveConvenio = "CONV-MONTO",
            Nombre = "Convenio Monto Fijo",
            TipoBeneficio = "MONTO",
            Monto = 300m,
            Activo = true,
            Status = StatusEnum.Active
        };
        _context.Convenio.Add(convenio);
        await _context.SaveChangesAsync();

        var resultado = await _service.CalcularDescuentoConvenioAsync(convenio.IdConvenio, 1000m, CancellationToken.None);

        resultado.MontoOriginal.Should().Be(1000m);
        resultado.Descuento.Should().Be(300m);
        resultado.MontoFinal.Should().Be(700m);
    }

    [Fact]
    public async Task CalcularDescuentoConvenioAsync_TipoExencion_DebeDescontarTodo()
    {
        var convenio = new Convenio
        {
            ClaveConvenio = "CONV-EXENCION",
            Nombre = "Convenio Exencion Total",
            TipoBeneficio = "EXENCION",
            Activo = true,
            Status = StatusEnum.Active
        };
        _context.Convenio.Add(convenio);
        await _context.SaveChangesAsync();

        var resultado = await _service.CalcularDescuentoConvenioAsync(convenio.IdConvenio, 1500m, CancellationToken.None);

        resultado.MontoOriginal.Should().Be(1500m);
        resultado.Descuento.Should().Be(1500m);
        resultado.MontoFinal.Should().Be(0m);
    }

    [Fact]
    public async Task CalcularDescuentoConvenioAsync_ConConvenioInexistente_DebeLanzarExcepcion()
    {
        var idInexistente = 99999;

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CalcularDescuentoConvenioAsync(idInexistente, 1000m, CancellationToken.None)
        );
    }

    [Fact]
    public async Task CalcularDescuentoConvenioAsync_MontoMayorQueMonto_NoDebeExcederMonto()
    {
        var convenio = new Convenio
        {
            ClaveConvenio = "CONV-GRANDE",
            Nombre = "Convenio Grande",
            TipoBeneficio = "MONTO",
            Monto = 5000m,
            Activo = true,
            Status = StatusEnum.Active
        };
        _context.Convenio.Add(convenio);
        await _context.SaveChangesAsync();

        var resultado = await _service.CalcularDescuentoConvenioAsync(convenio.IdConvenio, 1000m, CancellationToken.None);

        resultado.Descuento.Should().Be(1000m);
        resultado.MontoFinal.Should().Be(0m);
    }

    [Fact]
    public async Task CalcularDescuentoTotalAspiranteAsync_ConConveniosAprobados_DebeCalcularTotal()
    {
        var persona = new Persona { Nombre = "Test", ApellidoPaterno = "User" };
        _context.Persona.Add(persona);
        await _context.SaveChangesAsync();

        var aspirante = new Aspirante { IdPersona = persona.IdPersona, Status = StatusEnum.Active };
        _context.Aspirante.Add(aspirante);
        await _context.SaveChangesAsync();

        var convenio = new Convenio
        {
            ClaveConvenio = "CONV-TOTAL",
            Nombre = "Convenio Total",
            TipoBeneficio = "PORCENTAJE",
            DescuentoPct = 15m,
            AplicaA = "TODOS",
            Activo = true,
            Status = StatusEnum.Active
        };
        _context.Convenio.Add(convenio);
        await _context.SaveChangesAsync();

        var asignacion = new AspiranteConvenio
        {
            IdAspirante = aspirante.IdAspirante,
            IdConvenio = convenio.IdConvenio,
            IdConvenioNavigation = convenio,
            Estatus = "Aprobado",
            Status = StatusEnum.Active,
            VecesAplicado = 0
        };
        _context.AspiranteConvenio.Add(asignacion);
        await _context.SaveChangesAsync();

        var descuento = await _service.CalcularDescuentoTotalAspiranteAsync(
            aspirante.IdAspirante, 1000m, null, CancellationToken.None);

        descuento.Should().Be(150m);
    }

    [Fact]
    public async Task CalcularDescuentoTotalAspiranteAsync_SinConveniosAprobados_DebeRetornarCero()
    {
        var persona = new Persona { Nombre = "Test", ApellidoPaterno = "User" };
        _context.Persona.Add(persona);
        await _context.SaveChangesAsync();

        var aspirante = new Aspirante { IdPersona = persona.IdPersona, Status = StatusEnum.Active };
        _context.Aspirante.Add(aspirante);
        await _context.SaveChangesAsync();

        var descuento = await _service.CalcularDescuentoTotalAspiranteAsync(
            aspirante.IdAspirante, 1000m, null, CancellationToken.None);

        descuento.Should().Be(0m);
    }

    [Fact]
    public async Task CalcularDescuentoTotalAspiranteAsync_ConvenioPendiente_NoDebeAplicar()
    {
        var persona = new Persona { Nombre = "Test", ApellidoPaterno = "User" };
        _context.Persona.Add(persona);
        await _context.SaveChangesAsync();

        var aspirante = new Aspirante { IdPersona = persona.IdPersona, Status = StatusEnum.Active };
        _context.Aspirante.Add(aspirante);
        await _context.SaveChangesAsync();

        var convenio = new Convenio
        {
            ClaveConvenio = "CONV-PEND",
            Nombre = "Convenio Pendiente",
            TipoBeneficio = "PORCENTAJE",
            DescuentoPct = 30m,
            Activo = true,
            Status = StatusEnum.Active
        };
        _context.Convenio.Add(convenio);
        await _context.SaveChangesAsync();

        var asignacion = new AspiranteConvenio
        {
            IdAspirante = aspirante.IdAspirante,
            IdConvenio = convenio.IdConvenio,
            IdConvenioNavigation = convenio,
            Estatus = "Pendiente",
            Status = StatusEnum.Active
        };
        _context.AspiranteConvenio.Add(asignacion);
        await _context.SaveChangesAsync();

        var descuento = await _service.CalcularDescuentoTotalAspiranteAsync(
            aspirante.IdAspirante, 1000m, null, CancellationToken.None);

        descuento.Should().Be(0m);
    }

    [Fact]
    public async Task CalcularDescuentoTotalAspiranteAsync_ConLimiteAplicaciones_NoDebeExcederLimite()
    {
        var persona = new Persona { Nombre = "Test", ApellidoPaterno = "User" };
        _context.Persona.Add(persona);
        await _context.SaveChangesAsync();

        var aspirante = new Aspirante { IdPersona = persona.IdPersona, Status = StatusEnum.Active };
        _context.Aspirante.Add(aspirante);
        await _context.SaveChangesAsync();

        var convenio = new Convenio
        {
            ClaveConvenio = "CONV-LIMITE",
            Nombre = "Convenio con Limite",
            TipoBeneficio = "PORCENTAJE",
            DescuentoPct = 20m,
            MaxAplicaciones = 2,
            Activo = true,
            Status = StatusEnum.Active
        };
        _context.Convenio.Add(convenio);
        await _context.SaveChangesAsync();

        var asignacion = new AspiranteConvenio
        {
            IdAspirante = aspirante.IdAspirante,
            IdConvenio = convenio.IdConvenio,
            IdConvenioNavigation = convenio,
            Estatus = "Aprobado",
            Status = StatusEnum.Active,
            VecesAplicado = 2
        };
        _context.AspiranteConvenio.Add(asignacion);
        await _context.SaveChangesAsync();

        var descuento = await _service.CalcularDescuentoTotalAspiranteAsync(
            aspirante.IdAspirante, 1000m, null, CancellationToken.None);

        descuento.Should().Be(0m);
    }

    [Fact]
    public async Task CalcularDescuentoTotalAspiranteAsync_FiltroTipoConcepto_SoloAplicaAlTipo()
    {
        var persona = new Persona { Nombre = "Test", ApellidoPaterno = "User" };
        _context.Persona.Add(persona);
        await _context.SaveChangesAsync();

        var aspirante = new Aspirante { IdPersona = persona.IdPersona, Status = StatusEnum.Active };
        _context.Aspirante.Add(aspirante);
        await _context.SaveChangesAsync();

        var convenioInscripcion = new Convenio
        {
            ClaveConvenio = "CONV-INSC",
            Nombre = "Convenio Inscripcion",
            TipoBeneficio = "PORCENTAJE",
            DescuentoPct = 25m,
            AplicaA = "INSCRIPCION",
            Activo = true,
            Status = StatusEnum.Active
        };
        _context.Convenio.Add(convenioInscripcion);
        await _context.SaveChangesAsync();

        var asignacion = new AspiranteConvenio
        {
            IdAspirante = aspirante.IdAspirante,
            IdConvenio = convenioInscripcion.IdConvenio,
            IdConvenioNavigation = convenioInscripcion,
            Estatus = "Aprobado",
            Status = StatusEnum.Active,
            VecesAplicado = 0
        };
        _context.AspiranteConvenio.Add(asignacion);
        await _context.SaveChangesAsync();

        var descuentoColegiatura = await _service.CalcularDescuentoTotalAspiranteAsync(
            aspirante.IdAspirante, 1000m, "COLEGIATURA", CancellationToken.None);

        var descuentoInscripcion = await _service.CalcularDescuentoTotalAspiranteAsync(
            aspirante.IdAspirante, 1000m, "INSCRIPCION", CancellationToken.None);

        descuentoColegiatura.Should().Be(0m);
        descuentoInscripcion.Should().Be(250m);
    }

    [Theory]
    [InlineData("PORCENTAJE", "10", null, "1000", "100")]
    [InlineData("PORCENTAJE", "50", null, "1000", "500")]
    [InlineData("PORCENTAJE", "100", null, "1000", "1000")]
    [InlineData("MONTO", null, "200", "1000", "200")]
    [InlineData("MONTO", null, "1500", "1000", "1000")]
    public async Task CalcularDescuento_DiferentesTipos_DebeCalcularCorrectamente(
        string tipoBeneficio, string? porcentajeStr, string? montoStr, string montoOriginalStr, string descuentoEsperadoStr)
    {
        decimal? porcentaje = porcentajeStr != null ? decimal.Parse(porcentajeStr) : null;
        decimal? monto = montoStr != null ? decimal.Parse(montoStr) : null;
        decimal montoOriginal = decimal.Parse(montoOriginalStr);
        decimal descuentoEsperado = decimal.Parse(descuentoEsperadoStr);

        var convenio = new Convenio
        {
            ClaveConvenio = $"CONV-{Guid.NewGuid():N}",
            Nombre = "Convenio Test",
            TipoBeneficio = tipoBeneficio,
            DescuentoPct = porcentaje,
            Monto = monto,
            Activo = true,
            Status = StatusEnum.Active
        };
        _context.Convenio.Add(convenio);
        await _context.SaveChangesAsync();

        var resultado = await _service.CalcularDescuentoConvenioAsync(convenio.IdConvenio, montoOriginal, CancellationToken.None);

        resultado.Descuento.Should().Be(descuentoEsperado);
    }
}
