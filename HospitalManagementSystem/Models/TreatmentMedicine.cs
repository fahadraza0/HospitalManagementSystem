namespace HospitalManagementSystem.Models
{
    public class TreatmentMedicine
    {
        public int Id { get; set; }
        public int TreatmentId { get; set; }
        public int MedicineId { get; set; }
        public int Quantity { get; set; }
        public decimal TotalCost => Quantity * Medicine.Price;

        public TreatmentRecord Treatment { get; set; }
        public Medicine Medicine { get; set; }
    }
}
