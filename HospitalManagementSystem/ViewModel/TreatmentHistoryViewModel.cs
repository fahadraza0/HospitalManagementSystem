using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.ViewModel
{
    public class TreatmentHistoryViewModel
    {
        public int? PatientId { get; set; }  // nullable because admin has no specific patient
        public string PatientName { get; set; }
        public List<TreatmentRecord> TreatmentRecords { get; set; }
    }
}
