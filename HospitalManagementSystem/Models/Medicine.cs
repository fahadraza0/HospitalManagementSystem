using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Models
{
    public class Medicine
    {
        public int MedicineId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public decimal Price { get; set; }
    }
}
