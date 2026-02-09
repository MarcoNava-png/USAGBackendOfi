using Microsoft.EntityFrameworkCore;
using WebApplication2.Core.Models;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Services
{
    public class UbicacionService : IUbicacionService
    {
        private readonly ApplicationDbContext _dbContext;

        public UbicacionService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Estado>> ObtenerEstados()
        {
            return await _dbContext.Estados.ToListAsync();
        }

        public async Task<IEnumerable<Municipio>> ObtenerMunicipios(string estadoId)
        {
            return await _dbContext.Municipios.Where(m => m.EstadoId == estadoId).ToListAsync();
        }

        public async Task<IEnumerable<CodigoPostal>> ObtenerCodigosPostales(string municipioId)
        {
            return await _dbContext.CodigosPostales.Where(cp => cp.MunicipioId == municipioId).ToListAsync();
        }
    }
}
