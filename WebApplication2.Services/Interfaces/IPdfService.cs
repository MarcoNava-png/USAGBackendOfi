using WebApplication2.Core.DTOs;
using WebApplication2.Core.DTOs.Admision;
using WebApplication2.Core.DTOs.Comprobante;
using WebApplication2.Core.DTOs.Documentos;
using WebApplication2.Core.DTOs.Recibo;

namespace WebApplication2.Services.Interfaces;

public interface IPdfService
{
    byte[] GenerarHojaInscripcion(FichaAdmisionDto fichaAdmision);

    Task<byte[]> GenerarKardexPdf(KardexEstudianteDto kardex, string folioDocumento, Guid codigoVerificacion, string urlVerificacion);

    Task<byte[]> GenerarConstanciaPdf(ConstanciaEstudiosDto constancia);

    byte[] GenerarComprobantePago(ComprobantePagoDto comprobante);

    byte[] GenerarReciboPdf(ReciboPdfDto recibo);
}
