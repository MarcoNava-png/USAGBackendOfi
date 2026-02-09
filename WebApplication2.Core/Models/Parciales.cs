using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication2.Core.Models
{
    public class Parciales : BaseEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Orden {  get; set; }
    }
}
