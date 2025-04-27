namespace HospitalManagementSystem.ViewModel
{
    public class PatientDashboardViewModel
    {
        public string FullName { get; set; }
        public string DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }

        public string Status { get; set; }
        public string CreatedAt { get; set; }
        public string DischargeDate { get; set; }

        public string WardName { get; set; }
        public int BedNumber { get; set; }

        public string AssignedDoctorName { get; set; }

        public string MedicalHistory { get; set; }
    }
}
