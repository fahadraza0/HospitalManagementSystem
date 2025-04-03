using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagementSystem.Models
{
    public class TreatmentRecord
    {
        [Key]
        public int RecordId { get; set; }

        // ✅ Foreign key linking to the patient
        public int PatientId { get; set; }
        [ForeignKey("PatientId")]
        public Patient Patient { get; set; }

        // ✅ Foreign key linking to the doctor
        public int DoctorId { get; set; }
        [ForeignKey("DoctorId")]
        public Doctor Doctor { get; set; }

        // ✅ Foreign key linking to the billing record
        //public int? BillingId { get; set; }
        //[ForeignKey("BillingId")]
        //public Billing? Billing { get; set; }

        [Required]
        [MaxLength(500)]
        public string Diagnosis { get; set; }

        // ✅ Treatment notes for additional comments or observations
        [MaxLength(1000)]
        public string? TreatmentNotes { get; set; }

        public DateTime TreatmentDate { get; set; } = DateTime.Now;

        public bool IsFinalTreatment { get; set; } = false; // Marks last treatment before discharge

        // ✅ List of prescribed medicines
        public List<TreatmentMedicine> TreatmentMedicines { get; set; } = new List<TreatmentMedicine>();

        // ✅ Calculates the total cost of medicines in this treatment
        [NotMapped]
        public decimal TotalCost => TreatmentMedicines?.Sum(tm => tm.TotalCost) ?? 0m;
    }
}
