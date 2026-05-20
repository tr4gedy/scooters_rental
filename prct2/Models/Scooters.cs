namespace prct2.Models
{
    public class Scooters
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Charge { get; set; }
        public string Status { get; set; }
        public string Location { get; set; }
        public virtual ICollection<Trips> Trips { get; set; } = new List<Trips>();
    }
}