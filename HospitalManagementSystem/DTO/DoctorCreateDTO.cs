using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.DTO
{
    public class DoctorCreateDTO
    {
        [Required]
        public string FullName { get; set; }

        [Required]
        public string Specialization { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        public int? TeamId { get; set; }

        [Required]
        [Range(0, 50)]
        public int Experience { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Hourly rate must be a positive number.")]
        public decimal HourlyRate { get; set; }
    }
}
