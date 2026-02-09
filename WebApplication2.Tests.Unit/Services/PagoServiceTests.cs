using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using AutoMapper;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.DTOs.Pagos;
using WebApplication2.Core.Enums;
using WebApplication2.Core.Models;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services;

namespace WebApplication2.Tests.Unit.Services;

public class PagoServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<IMapper> _mapperMock;
    private readonly PagoService _service;

    public PagoServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var loggerMock = new Mock<ILogger<ApplicationDbContext>>();

        _context = new ApplicationDbContext(options, httpContextAccessorMock.Object, loggerMock.Object);
        _mapperMock = new Mock<IMapper>();
        _service = new PagoService(_context, _mapperMock.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task RegistrarPagoAsync_ConDatosValidos_DebeCrearPago()
    {
        var dto = new RegistrarPagoDto
        {
            FechaPagoUtc = DateTime.UtcNow,
            IdMedioPago = 1,
            Monto = 1000m,
            Moneda = "MXN",
            Referencia = "REF-001",
            Notas = "Pago de prueba",
            estatus = EstatusPago.CONFIRMADO
        };

        var idPago = await _service.RegistrarPagoAsync(dto, CancellationToken.None);

        idPago.Should().BeGreaterThan(0);

        var pagoCreado = await _context.Pago.FindAsync(idPago);
        pagoCreado.Should().NotBeNull();
        pagoCreado!.Monto.Should().Be(1000m);
        pagoCreado.Moneda.Should().Be("MXN");
        pagoCreado.Referencia.Should().Be("REF-001");
    }

    [Fact]
    public async Task RegistrarPagoAsync_ConMontoEnCero_DebeCrearPago()
    {
        var dto = new RegistrarPagoDto
        {
            FechaPagoUtc = DateTime.UtcNow,
            IdMedioPago = 1,
            Monto = 0m,
            Moneda = "MXN"
        };

        var idPago = await _service.RegistrarPagoAsync(dto, CancellationToken.None);

        idPago.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData(100.00)]
    [InlineData(1500.50)]
    [InlineData(99999.99)]
    public async Task RegistrarPagoAsync_ConDiferentesMontos_DebeRegistrarCorrectamente(decimal monto)
    {
        var dto = new RegistrarPagoDto
        {
            FechaPagoUtc = DateTime.UtcNow,
            IdMedioPago = 1,
            Monto = monto,
            Moneda = "MXN"
        };

        var idPago = await _service.RegistrarPagoAsync(dto, CancellationToken.None);

        var pago = await _context.Pago.FindAsync(idPago);
        pago!.Monto.Should().Be(monto);
    }

    [Fact]
    public async Task AplicarPagoAsync_ConPagoInexistente_DebeLanzarExcepcion()
    {
        var dto = new AplicarPagoDto
        {
            IdPago = 99999,
            Aplicaciones = new List<AplicacionLineaDto>
            {
                new() { IdReciboDetalle = 1, Monto = 100 }
            }
        };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.AplicarPagoAsync(dto, CancellationToken.None)
        );
    }

    [Fact]
    public async Task AplicarPagoAsync_ConPagoExistente_DebeCrearAplicaciones()
    {
        var pago = new Pago
        {
            FechaPagoUtc = DateTime.UtcNow,
            IdMedioPago = 1,
            Monto = 500m,
            Moneda = "MXN",
            Estatus = EstatusPago.CONFIRMADO
        };
        _context.Pago.Add(pago);

        var recibo = new Recibo
        {
            Folio = "REC-001",
            FechaEmision = DateOnly.FromDateTime(DateTime.Now),
            FechaVencimiento = DateOnly.FromDateTime(DateTime.Now.AddDays(30)),
            Estatus = EstatusRecibo.PENDIENTE,
            Subtotal = 1000m,
            Descuento = 0m,
            Recargos = 0m,
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

        var dto = new AplicarPagoDto
        {
            IdPago = pago.IdPago,
            Aplicaciones = new List<AplicacionLineaDto>
            {
                new() { IdReciboDetalle = detalle.IdReciboDetalle, Monto = 500m }
            }
        };

        var resultado = await _service.AplicarPagoAsync(dto, CancellationToken.None);

        resultado.Should().NotBeEmpty();
        resultado.Should().Contain(recibo.IdRecibo);

        var aplicacion = await _context.PagoAplicacion
            .FirstOrDefaultAsync(a => a.IdPago == pago.IdPago);
        aplicacion.Should().NotBeNull();
        aplicacion!.MontoAplicado.Should().Be(500m);
    }

    [Fact]
    public async Task RegistrarPagoAsync_DebeAsignarEstatusConfirmado()
    {
        var dto = new RegistrarPagoDto
        {
            FechaPagoUtc = DateTime.UtcNow,
            IdMedioPago = 1,
            Monto = 100m,
            Moneda = "MXN",
            estatus = EstatusPago.CONFIRMADO
        };

        var idPago = await _service.RegistrarPagoAsync(dto, CancellationToken.None);

        var pago = await _context.Pago.FindAsync(idPago);
        pago!.Estatus.Should().Be(EstatusPago.CONFIRMADO);
    }

    [Theory]
    [InlineData("MXN")]
    [InlineData("USD")]
    public async Task RegistrarPagoAsync_ConDiferentesMonedas_DebeRegistrarCorrectamente(string moneda)
    {
        var dto = new RegistrarPagoDto
        {
            FechaPagoUtc = DateTime.UtcNow,
            IdMedioPago = 1,
            Monto = 100m,
            Moneda = moneda
        };

        var idPago = await _service.RegistrarPagoAsync(dto, CancellationToken.None);

        var pago = await _context.Pago.FindAsync(idPago);
        pago!.Moneda.Should().Be(moneda);
    }
}
