using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Models
{
    public class TreatmentMedicine
    {
        public int Id { get; set; }
        public int TreatmentId { get; set; }
        public int MedicineId { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }
        public decimal TotalCost => Quantity * (Medicine?.Price ?? 0);

        public TreatmentRecord Treatment { get; set; }
        public Medicine Medicine { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }
}
