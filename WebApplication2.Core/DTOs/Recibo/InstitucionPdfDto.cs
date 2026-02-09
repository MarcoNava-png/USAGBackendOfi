namespace WebApplication2.Core.DTOs.Recibo
{
    public class InstitucionPdfDto
    {
        public string Nombre { get; set; } = "UNIVERSIDAD SAN ANDRÉS DE GUANAJUATO";
        public string Campus { get; set; } = "CAMPUS LEÓN";
        public string Direccion { get; set; } = "República de Cuba #201 Col. Bellavista León, Gto.";
        public string? Telefono { get; set; } = "Tel: (477) 123-4567";
        public string? Email { get; set; } = "cobranza@usag.edu.mx";
        public string? RFC { get; set; }
    }
}
