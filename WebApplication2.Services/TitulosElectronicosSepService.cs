using System.Text;
using System.Xml;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Core.Enums;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;
using StatusEnum = WebApplication2.Core.Enums.StatusEnum;

namespace WebApplication2.Services
{
    public class TitulosElectronicosSepService : ITitulosElectronicosSepService
    {
        private readonly ApplicationDbContext _db;
        private readonly ICertificadoXmlService _xmlService;
        private readonly ITituloElectronicoXmlService _tituloXmlService;
        private const string NsSchemas = "http://ws.web.mec.sep.mx/schemas";

        public TitulosElectronicosSepService(ApplicationDbContext db, ICertificadoXmlService xmlService, ITituloElectronicoXmlService tituloXmlService)
        {
            _db = db;
            _xmlService = xmlService;
            _tituloXmlService = tituloXmlService;
        }

        private static HttpClient CrearCliente()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
            };
            return new HttpClient(handler) { Timeout = TimeSpan.FromMinutes(3) };
        }

        private async Task<(string usuario, string password, string endpoint)> ObtenerCredencialAsync(CancellationToken ct)
        {
            var cred = await _db.CredencialSEP
                .Where(c => c.Activa && c.Status != StatusEnum.Deleted)
                .OrderByDescending(c => c.Id)
                .FirstOrDefaultAsync(ct)
                ?? throw new InvalidOperationException("No hay credenciales SEP configuradas (CredencialSEP).");

            if (string.IsNullOrWhiteSpace(cred.EndpointUrl))
                throw new InvalidOperationException("La credencial SEP no tiene EndpointUrl.");

            var endpoint = cred.EndpointUrl.Trim();
            if (endpoint.EndsWith(".wsdl", StringComparison.OrdinalIgnoreCase))
                endpoint = endpoint[..^5];
            if (endpoint.EndsWith("?wsdl", StringComparison.OrdinalIgnoreCase))
                endpoint = endpoint[..^5];

            return (cred.Usuario, cred.Password, endpoint);
        }

        private static async Task<string> PostSoapAsync(string endpoint, string soap, CancellationToken ct)
        {
            using var http = CrearCliente();
            using var req = new HttpRequestMessage(HttpMethod.Post, endpoint);
            req.Content = new StringContent(soap, new UTF8Encoding(false), "text/xml");
            req.Headers.TryAddWithoutValidation("SOAPAction", "\"\"");

            using var resp = await http.SendAsync(req, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);
            if (!resp.IsSuccessStatusCode && string.IsNullOrWhiteSpace(body))
                throw new InvalidOperationException($"SEP respondió HTTP {(int)resp.StatusCode}.");
            return body;
        }

        private static string? Valor(XmlDocument doc, string localName)
        {
            foreach (XmlNode n in doc.GetElementsByTagName("*"))
            {
                if (string.Equals(n.LocalName, localName, StringComparison.OrdinalIgnoreCase))
                    return n.InnerText;
            }
            return null;
        }

        private static (string? folio, bool? satisfactorio) ExtraerResultado(string? archivoBase64)
        {
            if (string.IsNullOrWhiteSpace(archivoBase64)) return (null, null);
            try
            {
                var bytes = Convert.FromBase64String(archivoBase64.Trim());
                var sb = new StringBuilder();

                try
                {
                    using var msZip = new MemoryStream(bytes);
                    using var zip = new System.IO.Compression.ZipArchive(msZip, System.IO.Compression.ZipArchiveMode.Read);
                    foreach (var entry in zip.Entries)
                    {
                        using var es = entry.Open();
                        using var msE = new MemoryStream();
                        es.CopyTo(msE);
                        var b = msE.ToArray();
                        sb.Append(Encoding.Latin1.GetString(b)).Append('\n');
                        sb.Append(Encoding.Unicode.GetString(b)).Append('\n');
                    }
                }
                catch
                {
                    sb.Append(Encoding.Latin1.GetString(bytes)).Append('\n');
                    sb.Append(Encoding.Unicode.GetString(bytes)).Append('\n');
                }

                var texto = sb.ToString();

                bool? satisfactorio = null;
                if (texto.Contains("Satisfactorio", StringComparison.OrdinalIgnoreCase)) satisfactorio = true;
                else if (texto.Contains("Inválido", StringComparison.OrdinalIgnoreCase) || texto.Contains("Invalido", StringComparison.OrdinalIgnoreCase)) satisfactorio = false;

                var m = System.Text.RegularExpressions.Regex.Match(texto, @"[A-Z]{2,6}\d{6,}");
                var folio = m.Success ? m.Value : null;

                return (folio, satisfactorio);
            }
            catch { return (null, null); }
        }

        public async Task<EnvioSepResultDto> EnviarAsync(int idCertificado, string? cveInstitucionOverride = null, CancellationToken ct = default)
        {
            var tituloXml = await _tituloXmlService.GenerarXmlAsync(idCertificado, cveInstitucionOverride, ct);

            var cert = await _db.CertificadoElectronico
                .FirstOrDefaultAsync(c => c.Id == idCertificado && c.Status == StatusEnum.Active, ct)
                ?? throw new InvalidOperationException("Certificado no encontrado.");

            if (tituloXml.Contains("sello=\"\""))
                throw new InvalidOperationException("El título no está sellado. Verifica que la e.firma (.cer/.key + contraseña) del responsable esté cargada.");

            var (usuario, password, endpoint) = await ObtenerCredencialAsync(ct);

            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(tituloXml));
            var nombreArchivo = $"{(string.IsNullOrWhiteSpace(cert.NumeroControl) ? "Titulo" : cert.NumeroControl)}.xml";

            var soap =
                $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:sch=""{NsSchemas}"">
  <soapenv:Header/>
  <soapenv:Body>
    <sch:cargaTituloElectronicoRequest>
      <sch:nombreArchivo>{System.Security.SecurityElement.Escape(nombreArchivo)}</sch:nombreArchivo>
      <sch:archivoBase64>{base64}</sch:archivoBase64>
      <sch:autenticacion>
        <sch:usuario>{System.Security.SecurityElement.Escape(usuario)}</sch:usuario>
        <sch:password>{System.Security.SecurityElement.Escape(password)}</sch:password>
      </sch:autenticacion>
    </sch:cargaTituloElectronicoRequest>
  </soapenv:Body>
</soapenv:Envelope>";

            var respuesta = await PostSoapAsync(endpoint, soap, ct);

            var doc = new XmlDocument();
            try { doc.LoadXml(respuesta); } catch { }

            var numeroLoteStr = Valor(doc, "numeroLote");
            var mensaje = Valor(doc, "mensaje") ?? "";
            var fault = Valor(doc, "faultstring");

            int? numeroLote = int.TryParse(numeroLoteStr, out var nl) ? nl : null;

            var resultado = new EnvioSepResultDto
            {
                NumeroLote = numeroLote,
                Mensaje = !string.IsNullOrWhiteSpace(fault) ? fault : (string.IsNullOrWhiteSpace(mensaje) ? "Enviado" : mensaje),
                Exitoso = numeroLote.HasValue && string.IsNullOrWhiteSpace(fault),
                RespuestaCruda = respuesta
            };

            cert.FechaEnvioSEP = DateTime.UtcNow;
            cert.MensajeSEP = resultado.Mensaje;
            if (numeroLote.HasValue)
            {
                cert.NumeroLoteSEP = numeroLote;
                cert.Estatus = EstatusCertificadoEnum.Enviado;
            }
            cert.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);

            return resultado;
        }

        public async Task<EnvioSepResultDto> ConsultarAsync(int idCertificado, CancellationToken ct = default)
        {
            var cert = await _db.CertificadoElectronico
                .FirstOrDefaultAsync(c => c.Id == idCertificado && c.Status == StatusEnum.Active, ct)
                ?? throw new InvalidOperationException("Certificado no encontrado.");

            if (!cert.NumeroLoteSEP.HasValue)
                throw new InvalidOperationException("Este certificado no tiene Número de Lote (aún no se ha enviado).");

            var (usuario, password, endpoint) = await ObtenerCredencialAsync(ct);

            var soap =
                $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:sch=""{NsSchemas}"">
  <soapenv:Header/>
  <soapenv:Body>
    <sch:consultaProcesoTituloElectronicoRequest>
      <sch:numeroLote>{cert.NumeroLoteSEP.Value}</sch:numeroLote>
      <sch:autenticacion>
        <sch:usuario>{System.Security.SecurityElement.Escape(usuario)}</sch:usuario>
        <sch:password>{System.Security.SecurityElement.Escape(password)}</sch:password>
      </sch:autenticacion>
    </sch:consultaProcesoTituloElectronicoRequest>
  </soapenv:Body>
</soapenv:Envelope>";

            var respuesta = await PostSoapAsync(endpoint, soap, ct);

            var doc = new XmlDocument();
            try { doc.LoadXml(respuesta); } catch { }

            var estatusStr = Valor(doc, "estatusLote");
            var mensaje = Valor(doc, "mensaje") ?? "";
            int? estatus = int.TryParse(estatusStr, out var es) ? es : null;

            cert.EstatusLoteSEP = estatus;
            cert.MensajeSEP = mensaje;
            cert.FechaRespuestaSEP = DateTime.UtcNow;
            cert.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);

            return new EnvioSepResultDto
            {
                Exitoso = true,
                NumeroLote = cert.NumeroLoteSEP,
                EstatusLote = estatus,
                Mensaje = string.IsNullOrWhiteSpace(mensaje) ? $"Estatus de lote: {estatus}" : mensaje,
                RespuestaCruda = respuesta
            };
        }

        public async Task<EnvioSepResultDto> DescargarAsync(int idCertificado, CancellationToken ct = default)
        {
            var cert = await _db.CertificadoElectronico
                .FirstOrDefaultAsync(c => c.Id == idCertificado && c.Status == StatusEnum.Active, ct)
                ?? throw new InvalidOperationException("Certificado no encontrado.");

            if (!cert.NumeroLoteSEP.HasValue)
                throw new InvalidOperationException("Este certificado no tiene Número de Lote (aún no se ha enviado).");

            var (usuario, password, endpoint) = await ObtenerCredencialAsync(ct);

            var soap =
                $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:sch=""{NsSchemas}"">
  <soapenv:Header/>
  <soapenv:Body>
    <sch:descargaTituloElectronicoRequest>
      <sch:numeroLote>{cert.NumeroLoteSEP.Value}</sch:numeroLote>
      <sch:autenticacion>
        <sch:usuario>{System.Security.SecurityElement.Escape(usuario)}</sch:usuario>
        <sch:password>{System.Security.SecurityElement.Escape(password)}</sch:password>
      </sch:autenticacion>
    </sch:descargaTituloElectronicoRequest>
  </soapenv:Body>
</soapenv:Envelope>";

            var respuesta = await PostSoapAsync(endpoint, soap, ct);

            var doc = new XmlDocument();
            try { doc.LoadXml(respuesta); } catch { }

            var mensaje = Valor(doc, "mensaje") ?? "";
            var archivoBase64 = Valor(doc, "titulosBase64") ?? Valor(doc, "certificadosBase64");

            var (folio, satisfactorio) = ExtraerResultado(archivoBase64);

            cert.MensajeSEP = mensaje;
            cert.FechaRespuestaSEP = DateTime.UtcNow;
            if (satisfactorio == true)
            {
                if (!string.IsNullOrWhiteSpace(folio)) cert.FolioControlSEP = folio;
                cert.Estatus = EstatusCertificadoEnum.Registrado;
            }
            else if (satisfactorio == false)
            {
                cert.Estatus = EstatusCertificadoEnum.Rechazado;
            }
            cert.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);

            return new EnvioSepResultDto
            {
                Exitoso = true,
                NumeroLote = cert.NumeroLoteSEP,
                FolioControl = satisfactorio == true ? folio : null,
                ArchivoBase64 = archivoBase64,
                Mensaje = string.IsNullOrWhiteSpace(mensaje) ? "Descarga procesada" : mensaje,
                RespuestaCruda = respuesta
            };
        }
    }
}
