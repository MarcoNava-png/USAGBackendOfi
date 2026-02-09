using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication2.Core.Enums
{
    public enum EstatusRecibo
    {
        PENDIENTE = 0, 
        PARCIAL = 1, 
        PAGADO = 2, 
        VENCIDO = 3, 
        CANCELADO = 4, 
        BONIFICADO = 5
    }
}
