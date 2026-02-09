namespace WebApplication2.Core.DTOs
{
    public class ProgramaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int Nivel { get; set; }
        public DepartamentoDto Departamento { get; set; }
    }
}
