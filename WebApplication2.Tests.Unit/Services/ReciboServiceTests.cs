using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using AutoMapper;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.Enums;
using WebApplication2.Core.Models;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Tests.Unit.Services;

public class ReciboServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IConvenioService> _convenioServiceMock;
    private readonly Mock<IBecaService> _becaServiceMock;
    private readonly ReciboService _service;

    public ReciboServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var loggerMock = new Mock<ILogger<ApplicationDbContext>>();

        _context = new ApplicationDbContext(options, httpContextAccessorMock.Object, loggerMock.Object);
        _mapperMock = new Mock<IMapper>();
        _convenioServiceMock = new Mock<IConvenioService>();
        _becaServiceMock = new Mock<IBecaService>();

        _service = new ReciboService(_context, _mapperMock.Object, _convenioServiceMock.Object, _becaServiceMock.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task ObtenerAsync_ConReciboExistente_DebeRetornarRecibo()
    {
        var recibo = new Recibo
        {
            Folio = "REC-2024-000001",
            FechaEmision = DateOnly.FromDateTime(DateTime.Now),
            FechaVencimiento = DateOnly.FromDateTime(DateTime.Now.AddDays(30)),
            Estatus = EstatusRecibo.PENDIENTE,
            Subtotal = 1000m,
            Saldo = 1000m
        };
        _context.Recibo.Add(recibo);
        await _context.SaveChangesAsync();

        var reciboDto = new ReciboDto { IdRecibo = recibo.IdRecibo, Folio = recibo.Folio };
        _mapperMock.Setup(m => m.Map<ReciboDto>(It.IsAny<Recibo>())).Returns(reciboDto);

        var resultado = await _service.ObtenerAsync(recibo.IdRecibo, CancellationToken.None);

        resultado.Should().NotBeNull();
        resultado!.Folio.Should().Be("REC-2024-000001");
    }

    [Fact]
    public async Task ObtenerAsync_ConReciboInexistente_DebeRetornarNull()
    {
        var idInexistente = 99999L;

        var resultado = await _service.ObtenerAsync(idInexistente, CancellationToken.None);

        resultado.Should().BeNull();
    }

    [Fact]
    public async Task GenerarReciboAspiranteAsync_ConAspiranteInexistente_DebeLanzarExcepcion()
    {
        var idAspiranteInexistente = 99999;

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.GenerarReciboAspiranteAsync(idAspiranteInexistente, 1000m, "Inscripcion", 30, CancellationToken.None)
        );
    }

    [Fact]
    public async Task GenerarReciboAspiranteAsync_ConAspiranteValido_DebeCrearRecibo()
    {
        var persona = new Persona
        {
            Nombre = "Juan",
            ApellidoPaterno = "Perez",
            ApellidoMaterno = "Lopez"
        };
        _context.Persona.Add(persona);
        await _context.SaveChangesAsync();

        var aspirante = new Aspirante
        {
            IdPersona = persona.IdPersona,
            Status = StatusEnum.Active
        };
        _context.Aspirante.Add(aspirante);
        await _context.SaveChangesAsync();

        _convenioServiceMock
            .Setup(c => c.CalcularDescuentoTotalAspiranteAsync(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0m);

        var reciboDto = new ReciboDto { IdRecibo = 1, Folio = "REC-2024-000001" };
        _mapperMock.Setup(m => m.Map<ReciboDto>(It.IsAny<Recibo>())).Returns(reciboDto);

        var resultado = await _service.GenerarReciboAspiranteAsync(
            aspirante.IdAspirante,
            1500m,
            "Cuota de Inscripcion",
            30,
            CancellationToken.None
        );

        resultado.Should().NotBeNull();

        var reciboCreado = await _context.Recibo
            .Include(r => r.Detalles)
            .FirstOrDefaultAsync(r => r.IdAspirante == aspirante.IdAspirante);

        reciboCreado.Should().NotBeNull();
        reciboCreado!.Subtotal.Should().Be(1500m);
        reciboCreado.Saldo.Should().Be(1500m);
        reciboCreado.Detalles.Should().HaveCount(1);
    }

    [Fact]
    public async Task GenerarReciboAspiranteAsync_ConConvenioAplicado_DebeAplicarDescuento()
    {
        var persona = new Persona
        {
            Nombre = "Maria",
            ApellidoPaterno = "Garcia",
            ApellidoMaterno = "Lopez"
        };
        _context.Persona.Add(persona);
        await _context.SaveChangesAsync();

        var aspirante = new Aspirante
        {
            IdPersona = persona.IdPersona,
            Status = StatusEnum.Active
        };
        _context.Aspirante.Add(aspirante);
        await _context.SaveChangesAsync();

        _convenioServiceMock
            .Setup(c => c.CalcularDescuentoTotalAspiranteAsync(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(200m);

        var reciboDto = new ReciboDto { IdRecibo = 1, Folio = "REC-2024-000002" };
        _mapperMock.Setup(m => m.Map<ReciboDto>(It.IsAny<Recibo>())).Returns(reciboDto);

        var resultado = await _service.GenerarReciboAspiranteAsync(
            aspirante.IdAspirante,
            1000m,
            "Inscripcion",
            30,
            CancellationToken.None
        );

        var reciboCreado = await _context.Recibo
            .FirstOrDefaultAsync(r => r.IdAspirante == aspirante.IdAspirante);

        reciboCreado.Should().NotBeNull();
        reciboCreado!.Subtotal.Should().Be(1000m);
        reciboCreado.Descuento.Should().Be(200m);
        reciboCreado.Saldo.Should().Be(800m);
    }

    [Fact]
    public async Task EliminarReciboAsync_ConReciboInexistente_DebeLanzarExcepcion()
    {
        var idInexistente = 99999L;

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.EliminarReciboAsync(idInexistente, CancellationToken.None)
        );
    }

    [Fact]
    public async Task EliminarReciboAsync_ConReciboSinPagos_DebeEliminar()
    {
        var recibo = new Recibo
        {
            Folio = "REC-ELIMINAR-001",
            FechaEmision = DateOnly.FromDateTime(DateTime.Now),
            FechaVencimiento = DateOnly.FromDateTime(DateTime.Now.AddDays(30)),
            Estatus = EstatusRecibo.PENDIENTE,
            Subtotal = 500m,
            Saldo = 500m
        };
        _context.Recibo.Add(recibo);
        await _context.SaveChangesAsync();

        var detalle = new ReciboDetalle
        {
            IdRecibo = recibo.IdRecibo,
            IdConceptoPago = 1,
            Descripcion = "Concepto test",
            Cantidad = 1,
            PrecioUnitario = 500m
        };
        _context.ReciboDetalle.Add(detalle);
        await _context.SaveChangesAsync();

        var resultado = await _service.EliminarReciboAsync(recibo.IdRecibo, CancellationToken.None);

        resultado.Should().BeTrue();

        var reciboEliminado = await _context.Recibo.FindAsync(recibo.IdRecibo);
        reciboEliminado.Should().BeNull();
    }

    [Fact]
    public async Task EliminarReciboAsync_ConPagosAplicados_DebeLanzarExcepcion()
    {
        var recibo = new Recibo
        {
            Folio = "REC-CON-PAGO",
            FechaEmision = DateOnly.FromDateTime(DateTime.Now),
            FechaVencimiento = DateOnly.FromDateTime(DateTime.Now.AddDays(30)),
            Estatus = EstatusRecibo.PARCIAL,
            Subtotal = 1000m,
            Saldo = 500m
        };
        _context.Recibo.Add(recibo);
        await _context.SaveChangesAsync();

        var pago = new Pago
        {
            FechaPagoUtc = DateTime.UtcNow,
            IdMedioPago = 1,
            Monto = 500m,
            Moneda = "MXN",
            Estatus = EstatusPago.CONFIRMADO
        };
        _context.Pago.Add(pago);
        await _context.SaveChangesAsync();

        var aplicacion = new PagoAplicacion
        {
            IdPago = pago.IdPago,
            MontoAplicado = 500m
        };

        var detalle = new ReciboDetalle
        {
            IdRecibo = recibo.IdRecibo,
            IdConceptoPago = 1,
            Descripcion = "Concepto test",
            Cantidad = 1,
            PrecioUnitario = 1000m,
            Aplicaciones = new List<PagoAplicacion> { aplicacion }
        };
        _context.ReciboDetalle.Add(detalle);
        await _context.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.EliminarReciboAsync(recibo.IdRecibo, CancellationToken.None)
        );
    }

    [Fact]
    public async Task ListarPorAspiranteAsync_ConRecibosExistentes_DebeRetornarLista()
    {
        var persona = new Persona { Nombre = "Test", ApellidoPaterno = "Usuario" };
        _context.Persona.Add(persona);
        await _context.SaveChangesAsync();

        var aspirante = new Aspirante { IdPersona = persona.IdPersona, Status = StatusEnum.Active };
        _context.Aspirante.Add(aspirante);
        await _context.SaveChangesAsync();

        var recibo1 = new Recibo
        {
            IdAspirante = aspirante.IdAspirante,
            Folio = "REC-ASP-001",
            FechaEmision = DateOnly.FromDateTime(DateTime.Now),
            FechaVencimiento = DateOnly.FromDateTime(DateTime.Now.AddDays(30)),
            Estatus = EstatusRecibo.PENDIENTE,
            Subtotal = 1000m,
            Saldo = 1000m
        };
        var recibo2 = new Recibo
        {
            IdAspirante = aspirante.IdAspirante,
            Folio = "REC-ASP-002",
            FechaEmision = DateOnly.FromDateTime(DateTime.Now),
            FechaVencimiento = DateOnly.FromDateTime(DateTime.Now.AddDays(60)),
            Estatus = EstatusRecibo.PENDIENTE,
            Subtotal = 2000m,
            Saldo = 2000m
        };
        _context.Recibo.AddRange(recibo1, recibo2);
        await _context.SaveChangesAsync();

        var dtos = new List<ReciboDto>
        {
            new() { IdRecibo = recibo1.IdRecibo, Folio = recibo1.Folio },
            new() { IdRecibo = recibo2.IdRecibo, Folio = recibo2.Folio }
        };
        _mapperMock.Setup(m => m.Map<IReadOnlyList<ReciboDto>>(It.IsAny<List<Recibo>>()))
            .Returns(dtos);

        var resultado = await _service.ListarPorAspiranteAsync(aspirante.IdAspirante, CancellationToken.None);

        resultado.Should().HaveCount(2);
    }

    [Fact]
    public async Task ListarPorAspiranteAsync_SinRecibos_DebeRetornarListaVacia()
    {
        var idAspiranteSinRecibos = 99999;
        _mapperMock.Setup(m => m.Map<IReadOnlyList<ReciboDto>>(It.IsAny<List<Recibo>>()))
            .Returns(new List<ReciboDto>());

        var resultado = await _service.ListarPorAspiranteAsync(idAspiranteSinRecibos, CancellationToken.None);

        resultado.Should().BeEmpty();
    }

    [Fact]
    public async Task RepararRecibosSinDetallesAsync_SinRecibosSinDetalles_DebeRetornarCero()
    {
        var recibo = new Recibo
        {
            Folio = "REC-CON-DETALLE",
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
            Descripcion = "Test",
            Cantidad = 1,
            PrecioUnitario = 1000m
        };
        _context.ReciboDetalle.Add(detalle);
        await _context.SaveChangesAsync();

        var resultado = await _service.RepararRecibosSinDetallesAsync(CancellationToken.None);

        resultado.Should().Be(0);
    }
}
