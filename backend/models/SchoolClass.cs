using System.ComponentModel.DataAnnotations;

namespace SchoolBackend.Models
{
    public class SchoolClass
    {
        [Key]
        public string Id { get; set; } = "c_" + Guid.NewGuid().ToString("N").Substring(0, 6);
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public int TotalSeats { get; set; }
        
        public ICollection<SeatHold> SeatHolds { get; set; } = new List<SeatHold>();
    }
}