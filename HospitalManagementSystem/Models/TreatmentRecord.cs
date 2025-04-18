using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Models
{
    public class TreatmentRecord
    {
        [Key]
        public int RecordId { get; set; }

        public int PatientId { get; set; }
        [ForeignKey("PatientId")]
        public Patient Patient { get; set; }

        public int DoctorId { get; set; }
        [ForeignKey("DoctorId")]
        public Doctor Doctor { get; set; }

        [Required]
        public string Diagnosis { get; set; }

        public string Prescriptions { get; set; }

        public DateTime StartTime { get; set; }  // 🕒 Treatment Start Time
        public DateTime EndTime { get; set; }    // 🕒 Treatment End Time

        [NotMapped]  // Exclude from DB, used for calculation only
        public TimeSpan Duration => EndTime - StartTime;

        [Column(TypeName = "decimal(18,2)")]
        public decimal DoctorFee { get; set; }   // 💵 Doctor's Fee based on duration

        public DateTime TreatmentDate { get; set; } = DateTime.Now;

        public bool IsFinalTreatment { get; set; } = false; // Marks last treatment before discharge

        // 🩺 Medicines prescribed in the treatment (many-to-many relationship)
        public ICollection<TreatmentMedicine> TreatmentMedicines { get; set; } = new List<TreatmentMedicine>();

        // 💰 Associated Bill
        public int? BillId { get; set; }
        [ForeignKey("BillId")]
        public Billing Billing { get; set; }
    }
}
