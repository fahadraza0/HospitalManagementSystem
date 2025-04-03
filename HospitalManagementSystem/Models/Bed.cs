using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagementSystem.Models
{
    public class Bed
    {
        [Key]
        public int BedId { get; set; }

        public int WardId { get; set; }
        [ForeignKey("WardId")]
        public Ward Ward { get; set; }

        public bool IsOccupied { get; set; } = false;
    }
}
