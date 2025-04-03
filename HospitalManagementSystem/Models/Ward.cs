using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Models
{
    public class Ward
    {
        [Key]
        public int WardId { get; set; }

        [Required]
        public string WardName { get; set; }

        [Required]
        [Range(1, 1000, ErrorMessage = "Capacity must be between 1 and 1000.")]
        public int Capacity { get; set; }

        public int CurrentOccupancy { get; set; } = 0;

        public List<Patient>? Patients { get; set; }
        public List<Bed>? Beds { get; set; }

        public int AvailableBeds => Capacity - CurrentOccupancy; // Auto-calculate available beds
    }
}
