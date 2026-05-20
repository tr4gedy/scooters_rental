using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prct2.Models
{
    public class Payments
    {
        public int Id { get; set; }
        public int TripId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentStatus { get; set; } = "pending"; // pending, completed, failed
        public DateTime PaymentDate { get; set; }

        public virtual Trips Trip { get; set; } = null!;
    }

}
