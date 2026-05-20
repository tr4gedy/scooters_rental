using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prct2.Models
{
    public class Users
    {
        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Phone { get; set; }
        public DateTime CreatedAt { get; set; }

        // Навигация
        public virtual ICollection<Trips> Trips { get; set; } = new List<Trips>();
    }
}
