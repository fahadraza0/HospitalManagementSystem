using HospitalManagementSystem.Models;

namespace HospitalManagementSystem.ViewModel
{
    public class TreatmentRecordViewModel
    {
        public TreatmentRecord TreatmentRecord { get; set; }
        public List<Doctor> Doctors { get; set; }
        public List<Patient> Patients { get; set; }
        public List<Medicine> Medicines { get; set; }
        public List<MedicineSelection> MedicinesWithQuantities { get; set; }
    }
    public class MedicineSelection
    {
        public int MedicineId { get; set; }
        public int Quantity { get; set; }
    }
}
