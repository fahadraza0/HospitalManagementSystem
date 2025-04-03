using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Models
{
    public class Team
    {
        [Key]
        public int TeamId { get; set; }

        [Required]
        public string TeamName { get; set; }

        public List<Doctor>? Doctors { get; set; }
    }
}
