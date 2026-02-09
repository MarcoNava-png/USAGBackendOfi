namespace WebApplication2.Services.Interfaces
{
    public interface IMatriculaService
    {
        Task<string> GenerarMatriculaAsync(string nombrePlanEstudios);
        string ObtenerPrefijo(string nombrePlanEstudios);
        bool ValidarFormatoMatricula(string matricula);
        Task<bool> ExisteMatriculaAsync(string matricula);
    }
}
