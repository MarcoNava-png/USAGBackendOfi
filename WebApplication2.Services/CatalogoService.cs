using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication2.Core.Models;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;

namespace WebApplication2
{
    public class CatalogoService : ICatalogoService
    {
        private readonly ApplicationDbContext _dbContext;

        public CatalogoService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<IEnumerable<AspiranteEstatus>> GetEstatusAspirante()
        {
            var aspiranteEstatus = await _dbContext.AspiranteEstatus
                .ToListAsync();

            if (aspiranteEstatus == null)
            {
                throw new Exception("No se ha encontrado Estatus.");
            }

            return aspiranteEstatus;
        }
    }
}
