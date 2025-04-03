using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.ViewModel
{
    public class AppointmentViewModel
    {
        public int AppointmentId { get; set; }

        [Required]
        public int DoctorId { get; set; }

        public int PatientId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime AppointmentDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        public bool IsEmergency { get; set; }
        public string Status { get; set; }
    }
}
