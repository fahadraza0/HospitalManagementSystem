namespace HospitalManagementSystem.Models
{
    public class Billing
    {
        public int BillingId { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public int AppointmentId { get; set; }  // Added: Link to Appointment
        public DateTime CreatedAt { get; set; }

        // Billing details
        public decimal DoctorFee { get; set; }
        public decimal MedicineCost { get; set; }
        public decimal TotalAmount { get; set; }

        // Payment status
        public bool IsPaid { get; set; }

        // Timestamps for tracking
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public Patient Patient { get; set; }
        public Doctor Doctor { get; set; }
        public Appointment Appointment { get; set; }  // Added: Navigation to Appointment

        // New: Track whether the bill is paid or needs follow-up
        public string PaymentStatus => IsPaid ? "Paid" : "Pending";
        public ICollection<TreatmentMedicine> TreatmentMedicines { get; set; }  // Link to TreatmentMedicines
    }
}
