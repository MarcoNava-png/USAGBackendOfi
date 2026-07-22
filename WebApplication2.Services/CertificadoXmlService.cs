using System.Text;
using System.Xml;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Core.Enums;
using WebApplication2.Core.Models.Titulacion;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Services
{
    public class CertificadoXmlService : ICertificadoXmlService
    {
        private readonly ApplicationDbContext _db;
        private const string Namespace = "https://www.siged.sep.gob.mx/certificados/";

        public CertificadoXmlService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<string> GenerarXmlAsync(int idCertificado, CancellationToken ct = default)
        {
            var cert = await _db.CertificadoElectronico
                .Include(c => c.ConfiguracionIPESNavigation)
                .Include(c => c.ResponsableFirmaNavigation)
                .Include(c => c.Asignaturas)
                .FirstOrDefaultAsync(c => c.Id == idCertificado && c.Status == StatusEnum.Active, ct)
                ?? throw new InvalidOperationException("Certificado no encontrado");

            var config = cert.ConfiguracionIPESNavigation
                ?? throw new InvalidOperationException("Configuración IPES no encontrada para este certificado");

            var responsable = cert.ResponsableFirmaNavigation;
            if (responsable == null)
            {
                responsable = await _db.ResponsableFirma
                    .Where(r => r.Activo && r.Status == StatusEnum.Active
                        && r.RutaCertificadoCer != null && r.RutaLlavePrivadaKey != null && r.PasswordLlavePrivada != null)
                    .OrderByDescending(r => r.IdConfiguracionIPES == cert.IdConfiguracionIPES)
                    .ThenByDescending(r => r.Id)
                    .FirstOrDefaultAsync(ct);

                if (responsable == null)
                    throw new InvalidOperationException("No hay un responsable de firma con e.firma (.cer/.key + contraseña) cargada. Sube la e.firma del responsable en Configuración de Titulación.");

                cert.IdResponsableFirma = responsable.Id;
            }

            var certificadoResponsableB64 = "";
            var noCertificado = responsable.NoCertificadoResponsable ?? "";
            if (!string.IsNullOrWhiteSpace(responsable.RutaCertificadoCer) && System.IO.File.Exists(responsable.RutaCertificadoCer))
            {
                var cerBytes = await System.IO.File.ReadAllBytesAsync(responsable.RutaCertificadoCer, ct);
                certificadoResponsableB64 = Convert.ToBase64String(cerBytes);
                if (string.IsNullOrWhiteSpace(noCertificado))
                {
                    try
                    {
                        using var x509 = System.Security.Cryptography.X509Certificates.X509CertificateLoader.LoadCertificate(cerBytes);
                        var serial = x509.GetSerialNumber();
                        Array.Reverse(serial);
                        var ascii = Encoding.ASCII.GetString(serial);
                        noCertificado = ascii.Length > 0 && ascii.All(char.IsDigit) ? ascii : x509.SerialNumber;
                    }
                    catch { }
                }
                if (!string.IsNullOrWhiteSpace(noCertificado) && string.IsNullOrWhiteSpace(responsable.NoCertificadoResponsable))
                    responsable.NoCertificadoResponsable = noCertificado;
            }

            if (!cert.Asignaturas.Any())
                throw new InvalidOperationException("El certificado no tiene asignaturas registradas");

            var carrera = await _db.CatalogoCarreraSEP
                .FirstOrDefaultAsync(c => c.IdCarrera == cert.IdCarreraSEP && c.Activo, ct);

            var idNivel = carrera?.IdNivelEstudios ?? "81";
            var nivelEntity = await _db.CatalogoNivelEstudiosSEP
                .FirstOrDefaultAsync(n => n.IdNivelEstudios == idNivel, ct);
            var nivelEstudios = nivelEntity != null ? new NivelEstudioResult { Codigo = nivelEntity.IdNivelEstudios, Descripcion = nivelEntity.Descripcion } : null;

            var settings = new XmlWriterSettings
            {
                Indent = true,
                Encoding = new UTF8Encoding(false),
                OmitXmlDeclaration = false
            };

            using var ms = new MemoryStream();
            using (var writer = XmlWriter.Create(ms, settings))
            {
                writer.WriteStartDocument();
                WriteDecElement(writer, cert, config, responsable, carrera, nivelEstudios, certificadoResponsableB64, noCertificado);
                writer.WriteEndDocument();
            }

            var xml = Encoding.UTF8.GetString(ms.ToArray());

            cert.CadenaOriginal = GenerarCadenaOriginal(idCertificado, xml);

            if (!string.IsNullOrWhiteSpace(responsable.RutaLlavePrivadaKey)
                && System.IO.File.Exists(responsable.RutaLlavePrivadaKey)
                && !string.IsNullOrEmpty(responsable.PasswordLlavePrivada))
            {
                var llaveBytes = await System.IO.File.ReadAllBytesAsync(responsable.RutaLlavePrivadaKey, ct);
                cert.SelloDigital = FirmaDigitalSep.GenerarSello(cert.CadenaOriginal, llaveBytes, responsable.PasswordLlavePrivada);

                var docFirmado = new XmlDocument { PreserveWhitespace = true };
                docFirmado.LoadXml(xml);
                var ns = new XmlNamespaceManager(docFirmado.NameTable);
                ns.AddNamespace("dec", Namespace);
                if (docFirmado.SelectSingleNode("/dec:Dec", ns) is XmlElement decEl)
                    decEl.SetAttribute("sello", cert.SelloDigital);

                using var msOut = new MemoryStream();
                using (var w2 = XmlWriter.Create(msOut, settings))
                {
                    docFirmado.Save(w2);
                }
                xml = Encoding.UTF8.GetString(msOut.ToArray());
            }

            cert.XmlGenerado = xml;
            cert.Estatus = EstatusCertificadoEnum.XMLGenerado;
            cert.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);

            return xml;
        }

        private void WriteDecElement(
            XmlWriter w,
            CertificadoElectronico cert,
            ConfiguracionIPES config,
            ResponsableFirma responsable,
            CatalogoCarreraSEP? carrera,
            NivelEstudioResult? nivelEstudios,
            string certificadoResponsableB64,
            string noCertificado)
        {
            w.WriteStartElement("Dec", Namespace);
            w.WriteAttributeString("xsi", "schemaLocation", "http://www.w3.org/2001/XMLSchema-instance", $"{Namespace} {Namespace}IPESCertificado1_0.xsd");
            w.WriteAttributeString("version", "3.0");
            w.WriteAttributeString("tipoCertificado", "5");
            if (!string.IsNullOrEmpty(cert.FolioControl))
                w.WriteAttributeString("folioControl", cert.FolioControl);
            w.WriteAttributeString("sello", cert.SelloDigital ?? "");
            w.WriteAttributeString("certificadoResponsable", certificadoResponsableB64);
            w.WriteAttributeString("noCertificadoResponsable", noCertificado);

            WriteServicioFirmante(w, config);
            WriteIpes(w, config, responsable);
            WriteRvoe(w, cert);
            WriteCarrera(w, cert, carrera, nivelEstudios);
            WriteAlumno(w, cert);
            WriteExpedicion(w, cert);
            WriteAsignaturas(w, cert);

            w.WriteEndElement();
        }

        private void WriteServicioFirmante(XmlWriter w, ConfiguracionIPES config)
        {
            w.WriteStartElement("ServicioFirmante", Namespace);
            w.WriteAttributeString("idEntidad", config.IdNombreInstitucion);
            w.WriteEndElement();
        }

        private void WriteIpes(XmlWriter w, ConfiguracionIPES config, ResponsableFirma responsable)
        {
            w.WriteStartElement("Ipes", Namespace);
            w.WriteAttributeString("idNombreInstitucion", config.IdNombreInstitucion);
            if (!string.IsNullOrEmpty(config.NombreInstitucion))
                w.WriteAttributeString("nombreInstitucion", config.NombreInstitucion);
            w.WriteAttributeString("idCampus", config.IdCampusSEP);
            if (!string.IsNullOrEmpty(config.CampusSEP))
                w.WriteAttributeString("campus", config.CampusSEP);
            w.WriteAttributeString("idEntidadFederativa", config.IdEntidadFederativa);
            if (!string.IsNullOrEmpty(config.EntidadFederativa))
                w.WriteAttributeString("entidadFederativa", config.EntidadFederativa);

            w.WriteStartElement("Responsable", Namespace);
            w.WriteAttributeString("curp", responsable.Curp);
            w.WriteAttributeString("nombre", responsable.Nombre);
            w.WriteAttributeString("primerApellido", responsable.PrimerApellido);
            if (!string.IsNullOrEmpty(responsable.SegundoApellido))
                w.WriteAttributeString("segundoApellido", responsable.SegundoApellido);
            w.WriteAttributeString("idCargo", responsable.IdCargo);
            if (!string.IsNullOrEmpty(responsable.Cargo))
                w.WriteAttributeString("cargo", responsable.Cargo);
            w.WriteEndElement();

            w.WriteEndElement();
        }

        private void WriteRvoe(XmlWriter w, CertificadoElectronico cert)
        {
            w.WriteStartElement("Rvoe", Namespace);
            w.WriteAttributeString("numero", cert.NumeroRvoe);
            w.WriteAttributeString("fechaExpedicion", cert.FechaExpedicionRvoe.ToString("yyyy-MM-ddTHH:mm:ss"));
            w.WriteEndElement();
        }

        private void WriteCarrera(XmlWriter w, CertificadoElectronico cert, CatalogoCarreraSEP? carrera, NivelEstudioResult? nivel)
        {
            w.WriteStartElement("Carrera", Namespace);
            w.WriteAttributeString("idCarrera", cert.IdCarreraSEP);
            if (!string.IsNullOrEmpty(cert.ClaveCarrera))
                w.WriteAttributeString("claveCarrera", cert.ClaveCarrera);
            if (!string.IsNullOrEmpty(cert.NombreCarrera))
                w.WriteAttributeString("nombreCarrera", cert.NombreCarrera);
            w.WriteAttributeString("idTipoPeriodo", cert.IdTipoPeriodo);
            if (!string.IsNullOrEmpty(cert.TipoPeriodo))
                w.WriteAttributeString("tipoPeriodo", cert.TipoPeriodo);
            w.WriteAttributeString("clavePlan", cert.ClavePlan);

            if (nivel != null)
            {
                w.WriteAttributeString("idNivelEstudios", nivel.Codigo);
                w.WriteAttributeString("nivelEstudios", nivel.Descripcion);
            }

            w.WriteAttributeString("calificacionMinima", "5");
            w.WriteAttributeString("calificacionMaxima", "10");
            w.WriteAttributeString("calificacionMinimaAprobatoria", "7.00");

            w.WriteEndElement();
        }

        private void WriteAlumno(XmlWriter w, CertificadoElectronico cert)
        {
            w.WriteStartElement("Alumno", Namespace);
            w.WriteAttributeString("numeroControl", cert.NumeroControl);
            if (!string.IsNullOrEmpty(cert.Curp))
                w.WriteAttributeString("curp", cert.Curp);
            w.WriteAttributeString("nombre", cert.Nombre);
            w.WriteAttributeString("primerApellido", cert.PrimerApellido);
            if (!string.IsNullOrEmpty(cert.SegundoApellido))
                w.WriteAttributeString("segundoApellido", cert.SegundoApellido);
            w.WriteAttributeString("idGenero", cert.IdGenero.ToString());
            w.WriteAttributeString("fechaNacimiento", cert.FechaNacimiento.ToString("yyyy-MM-ddTHH:mm:ss"));
            if (!string.IsNullOrEmpty(cert.FotoHash))
                w.WriteAttributeString("foto", cert.FotoHash);
            if (!string.IsNullOrEmpty(cert.FirmaAutografaHash))
                w.WriteAttributeString("firmaAutografa", cert.FirmaAutografaHash);
            w.WriteEndElement();
        }

        private void WriteExpedicion(XmlWriter w, CertificadoElectronico cert)
        {
            w.WriteStartElement("Expedicion", Namespace);
            w.WriteAttributeString("idTipoCertificacion", cert.IdTipoCertificacion);
            if (!string.IsNullOrEmpty(cert.TipoCertificacion))
                w.WriteAttributeString("tipoCertificacion", cert.TipoCertificacion);
            w.WriteAttributeString("fecha", cert.FechaExpedicion.ToString("yyyy-MM-ddTHH:mm:ss"));
            w.WriteAttributeString("idLugarExpedicion", cert.IdLugarExpedicion);
            if (!string.IsNullOrEmpty(cert.LugarExpedicion))
                w.WriteAttributeString("lugarExpedicion", cert.LugarExpedicion);
            w.WriteEndElement();
        }

        private void WriteAsignaturas(XmlWriter w, CertificadoElectronico cert)
        {
            var asignaturas = cert.Asignaturas
                .Where(a => a.Status == StatusEnum.Active)
                .OrderBy(a => a.IdAsignatura)
                .ToList();

            var totalCreditos = asignaturas.Sum(a => a.Creditos ?? 0);
            var ciclosDistintos = asignaturas.Select(a => a.Ciclo).Distinct().Count();

            w.WriteStartElement("Asignaturas", Namespace);
            w.WriteAttributeString("total", cert.TotalAsignaturas.ToString());
            w.WriteAttributeString("asignadas", cert.AsignaturasAsignadas.ToString());
            w.WriteAttributeString("promedio", cert.Promedio ?? "0");
            if (totalCreditos > 0)
            {
                w.WriteAttributeString("creditosObtenidos", totalCreditos.ToString("F2"));
                w.WriteAttributeString("totalCreditos", totalCreditos.ToString("F2"));
            }
            if (ciclosDistintos > 0)
                w.WriteAttributeString("numeroCiclos", ciclosDistintos.ToString());

            foreach (var asig in asignaturas)
            {
                w.WriteStartElement("Asignatura", Namespace);
                w.WriteAttributeString("idAsignatura", asig.IdAsignatura.ToString());
                if (!string.IsNullOrEmpty(asig.ClaveAsignatura))
                    w.WriteAttributeString("claveAsignatura", asig.ClaveAsignatura);
                w.WriteAttributeString("nombre", asig.Nombre);
                w.WriteAttributeString("ciclo", asig.Ciclo);
                w.WriteAttributeString("calificacion", asig.Calificacion);
                if (asig.Creditos.HasValue)
                    w.WriteAttributeString("creditos", asig.Creditos.Value.ToString("F2"));
                w.WriteAttributeString("idTipoAsignatura", (asig.IdTipoAsignatura ?? 263).ToString());
                w.WriteAttributeString("tipoAsignatura", asig.TipoAsignatura ?? "OBLIGATORIA");
                if (asig.IdObservaciones.HasValue)
                    w.WriteAttributeString("idObservaciones", asig.IdObservaciones.Value.ToString());
                if (!string.IsNullOrEmpty(asig.Observaciones))
                    w.WriteAttributeString("observaciones", asig.Observaciones);
                w.WriteEndElement();
            }

            w.WriteEndElement();
        }

        public string GenerarCadenaOriginal(int idCertificado, string xml)
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            var nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("dec", Namespace);

            var sb = new StringBuilder("||");

            AppendAttr(sb, doc, nsmgr, "/dec:Dec", "version");
            AppendAttr(sb, doc, nsmgr, "/dec:Dec", "tipoCertificado");

            AppendAttr(sb, doc, nsmgr, "/dec:Dec/dec:Ipes", "idNombreInstitucion");
            AppendAttr(sb, doc, nsmgr, "/dec:Dec/dec:Ipes", "idCampus");
            AppendAttr(sb, doc, nsmgr, "/dec:Dec/dec:Ipes", "idEntidadFederativa");

            AppendAttr(sb, doc, nsmgr, "/dec:Dec/dec:Ipes/dec:Responsable", "curp");
            AppendAttr(sb, doc, nsmgr, "/dec:Dec/dec:Ipes/dec:Responsable", "idCargo");

            AppendAttr(sb, doc, nsmgr, "/dec:Dec/dec:Rvoe", "numero");
            AppendAttr(sb, doc, nsmgr, "/dec:Dec/dec:Rvoe", "fechaExpedicion");

            AppendAttr(sb, doc, nsmgr, "/dec:Dec/dec:Carrera", "idCarrera");
            AppendAttr(sb, doc, nsmgr, "/dec:Dec/dec:Carrera", "idTipoPeriodo");
            AppendAttr(sb, doc, nsmgr, "/dec:Dec/dec:Carrera", "clavePlan");

            AppendAttr(sb, doc, nsmgr, "/dec:Dec/dec:Alumno", "numeroControl");
            AppendAttr(sb, doc, nsmgr, "/dec:Dec/dec:Alumno", "curp");
            AppendAttr(sb, doc, nsmgr, "/dec:Dec/dec:Alumno", "nombre");
            AppendAttr(sb, doc, nsmgr, "/dec:Dec/dec:Alumno", "primerApellido");
            AppendAttr(sb, doc, nsmgr, "/dec:Dec/dec:Alumno", "segundoApellido");
            AppendAttr(sb, doc, nsmgr, "/dec:Dec/dec:Alumno", "idGenero");
            AppendAttr(sb, doc, nsmgr, "/dec:Dec/dec:Alumno", "fechaNacimiento");
            AppendAttr(sb, doc, nsmgr, "/dec:Dec/dec:Alumno", "foto");
            AppendAttr(sb, doc, nsmgr, "/dec:Dec/dec:Alumno", "firmaAutografa");

            AppendAttr(sb, doc, nsmgr, "/dec:Dec/dec:Expedicion", "idTipoCertificacion");
            AppendAttr(sb, doc, nsmgr, "/dec:Dec/dec:Expedicion", "fecha");
            AppendAttr(sb, doc, nsmgr, "/dec:Dec/dec:Expedicion", "idLugarExpedicion");

            AppendAttr(sb, doc, nsmgr, "/dec:Dec/dec:Asignaturas", "total");
            AppendAttr(sb, doc, nsmgr, "/dec:Dec/dec:Asignaturas", "asignadas");
            AppendAttr(sb, doc, nsmgr, "/dec:Dec/dec:Asignaturas", "promedio");

            var asignaturas = doc.SelectNodes("/dec:Dec/dec:Asignaturas/dec:Asignatura", nsmgr);
            if (asignaturas != null)
            {
                foreach (XmlNode asig in asignaturas)
                {
                    AppendNodeAttr(sb, asig, "idAsignatura");
                    AppendNodeAttr(sb, asig, "ciclo");
                    AppendNodeAttr(sb, asig, "calificacion");
                }
            }

            sb.Append('|');
            return sb.ToString();
        }

        private static void AppendAttr(StringBuilder sb, XmlDocument doc, XmlNamespaceManager nsmgr, string xpath, string attr)
        {
            var node = doc.SelectSingleNode(xpath, nsmgr);
            var val = node?.Attributes?[attr]?.Value;
            if (!string.IsNullOrEmpty(val))
                sb.Append(val).Append('|');
        }

        private static void AppendNodeAttr(StringBuilder sb, XmlNode node, string attr)
        {
            var val = node.Attributes?[attr]?.Value;
            if (!string.IsNullOrEmpty(val))
                sb.Append(val).Append('|');
        }
    }

    internal class NivelEstudioResult
    {
        public string Codigo { get; set; } = "";
        public string Descripcion { get; set; } = "";
    }
}
