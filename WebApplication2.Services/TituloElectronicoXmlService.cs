using System.Text;
using System.Xml;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Core.Models.Titulacion;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;
using StatusEnum = WebApplication2.Core.Enums.StatusEnum;

namespace WebApplication2.Services
{
    public class TituloElectronicoXmlService : ITituloElectronicoXmlService
    {
        private readonly ApplicationDbContext _db;
        private const string Ns = "https://www.siged.sep.gob.mx/titulos/";

        private const int DefaultIdAutorizacionReconocimiento = 5;
        private const string DefaultAutorizacionReconocimiento = "RVOE";
        private const int DefaultIdModalidadTitulacion = 1;
        private const string DefaultModalidadTitulacion = "POR PROMEDIO";
        private const int DefaultCumplioServicioSocial = 1;
        private const int DefaultIdFundamentoLegalServicioSocial = 2;
        private const string DefaultFundamentoLegalServicioSocial = "ART. 55 LR ART. 5 CONST";
        private const int DefaultIdTipoEstudioAntecedente = 4;
        private const string DefaultTipoEstudioAntecedente = "BACHILLERATO";
        private const string DefaultInstitucionProcedencia = "BACHILLERATO";

        public TituloElectronicoXmlService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<string> GenerarXmlAsync(int idCertificado, string? cveInstitucionOverride = null, CancellationToken ct = default)
        {
            var cert = await _db.CertificadoElectronico
                .Include(c => c.ConfiguracionIPESNavigation)
                .Include(c => c.ResponsableFirmaNavigation)
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
                    .FirstOrDefaultAsync(ct)
                    ?? throw new InvalidOperationException("No hay un responsable de firma con e.firma cargada.");
            }

            var correo = "";
            if (cert.IdPersona.HasValue)
            {
                correo = await _db.Persona.Where(p => p.IdPersona == cert.IdPersona.Value)
                    .Select(p => p.Correo).FirstOrDefaultAsync(ct) ?? "";
            }
            if (string.IsNullOrWhiteSpace(correo))
                correo = $"{cert.NumeroControl.ToLower()}@{config.NombreInstitucion?.Replace(" ", "").ToLower() ?? "institucion"}.edu.mx";

            var certificadoB64 = "";
            var noCertificado = responsable.NoCertificadoResponsable ?? "";
            if (!string.IsNullOrWhiteSpace(responsable.RutaCertificadoCer) && System.IO.File.Exists(responsable.RutaCertificadoCer))
            {
                var cerBytes = await System.IO.File.ReadAllBytesAsync(responsable.RutaCertificadoCer, ct);
                certificadoB64 = Convert.ToBase64String(cerBytes);
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
            }

            var datos = new TituloData
            {
                Version = "1.0",
                FolioControl = string.IsNullOrWhiteSpace(cert.FolioControl) ? cert.NumeroControl : cert.FolioControl,
                FirmaNombre = responsable.Nombre,
                FirmaPrimerApellido = responsable.PrimerApellido,
                FirmaSegundoApellido = responsable.SegundoApellido,
                FirmaCurp = responsable.Curp,
                FirmaIdCargo = int.TryParse(responsable.IdCargo, out var idc) ? idc : 1,
                FirmaCargo = responsable.Cargo ?? "DIRECTOR",
                CveInstitucion = string.IsNullOrWhiteSpace(cveInstitucionOverride) ? config.IdNombreInstitucion : cveInstitucionOverride,
                NombreInstitucion = config.NombreInstitucion ?? "",
                CveCarrera = string.IsNullOrWhiteSpace(cert.ClaveCarrera) ? cert.IdCarreraSEP : cert.ClaveCarrera!,
                NombreCarrera = cert.NombreCarrera ?? "",
                FechaTerminacionCarrera = cert.FechaExpedicion,
                IdAutorizacionReconocimiento = DefaultIdAutorizacionReconocimiento,
                AutorizacionReconocimiento = DefaultAutorizacionReconocimiento,
                NumeroRvoe = cert.NumeroRvoe,
                AlumnoCurp = cert.Curp ?? "",
                AlumnoNombre = cert.Nombre,
                AlumnoPrimerApellido = cert.PrimerApellido,
                AlumnoSegundoApellido = cert.SegundoApellido,
                CorreoElectronico = correo,
                FechaExpedicion = cert.FechaExpedicion,
                IdModalidadTitulacion = DefaultIdModalidadTitulacion,
                ModalidadTitulacion = DefaultModalidadTitulacion,
                CumplioServicioSocial = DefaultCumplioServicioSocial,
                IdFundamentoLegalServicioSocial = DefaultIdFundamentoLegalServicioSocial,
                FundamentoLegalServicioSocial = DefaultFundamentoLegalServicioSocial,
                IdEntidadFederativa = string.IsNullOrWhiteSpace(cert.IdLugarExpedicion) ? config.IdEntidadFederativa : cert.IdLugarExpedicion,
                EntidadFederativa = cert.LugarExpedicion ?? config.EntidadFederativa ?? "",
                AntInstitucionProcedencia = DefaultInstitucionProcedencia,
                AntIdTipoEstudio = DefaultIdTipoEstudioAntecedente,
                AntTipoEstudio = DefaultTipoEstudioAntecedente,
                AntIdEntidadFederativa = config.IdEntidadFederativa,
                AntEntidadFederativa = config.EntidadFederativa ?? "",
                AntFechaTerminacion = cert.FechaExpedicionRvoe,
                Sello = "",
                CertificadoResponsable = certificadoB64,
                NoCertificadoResponsable = noCertificado
            };

            var cadena = GenerarCadenaOriginal(datos);

            if (!string.IsNullOrWhiteSpace(responsable.RutaLlavePrivadaKey)
                && System.IO.File.Exists(responsable.RutaLlavePrivadaKey)
                && !string.IsNullOrEmpty(responsable.PasswordLlavePrivada))
            {
                var keyBytes = await System.IO.File.ReadAllBytesAsync(responsable.RutaLlavePrivadaKey, ct);
                datos.Sello = FirmaDigitalSep.GenerarSello(cadena, keyBytes, responsable.PasswordLlavePrivada);
            }

            return ConstruirXml(datos);
        }

        private static string F(DateTime d) => d.ToString("yyyy-MM-dd");

        public string GenerarCadenaOriginal(TituloData d)
        {
            var sb = new StringBuilder("||");
            void A(string? v) => sb.Append(v ?? "").Append('|');

            A(d.Version);
            A(d.FolioControl);
            A(d.FirmaCurp); A(d.FirmaIdCargo.ToString()); A(d.FirmaCargo); A(d.AbrTitulo);
            A(d.CveInstitucion); A(d.NombreInstitucion);
            A(d.CveCarrera); A(d.NombreCarrera); A(d.FechaInicioCarrera.HasValue ? F(d.FechaInicioCarrera.Value) : "");
            A(F(d.FechaTerminacionCarrera)); A(d.IdAutorizacionReconocimiento.ToString()); A(d.AutorizacionReconocimiento); A(d.NumeroRvoe);
            A(d.AlumnoCurp); A(d.AlumnoNombre); A(d.AlumnoPrimerApellido); A(d.AlumnoSegundoApellido); A(d.CorreoElectronico);
            A(F(d.FechaExpedicion)); A(d.IdModalidadTitulacion.ToString()); A(d.ModalidadTitulacion);
            A(d.FechaExamenProfesional.HasValue ? F(d.FechaExamenProfesional.Value) : "");
            A(d.FechaExencionExamenProfesional.HasValue ? F(d.FechaExencionExamenProfesional.Value) : "");
            A(d.CumplioServicioSocial.ToString()); A(d.IdFundamentoLegalServicioSocial.ToString()); A(d.FundamentoLegalServicioSocial);
            A(d.IdEntidadFederativa); A(d.EntidadFederativa);
            A(d.AntInstitucionProcedencia); A(d.AntIdTipoEstudio.ToString()); A(d.AntTipoEstudio);
            A(d.AntIdEntidadFederativa); A(d.AntEntidadFederativa);
            A(d.AntFechaInicio.HasValue ? F(d.AntFechaInicio.Value) : "");
            A(F(d.AntFechaTerminacion)); A(d.AntNoCedula);

            sb.Append('|');
            return sb.ToString();
        }

        private static string ConstruirXml(TituloData d)
        {
            var settings = new XmlWriterSettings { Indent = true, Encoding = new UTF8Encoding(false), OmitXmlDeclaration = false };
            using var ms = new MemoryStream();
            using (var w = XmlWriter.Create(ms, settings))
            {
                w.WriteStartDocument();
                w.WriteStartElement("TituloElectronico", Ns);
                w.WriteAttributeString("xsi", "schemaLocation", "http://www.w3.org/2001/XMLSchema-instance", $"{Ns} {Ns}TitulosElectronicos.xsd");
                w.WriteAttributeString("version", d.Version);
                w.WriteAttributeString("folioControl", d.FolioControl);

                w.WriteStartElement("FirmaResponsables", Ns);
                w.WriteStartElement("FirmaResponsable", Ns);
                w.WriteAttributeString("nombre", d.FirmaNombre);
                w.WriteAttributeString("primerApellido", d.FirmaPrimerApellido);
                if (!string.IsNullOrWhiteSpace(d.FirmaSegundoApellido)) w.WriteAttributeString("segundoApellido", d.FirmaSegundoApellido);
                w.WriteAttributeString("curp", d.FirmaCurp);
                w.WriteAttributeString("idCargo", d.FirmaIdCargo.ToString());
                w.WriteAttributeString("cargo", d.FirmaCargo);
                if (!string.IsNullOrWhiteSpace(d.AbrTitulo)) w.WriteAttributeString("abrTitulo", d.AbrTitulo);
                w.WriteAttributeString("sello", d.Sello);
                w.WriteAttributeString("certificadoResponsable", d.CertificadoResponsable);
                w.WriteAttributeString("noCertificadoResponsable", d.NoCertificadoResponsable);
                w.WriteEndElement();
                w.WriteEndElement();

                w.WriteStartElement("Institucion", Ns);
                w.WriteAttributeString("cveInstitucion", d.CveInstitucion);
                w.WriteAttributeString("nombreInstitucion", d.NombreInstitucion);
                w.WriteEndElement();

                w.WriteStartElement("Carrera", Ns);
                w.WriteAttributeString("cveCarrera", d.CveCarrera);
                w.WriteAttributeString("nombreCarrera", d.NombreCarrera);
                if (d.FechaInicioCarrera.HasValue) w.WriteAttributeString("fechaInicio", F(d.FechaInicioCarrera.Value));
                w.WriteAttributeString("fechaTerminacion", F(d.FechaTerminacionCarrera));
                w.WriteAttributeString("idAutorizacionReconocimiento", d.IdAutorizacionReconocimiento.ToString());
                w.WriteAttributeString("autorizacionReconocimiento", d.AutorizacionReconocimiento);
                if (!string.IsNullOrWhiteSpace(d.NumeroRvoe)) w.WriteAttributeString("numeroRvoe", d.NumeroRvoe);
                w.WriteEndElement();

                w.WriteStartElement("Profesionista", Ns);
                w.WriteAttributeString("curp", d.AlumnoCurp);
                w.WriteAttributeString("nombre", d.AlumnoNombre);
                w.WriteAttributeString("primerApellido", d.AlumnoPrimerApellido);
                if (!string.IsNullOrWhiteSpace(d.AlumnoSegundoApellido)) w.WriteAttributeString("segundoApellido", d.AlumnoSegundoApellido);
                w.WriteAttributeString("correoElectronico", d.CorreoElectronico);
                w.WriteEndElement();

                w.WriteStartElement("Expedicion", Ns);
                w.WriteAttributeString("fechaExpedicion", F(d.FechaExpedicion));
                w.WriteAttributeString("idModalidadTitulacion", d.IdModalidadTitulacion.ToString());
                w.WriteAttributeString("modalidadTitulacion", d.ModalidadTitulacion);
                if (d.FechaExamenProfesional.HasValue) w.WriteAttributeString("fechaExamenProfesional", F(d.FechaExamenProfesional.Value));
                if (d.FechaExencionExamenProfesional.HasValue) w.WriteAttributeString("fechaExencionExamenProfesional", F(d.FechaExencionExamenProfesional.Value));
                w.WriteAttributeString("cumplioServicioSocial", d.CumplioServicioSocial.ToString());
                w.WriteAttributeString("idFundamentoLegalServicioSocial", d.IdFundamentoLegalServicioSocial.ToString());
                w.WriteAttributeString("fundamentoLegalServicioSocial", d.FundamentoLegalServicioSocial);
                w.WriteAttributeString("idEntidadFederativa", d.IdEntidadFederativa);
                w.WriteAttributeString("entidadFederativa", d.EntidadFederativa);
                w.WriteEndElement();

                w.WriteStartElement("Antecedente", Ns);
                w.WriteAttributeString("institucionProcedencia", d.AntInstitucionProcedencia);
                w.WriteAttributeString("idTipoEstudioAntecedente", d.AntIdTipoEstudio.ToString());
                w.WriteAttributeString("tipoEstudioAntecedente", d.AntTipoEstudio);
                w.WriteAttributeString("idEntidadFederativa", d.AntIdEntidadFederativa);
                if (!string.IsNullOrWhiteSpace(d.AntEntidadFederativa)) w.WriteAttributeString("entidadFederativa", d.AntEntidadFederativa);
                if (d.AntFechaInicio.HasValue) w.WriteAttributeString("fechaInicio", F(d.AntFechaInicio.Value));
                w.WriteAttributeString("fechaTerminacion", F(d.AntFechaTerminacion));
                if (!string.IsNullOrWhiteSpace(d.AntNoCedula)) w.WriteAttributeString("noCedula", d.AntNoCedula);
                w.WriteEndElement();

                w.WriteEndElement();
                w.WriteEndDocument();
            }
            return Encoding.UTF8.GetString(ms.ToArray());
        }
    }

    public class TituloData
    {
        public string Version = "1.0";
        public string FolioControl = "";
        public string FirmaNombre = "", FirmaPrimerApellido = "", FirmaCurp = "", FirmaCargo = "";
        public string? FirmaSegundoApellido, AbrTitulo;
        public int FirmaIdCargo;
        public string CveInstitucion = "", NombreInstitucion = "";
        public string CveCarrera = "", NombreCarrera = "";
        public DateTime? FechaInicioCarrera;
        public DateTime FechaTerminacionCarrera;
        public int IdAutorizacionReconocimiento;
        public string AutorizacionReconocimiento = "";
        public string NumeroRvoe = "";
        public string AlumnoCurp = "", AlumnoNombre = "", AlumnoPrimerApellido = "", CorreoElectronico = "";
        public string? AlumnoSegundoApellido;
        public DateTime FechaExpedicion;
        public int IdModalidadTitulacion;
        public string ModalidadTitulacion = "";
        public DateTime? FechaExamenProfesional, FechaExencionExamenProfesional;
        public int CumplioServicioSocial;
        public int IdFundamentoLegalServicioSocial;
        public string FundamentoLegalServicioSocial = "";
        public string IdEntidadFederativa = "", EntidadFederativa = "";
        public string AntInstitucionProcedencia = "";
        public int AntIdTipoEstudio;
        public string AntTipoEstudio = "";
        public string AntIdEntidadFederativa = "", AntEntidadFederativa = "";
        public DateTime? AntFechaInicio;
        public DateTime AntFechaTerminacion;
        public string? AntNoCedula;
        public string Sello = "", CertificadoResponsable = "", NoCertificadoResponsable = "";
    }
}
