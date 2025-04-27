using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.ViewModel
{
    public class TreatmentRecordDetailsViewModel
    {
        public TreatmentRecord? Treatment { get; set; }
        public List<TreatmentMedicine>? Medicines { get; set; }
    }
}
