using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication2.Core.Common;
using WebApplication2.Core.Models;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Services
{
    public class ParcialesService : IParcialesService
    {
        private readonly ApplicationDbContext _dbContext;

        public ParcialesService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PagedResult<Parciales>> GetParciales(int page, int pageSize)
        {
            var totalItems = await _dbContext.Parciales
                .Where(d => d.Status == Core.Enums.StatusEnum.Active)
                .CountAsync();

            var items = await _dbContext.Parciales
                .Where(d => d.Status == Core.Enums.StatusEnum.Active)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Parciales>
            {
                TotalItems = totalItems,
                Items = items,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        public async Task<Parciales> CrearParciales(Parciales parciales)
        {
            await _dbContext.Parciales.AddAsync(parciales);
            await _dbContext.SaveChangesAsync();

            return parciales;
        }

        public async Task<Parciales> ActualizarParciales(Parciales newParcial)
        {
            var parciales = await _dbContext.Parciales
                .FirstOrDefaultAsync(e => e.Id == newParcial.Id);

            if (parciales == null)
            {
                throw new Exception("No existe Parcial con el id ingresado");
            }

            parciales.Name  = newParcial.Name;
            parciales.Orden = newParcial.Orden;


            _dbContext.Parciales.Update(parciales);

            await _dbContext.SaveChangesAsync();

            return parciales;
        }
    }
}
