namespace HospitalManagementSystem.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public int BillId { get; set; }
        public decimal AmountPaid { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.Now;   
        public string PaymentMethod { get; set; } // e.g., Cash, Credit Card
        public bool IsConfirmed { get; set; }

        public Billing Billing { get; set; }
    }
}
