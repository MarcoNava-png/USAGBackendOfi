using FluentAssertions;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.DTOs.Pagos;
using WebApplication2.Core.Enums;
using WebApplication2.Core.Requests.Pagos;

namespace WebApplication2.Tests.Unit.Validation;

public class PagoValidationTests
{
    [Fact]
    public void RegistrarPagoDto_ConDatosCompletos_DebeSerValido()
    {
        var dto = new RegistrarPagoDto
        {
            FechaPagoUtc = DateTime.UtcNow,
            IdMedioPago = 1,
            Monto = 1000m,
            Moneda = "MXN",
            Referencia = "REF-001",
            estatus = EstatusPago.CONFIRMADO
        };

        dto.Monto.Should().BePositive();
        dto.Moneda.Should().NotBeNullOrEmpty();
        dto.IdMedioPago.Should().BePositive();
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(100)]
    [InlineData(1000)]
    [InlineData(50000)]
    [InlineData(999999.99)]
    public void RegistrarPagoDto_ConMontosValidos_DebeAceptarValor(decimal monto)
    {
        var dto = new RegistrarPagoDto
        {
            Monto = monto,
            Moneda = "MXN",
            IdMedioPago = 1
        };

        dto.Monto.Should().Be(monto);
        dto.Monto.Should().BeGreaterThanOrEqualTo(0);
    }

    [Theory]
    [InlineData(1, "Efectivo")]
    [InlineData(2, "Transferencia")]
    [InlineData(3, "Tarjeta")]
    public void RegistrarPagoDto_ConMediosPagoValidos_DebeAceptar(int idMedioPago, string descripcion)
    {
        var dto = new RegistrarPagoDto
        {
            IdMedioPago = idMedioPago,
            Monto = 100,
            Moneda = "MXN"
        };

        dto.IdMedioPago.Should().Be(idMedioPago);
        descripcion.Should().NotBeEmpty();
    }

    [Fact]
    public void RegistrarYAplicarPagoDto_DebeInicializarValoresPorDefecto()
    {
        var dto = new RegistrarYAplicarPagoDto();

        dto.FechaPagoUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        dto.Moneda.Should().Be("MXN");
        dto.Estatus.Should().Be(EstatusPago.CONFIRMADO);
    }

    [Fact]
    public void RegistrarYAplicarPagoDto_ConIdReciboYMonto_DebeSerValido()
    {
        var dto = new RegistrarYAplicarPagoDto
        {
            IdRecibo = 123,
            Monto = 500m,
            IdMedioPago = 1
        };

        dto.IdRecibo.Should().Be(123);
        dto.Monto.Should().Be(500m);
    }

    [Fact]
    public void RegistrarPagoCajaRequest_DebeInicializarListaVacia()
    {
        var request = new RegistrarPagoCajaRequest();

        request.RecibosSeleccionados.Should().NotBeNull();
        request.RecibosSeleccionados.Should().BeEmpty();
    }

    [Fact]
    public void RegistrarPagoCajaRequest_ConMultiplesRecibos_DebeCalcularMontoTotal()
    {
        var request = new RegistrarPagoCajaRequest
        {
            Monto = 1500m,
            RecibosSeleccionados = new List<ReciboParaPago>
            {
                new() { IdRecibo = 1, MontoAplicar = 500m },
                new() { IdRecibo = 2, MontoAplicar = 700m },
                new() { IdRecibo = 3, MontoAplicar = 300m }
            }
        };

        var totalAplicar = request.RecibosSeleccionados.Sum(r => r.MontoAplicar);

        totalAplicar.Should().Be(1500m);
        request.Monto.Should().Be(totalAplicar);
    }

    [Fact]
    public void RegistrarPagoCajaRequest_ConDescuentoAutorizado_DebeIncluirMotivo()
    {
        var request = new RegistrarPagoCajaRequest
        {
            Monto = 900m,
            DescuentoAutorizado = new DescuentoAutorizado
            {
                Monto = 100m,
                AutorizadoPor = "Director Financiero",
                Motivo = "Convenio especial"
            },
            RecibosSeleccionados = new List<ReciboParaPago>
            {
                new() { IdRecibo = 1, MontoAplicar = 900m }
            }
        };

        request.DescuentoAutorizado.Should().NotBeNull();
        request.DescuentoAutorizado!.Monto.Should().Be(100m);
        request.DescuentoAutorizado.Motivo.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData(1000, 200, 800)]
    [InlineData(5000, 5000, 0)]
    [InlineData(3500, 1000, 2500)]
    [InlineData(100, 50, 50)]
    public void CalcularSaldoRestante_DebeSerCorrecto(decimal saldoInicial, decimal pago, decimal saldoEsperado)
    {
        var saldoRestante = saldoInicial - pago;

        saldoRestante.Should().Be(saldoEsperado);
    }

    [Theory]
    [InlineData(1000, 0, 100, 1100)]
    [InlineData(1000, 100, 0, 900)]
    [InlineData(1000, 50, 30, 980)]
    [InlineData(1000, 0, 0, 1000)]
    public void CalcularTotalRecibo_ConDescuentoYRecargo_DebeSerCorrecto(
        decimal subtotal, decimal descuento, decimal recargo, decimal totalEsperado)
    {
        var total = subtotal - descuento + recargo;

        total.Should().Be(totalEsperado);
    }

    [Theory]
    [InlineData(1000, 0.01, 10, 100)]
    [InlineData(1000, 0.01, 30, 300)]
    [InlineData(5000, 0.01, 5, 250)]
    public void CalcularRecargo_PorDiasVencidos_DebeSerCorrecto(
        decimal saldo, decimal tasaDiaria, int diasVencido, decimal recargoEsperado)
    {
        var recargo = saldo * tasaDiaria * diasVencido;

        recargo.Should().Be(recargoEsperado);
    }

    [Theory]
    [InlineData(1000, 0, EstatusRecibo.PAGADO)]
    [InlineData(1000, 500, EstatusRecibo.PARCIAL)]
    [InlineData(1000, 1000, EstatusRecibo.PENDIENTE)]
    public void DeterminarEstatusRecibo_SegunSaldo_DebeSerCorrecto(
        decimal montoOriginal, decimal saldoActual, EstatusRecibo estatusEsperado)
    {
        var montoPagado = montoOriginal - saldoActual;

        EstatusRecibo estatus;
        if (saldoActual <= 0)
            estatus = EstatusRecibo.PAGADO;
        else if (montoPagado > 0)
            estatus = EstatusRecibo.PARCIAL;
        else
            estatus = EstatusRecibo.PENDIENTE;

        estatus.Should().Be(estatusEsperado);
    }
}
