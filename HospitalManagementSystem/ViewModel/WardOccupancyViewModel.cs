namespace HospitalManagementSystem.ViewModel
{
    public class WardOccupancyViewModel
    {
        public string WardName { get; set; }
        public int TotalBeds { get; set; }
        public int OccupiedBeds { get; set; }
        public int AvailableBeds { get; set; }
    }
}
