using WebApplication2.Core.Models;

namespace WebApplication2.Services.Interfaces
{
    public interface IDocumentoRequisitoService
    {
        Task<List<DocumentoRequisito>> GetAll();
        Task<DocumentoRequisito?> GetById(int id);
        Task<DocumentoRequisito> Crear(DocumentoRequisito documento);
        Task<DocumentoRequisito> Actualizar(DocumentoRequisito documento);
        Task<bool> Eliminar(int id);
        Task<DocumentoRequisito> ToggleActivo(int id);
    }
}
