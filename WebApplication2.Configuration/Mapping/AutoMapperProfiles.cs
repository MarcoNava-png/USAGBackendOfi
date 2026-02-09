using AutoMapper;
using WebApplication2.Configuration.Mapping.Profiles;
using WebApplication2.Core.DTOs;

namespace WebApplication2.Configuration.Mapping
{
    public static class AutoMapperProfiles
    {
        public static List<Profile> GetProfiles()
        {
            return new List<Profile>
            {
                new UserProfile(),
                new AspiranteProfile(),
                new PersonaProfile(),
                new CoordinadorProfile(),
                new DirectorProfile(),
                new EstudianteProfile(),
                new ProfesorProfile(),
                new DepartamentoProfile(),
                new AspiranteProgramaProfile(),
                new ProgramaProfile(),
                new GrupoProfile(),
                new PlanEstudioProfile(),
                new InscripcionProfile(),
                new PeriodoAcademicoProfile(),
                new GrupoMateriaProfile(),
                new CampusProfile(),
                new ApplicationUserProfile(),
                new AspiranteBitacoraProfile(),
                new MateriaProfile(),
                new ParcialesProfile(),
                new CalificacionesProfile(),
                new CalificacionDetalleProfile(),
                new AspiranteProfile(),
                new DocumentosProfile(),
                new ConceptoPrecioProfile(),
                new ReciboProfile(),
                new PagoProfile(),
                new BitacoraProfile(),
                new PlantillaCobroProfile()
            };
        }
    }
}
