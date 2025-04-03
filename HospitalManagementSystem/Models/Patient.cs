using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Models
{
    public class Patient
    {
        [Key]
        public int PatientId { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required, DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [StringLength(10)]
        public string Gender { get; set; }

        [Required]
        [StringLength(200)]
        public string Address { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        [StringLength(500)]
        public string? MedicalHistory { get; set; }

        public int? WardId { get; set; }

        [ForeignKey("WardId")]
        public Ward? Ward { get; set; }

        public int? BedId { get; set; }

        [ForeignKey("BedId")]
        public Bed? Bed { get; set; }

        public int? AssignedDoctorId { get; set; }

        [ForeignKey("AssignedDoctorId")]
        public Doctor? AssignedDoctor { get; set; }

        [Required]
        public string Status { get; set; } = "Admitted"; // Default status

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // ✅ File Upload (Document Path)
        public string? DocumentPath { get; set; }

        // ✅ User Registration Fields
        [Required, EmailAddress]
        public string? Email { get; set; }

        [Required, MinLength(6)]
        public string? Password { get; set; }

        // ✅ Identity User ID for Patient Login
        public string? UserId { get; set; }

        public DateTime? DischargeDate { get; set; }
    }
}
