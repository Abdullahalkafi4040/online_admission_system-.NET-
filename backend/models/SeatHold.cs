using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolBackend.Models
{
    public class SeatHold
    {
        [Key]
        public string HoldId { get; set; } = "h_" + Guid.NewGuid().ToString("N").Substring(0, 6);
        
        [Required]
        public string SchoolClassId { get; set; } = string.Empty; 
        
        [ForeignKey("SchoolClassId")]
        public SchoolClass? SchoolClass { get; set; }
        
        [Required]
        public string StudentNid { get; set; } = string.Empty; 
        
        [Required]
        public string Status { get; set; } = "HELD"; 
        
        public DateTime ExpiresAt { get; set; }
    }
}