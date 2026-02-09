using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication2.Configuration.Constants;



namespace WebApplication2.Configuration.Constants
{
    public static class ErrorConstants
    {
        public const string GET_DATA_FAILED = "Se ha producido un error al obtener los datos. Favor de intentarlo de nuevo más tarde. ";
        public const string ADD_FAILED = "Se ha producido un error al guardar los datos. Favor de intentarlo de nuevo más tarde. ";
        public const string UPDATE_FAILED = "Se ha producido un error al actualizar los datos. Favor de intentarlo de nuevo más tarde. ";
        public const string DELETE_FAILED = "Se ha producido un error al eliminar los datos. Favor de intentarlo de nuevo más tarde. ";
        public const string RECORD_NOTFOUND = "No se encontó con los parámetros proporcionados. ";
        public const string CLUB_MATCH_NOTFOUND = "No se encontó club en el partido con los parámetros proporcionados. ";
        public const string MATCH_NOTFOUND = "No se encontó partido con los parámetros proporcionados. ";
        public const string TOURNAMENT_NOTFOUND = "No se encontó torneo con los parámetros proporcionados. ";
        public const string RECORDS_NOTFOUND = "No se encontraron registros con los Ids proporcionados. ";
        public const string INVALID_CREDENTIALS = "Email o contraseña incorrectos. ";
        public const string GENERAL_EXCEPTION_MESSAGE = "Ocurrió un error al procesar la solicitud. Favor de intentarlo mas tarde. ";
        public const string RESOURCE_ALREADY_IN_DATABASE = "Ya existe un registro en la base de datos. ";
    }
}
