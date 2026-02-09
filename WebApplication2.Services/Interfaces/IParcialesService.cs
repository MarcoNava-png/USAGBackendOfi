using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication2.Core.Common;
using WebApplication2.Core.Models;

namespace WebApplication2.Services.Interfaces
{
    public interface IParcialesService 
    {
        Task<PagedResult<Parciales>> GetParciales(int page, int pageSize);
        Task<Parciales> CrearParciales(Parciales Parciales);
        Task<Parciales> ActualizarParciales(Parciales newParciales);
    }
}
