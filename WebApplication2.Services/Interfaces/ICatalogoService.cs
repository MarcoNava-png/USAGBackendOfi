using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication2.Core.Models;

namespace WebApplication2.Services.Interfaces
{
    public interface ICatalogoService 
    {
        Task<IEnumerable<AspiranteEstatus>> GetEstatusAspirante();
    }
}
