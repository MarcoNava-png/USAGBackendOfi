namespace WebApplication2.Configuration.Constants
{
    public class Rol
    {

        public const string SUPER_ADMIN = "superadmin";

        public const string ADMIN = "admin";
        public const string DIRECTOR = "director";
        public const string COORDINADOR = "coordinador";
        public const string DOCENTE = "docente";
        public const string ALUMNO = "alumno";
        public const string CONTROL_ESCOLAR = "controlescolar";
        public const string FINANZAS = "finanzas";
        public const string ADMISIONES = "admisiones";


        public const string ROLES_ADMINISTRACION = $"{SUPER_ADMIN},{ADMIN}";


        public const string ROLES_CAJA = $"{SUPER_ADMIN},{ADMIN},{CONTROL_ESCOLAR},{FINANZAS}";

        public const string ROLES_ADMISIONES = $"{SUPER_ADMIN},{ADMIN},{CONTROL_ESCOLAR},{ADMISIONES},{DIRECTOR}";

        public const string ROLES_REPORTES_FINANCIEROS = $"{SUPER_ADMIN},{ADMIN},{DIRECTOR},{FINANZAS},{CONTROL_ESCOLAR}";

        public const string ROLES_CONFIGURACION = $"{SUPER_ADMIN},{ADMIN}";

        public const string SOLO_SUPER_ADMIN = SUPER_ADMIN;
    }
}
