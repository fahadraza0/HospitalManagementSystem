using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagementSystem.Models
{
    public class Doctor
    {
        [Key]
        public int DoctorId { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(15)]
        public string PhoneNumber { get; set; }  // ✅ New field for contact info

        [Required]
        [StringLength(100)]
        public string Specialization { get; set; }

        public bool IsAvailable { get; set; } = true;

        public int? TeamId { get; set; }

        [ForeignKey("TeamId")]
        public Team? Team { get; set; }

        public int AssignedPatientsCount { get; set; } = 0;  // Tracks workload

        // ✅ Doctor's hourly rate for billing
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal HourlyRate { get; set; }  // Renamed for consistency

        // ✅ New field: Experience in years
        [Range(0, 50)]
        public int Experience { get; set; } = 0;

        // ✅ Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // ✅ Soft delete or disable functionality
        public bool IsActive { get; set; } = true;

        // ✅ Navigation properties
        public ICollection<Patient>? Patients { get; set; }
        public ICollection<TreatmentRecord>? TreatmentRecords { get; set; }
        public ICollection<Team>? Teams { get; set; }
        public ICollection<DoctorSchedule>? Schedules { get; set; }
        //public ICollection<Billing>? Billings { get; set; }
        public ICollection<TreatmentMedicine>? TreatmentMedicines { get; set; }
    }
}
