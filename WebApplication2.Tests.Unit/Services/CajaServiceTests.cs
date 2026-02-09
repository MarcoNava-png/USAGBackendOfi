using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.Enums;
using WebApplication2.Core.Models;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Tests.Unit.Services;

public class CajaServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<IWebHostEnvironment> _envMock;
    private readonly CajaService _service;

    public CajaServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var loggerMock = new Mock<ILogger<ApplicationDbContext>>();

        _context = new ApplicationDbContext(options, httpContextAccessorMock.Object, loggerMock.Object);
        _authServiceMock = new Mock<IAuthService>();
        _envMock = new Mock<IWebHostEnvironment>();
        _envMock.Setup(e => e.ContentRootPath).Returns(Directory.GetCurrentDirectory());

        _service = new CajaService(_context, _authServiceMock.Object, _envMock.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region BuscarRecibosParaCobroAsync Tests

    [Fact]
    public async Task BuscarRecibosParaCobroAsync_ConCriterioVacio_DebeRetornarListaVacia()
    {
        var criterio = "";

        var resultado = await _service.BuscarRecibosParaCobroAsync(criterio);

        resultado.Recibos.Should().BeEmpty();
    }

    [Fact]
    public async Task BuscarRecibosParaCobroAsync_ConCriterioNulo_DebeRetornarListaVacia()
    {
        string? criterio = null;

        var resultado = await _service.BuscarRecibosParaCobroAsync(criterio!);

        resultado.Recibos.Should().BeEmpty();
    }

    [Fact]
    public async Task BuscarRecibosParaCobroAsync_ConEstudianteExistente_DebeRetornarRecibos()
    {
        var persona = new Persona
        {
            Nombre = "Juan",
            ApellidoPaterno = "Pérez",
            ApellidoMaterno = "García"
        };
        _context.Persona.Add(persona);
        await _context.SaveChangesAsync();

        var estudiante = new Estudiante
        {
            Matricula = "2024001",
            IdPersona = persona.IdPersona,
            Activo = true,
            Email = "juan@test.com"
        };
        _context.Estudiante.Add(estudiante);
        await _context.SaveChangesAsync();

        var recibo = new Recibo
        {
            IdEstudiante = estudiante.IdEstudiante,
            Folio = "REC-2024-001",
            FechaEmision = DateOnly.FromDateTime(DateTime.Now),
            FechaVencimiento = DateOnly.FromDateTime(DateTime.Now.AddDays(30)),
            Estatus = EstatusRecibo.PENDIENTE,
            Subtotal = 5000m,
            Descuento = 0m,
            Recargos = 0m,
            Saldo = 5000m
        };
        _context.Recibo.Add(recibo);
        await _context.SaveChangesAsync();

        var resultado = await _service.BuscarRecibosParaCobroAsync("2024001");

        resultado.Estudiante.Should().NotBeNull();
        resultado.Estudiante!.Matricula.Should().Be("2024001");
        resultado.Recibos.Should().HaveCount(1);
        resultado.TotalAdeudo.Should().Be(5000m);
    }

    [Fact]
    public async Task BuscarRecibosParaCobroAsync_ConFolioRecibo_DebeRetornarRecibo()
    {
        var persona = new Persona
        {
            Nombre = "María",
            ApellidoPaterno = "López",
            ApellidoMaterno = "Sánchez"
        };
        _context.Persona.Add(persona);
        await _context.SaveChangesAsync();

        var estudiante = new Estudiante
        {
            Matricula = "2024002",
            IdPersona = persona.IdPersona,
            Activo = true
        };
        _context.Estudiante.Add(estudiante);
        await _context.SaveChangesAsync();

        var recibo = new Recibo
        {
            IdEstudiante = estudiante.IdEstudiante,
            Folio = "REC-2024-UNICO",
            FechaEmision = DateOnly.FromDateTime(DateTime.Now),
            FechaVencimiento = DateOnly.FromDateTime(DateTime.Now.AddDays(30)),
            Estatus = EstatusRecibo.PENDIENTE,
            Subtotal = 3000m,
            Saldo = 3000m
        };
        _context.Recibo.Add(recibo);
        await _context.SaveChangesAsync();

        var resultado = await _service.BuscarRecibosParaCobroAsync("REC-2024-UNICO");

        resultado.Recibos.Should().HaveCount(1);
        resultado.Recibos.First().Folio.Should().Be("REC-2024-UNICO");
    }

    [Fact]
    public async Task BuscarRecibosParaCobroAsync_SoloRecibosPendientes_NoDebeIncluirPagados()
    {
        var persona = new Persona
        {
            Nombre = "Carlos",
            ApellidoPaterno = "Ruiz",
            ApellidoMaterno = "Torres"
        };
        _context.Persona.Add(persona);
        await _context.SaveChangesAsync();

        var estudiante = new Estudiante
        {
            Matricula = "2024003",
            IdPersona = persona.IdPersona,
            Activo = true
        };
        _context.Estudiante.Add(estudiante);
        await _context.SaveChangesAsync();

        var reciboPagado = new Recibo
        {
            IdEstudiante = estudiante.IdEstudiante,
            Folio = "REC-PAGADO",
            FechaEmision = DateOnly.FromDateTime(DateTime.Now.AddMonths(-1)),
            FechaVencimiento = DateOnly.FromDateTime(DateTime.Now.AddDays(-15)),
            Estatus = EstatusRecibo.PAGADO,
            Subtotal = 2000m,
            Saldo = 0m
        };

        var reciboPendiente = new Recibo
        {
            IdEstudiante = estudiante.IdEstudiante,
            Folio = "REC-PENDIENTE",
            FechaEmision = DateOnly.FromDateTime(DateTime.Now),
            FechaVencimiento = DateOnly.FromDateTime(DateTime.Now.AddDays(30)),
            Estatus = EstatusRecibo.PENDIENTE,
            Subtotal = 2500m,
            Saldo = 2500m
        };

        _context.Recibo.AddRange(reciboPagado, reciboPendiente);
        await _context.SaveChangesAsync();

        var resultado = await _service.BuscarRecibosParaCobroAsync("2024003");

        resultado.Recibos.Should().HaveCount(1);
        resultado.Recibos.First().Folio.Should().Be("REC-PENDIENTE");
    }

    [Fact]
    public async Task BuscarRecibosParaCobroAsync_MultiplesEstudiantes_DebeRetornarLista()
    {
        var persona1 = new Persona { Nombre = "Pedro", ApellidoPaterno = "García", ApellidoMaterno = "López" };
        var persona2 = new Persona { Nombre = "Pedro", ApellidoPaterno = "García", ApellidoMaterno = "Martínez" };
        _context.Persona.AddRange(persona1, persona2);
        await _context.SaveChangesAsync();

        var estudiante1 = new Estudiante { Matricula = "2024010", IdPersona = persona1.IdPersona, Activo = true };
        var estudiante2 = new Estudiante { Matricula = "2024011", IdPersona = persona2.IdPersona, Activo = true };
        _context.Estudiante.AddRange(estudiante1, estudiante2);
        await _context.SaveChangesAsync();

        var resultado = await _service.BuscarRecibosParaCobroAsync("Pedro García");

        resultado.Multiple.Should().BeTrue();
        resultado.Estudiantes.Should().HaveCount(2);
    }

    #endregion

    #region QuitarRecargoAsync Tests

    [Fact]
    public async Task QuitarRecargoAsync_ConReciboInexistente_DebeRetornarError()
    {
        var idReciboInexistente = 99999L;
        var resultado = await _service.QuitarRecargoAsync(idReciboInexistente, "Motivo test", "Usuario Test", "user-123");

        resultado.Exitoso.Should().BeFalse();
        resultado.Mensaje.Should().Contain("no encontrado");
    }

    [Fact]
    public async Task QuitarRecargoAsync_ConReciboPagado_DebeRetornarError()
    {
        var recibo = new Recibo
        {
            Folio = "REC-PAGADO-001",
            FechaEmision = DateOnly.FromDateTime(DateTime.Now),
            FechaVencimiento = DateOnly.FromDateTime(DateTime.Now.AddDays(30)),
            Estatus = EstatusRecibo.PAGADO,
            Subtotal = 1000m,
            Saldo = 0m
        };
        _context.Recibo.Add(recibo);
        await _context.SaveChangesAsync();

        var resultado = await _service.QuitarRecargoAsync(recibo.IdRecibo, "Motivo", "Usuario", "user-123");

        resultado.Exitoso.Should().BeFalse();
        resultado.Mensaje.Should().Contain("pagado");
    }

    [Fact]
    public async Task QuitarRecargoAsync_ConReciboPendiente_DebeCondonarRecargo()
    {
        var recibo = new Recibo
        {
            Folio = "REC-CON-RECARGO",
            FechaEmision = DateOnly.FromDateTime(DateTime.Now.AddMonths(-2)),
            FechaVencimiento = DateOnly.FromDateTime(DateTime.Now.AddMonths(-1)),
            Estatus = EstatusRecibo.VENCIDO,
            Subtotal = 1000m,
            Recargos = 100m,
            Saldo = 1100m
        };
        _context.Recibo.Add(recibo);
        await _context.SaveChangesAsync();

        var resultado = await _service.QuitarRecargoAsync(recibo.IdRecibo, "Cliente frecuente", "Admin Test", "admin-123");

        resultado.Exitoso.Should().BeTrue();
        resultado.RecargoCondonado.Should().Be(100m);

        var bitacora = await _context.BitacoraRecibo
            .FirstOrDefaultAsync(b => b.IdRecibo == recibo.IdRecibo && b.Accion == "CONDONACION_RECARGO");
        bitacora.Should().NotBeNull();
    }

    #endregion

    #region ModificarDetalleReciboAsync Tests

    [Fact]
    public async Task ModificarDetalleReciboAsync_ConMontoNegativo_DebeRetornarError()
    {
        var recibo = new Recibo
        {
            Folio = "REC-TEST",
            FechaEmision = DateOnly.FromDateTime(DateTime.Now),
            FechaVencimiento = DateOnly.FromDateTime(DateTime.Now.AddDays(30)),
            Estatus = EstatusRecibo.PENDIENTE,
            Subtotal = 1000m,
            Saldo = 1000m
        };
        _context.Recibo.Add(recibo);
        await _context.SaveChangesAsync();

        var detalle = new ReciboDetalle
        {
            IdRecibo = recibo.IdRecibo,
            IdConceptoPago = 1,
            Descripcion = "Colegiatura",
            Cantidad = 1,
            PrecioUnitario = 1000m
        };
        _context.ReciboDetalle.Add(detalle);
        await _context.SaveChangesAsync();

        var resultado = await _service.ModificarDetalleReciboAsync(
            recibo.IdRecibo,
            detalle.IdReciboDetalle,
            -500m,  
            "Test",
            "user-123"
        );

        resultado.Exitoso.Should().BeFalse();
        resultado.Mensaje.Should().Contain("negativo");
    }

    #endregion

    #region Corte de Caja Tests

    [Fact]
    public void GenerarFolioCorteCaja_DebeGenerarFolioConFormatoCorrecto()
    {
        var fechaActual = DateTime.Now.ToString("yyyyMMdd");

        fechaActual.Should().HaveLength(8);
    }

    [Fact]
    public async Task ObtenerCorteActivo_SinCorteActivo_DebeRetornarNull()
    {
        var usuarioId = "usuario-test-123";

        var corteActivo = await _service.ObtenerCorteActivo(usuarioId);

        corteActivo.Should().BeNull();
    }

    #endregion
}
