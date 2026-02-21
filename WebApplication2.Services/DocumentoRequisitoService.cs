using Microsoft.EntityFrameworkCore;
using WebApplication2.Core.Enums;
using WebApplication2.Core.Models;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Services
{
    public class DocumentoRequisitoService : IDocumentoRequisitoService
    {
        private readonly ApplicationDbContext _dbContext;

        public DocumentoRequisitoService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<DocumentoRequisito>> GetAll()
        {
            return await _dbContext.DocumentoRequisito
                .Where(d => d.Status == StatusEnum.Active)
                .OrderBy(d => d.Orden)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<DocumentoRequisito?> GetById(int id)
        {
            return await _dbContext.DocumentoRequisito
                .FirstOrDefaultAsync(d => d.IdDocumentoRequisito == id
                    && d.Status == StatusEnum.Active);
        }

        public async Task<DocumentoRequisito> Crear(DocumentoRequisito documento)
        {
            var existe = await _dbContext.DocumentoRequisito
                .AnyAsync(d => d.Clave.ToLower() == documento.Clave.ToLower()
                    && d.Status == StatusEnum.Active);

            if (existe)
                throw new Exception("Ya existe un documento requisito con esa clave");

            documento.Activo = true;
            documento.Status = StatusEnum.Active;

            await _dbContext.DocumentoRequisito.AddAsync(documento);
            await _dbContext.SaveChangesAsync();

            return documento;
        }

        public async Task<DocumentoRequisito> Actualizar(DocumentoRequisito newDoc)
        {
            var doc = await _dbContext.DocumentoRequisito
                .FirstOrDefaultAsync(d => d.IdDocumentoRequisito == newDoc.IdDocumentoRequisito
                    && d.Status == StatusEnum.Active);

            if (doc == null)
                throw new Exception("No existe documento requisito con el id ingresado");

            var duplicado = await _dbContext.DocumentoRequisito
                .AnyAsync(d => d.Clave.ToLower() == newDoc.Clave.ToLower()
                    && d.IdDocumentoRequisito != newDoc.IdDocumentoRequisito
                    && d.Status == StatusEnum.Active);

            if (duplicado)
                throw new Exception("Ya existe otro documento requisito con esa clave");

            doc.Clave = newDoc.Clave;
            doc.Descripcion = newDoc.Descripcion;
            doc.EsObligatorio = newDoc.EsObligatorio;
            doc.Orden = newDoc.Orden;
            doc.Activo = newDoc.Activo;
            doc.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return doc;
        }

        public async Task<bool> Eliminar(int id)
        {
            var doc = await _dbContext.DocumentoRequisito
                .FirstOrDefaultAsync(d => d.IdDocumentoRequisito == id);

            if (doc == null)
                throw new Exception("No existe documento requisito con el id ingresado");

            var documentosAspirante = await _dbContext.AspiranteDocumento
                .CountAsync(ad => ad.IdDocumentoRequisito == id);

            if (documentosAspirante > 0)
                throw new Exception(
                    $"No se puede eliminar porque tiene {documentosAspirante} documento(s) de aspirante asociado(s).");

            var documentosPlan = await _dbContext.PlanDocumentoRequisito
                .CountAsync(pd => pd.IdDocumentoRequisito == id
                    && pd.Status == StatusEnum.Active);

            if (documentosPlan > 0)
                throw new Exception(
                    $"No se puede eliminar porque está asignado a {documentosPlan} plan(es) de estudio.");

            doc.Activo = false;
            doc.Status = StatusEnum.Deleted;
            doc.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<DocumentoRequisito> ToggleActivo(int id)
        {
            var doc = await _dbContext.DocumentoRequisito
                .FirstOrDefaultAsync(d => d.IdDocumentoRequisito == id
                    && d.Status == StatusEnum.Active);

            if (doc == null)
                throw new Exception("Documento requisito no encontrado");

            doc.Activo = !doc.Activo;
            doc.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            return doc;
        }
    }
}
