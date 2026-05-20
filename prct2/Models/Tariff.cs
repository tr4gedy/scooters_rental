using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prct2.Models
{
    public class Tariff
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal PricePerMinute { get; set; }
        public string? Description { get; set; }

        // Навигация
        public virtual ICollection<Trips> Trips { get; set; } = new List<Trips>();
    }
}
