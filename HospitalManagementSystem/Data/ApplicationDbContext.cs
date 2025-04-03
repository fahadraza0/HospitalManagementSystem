using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Staff> Staff { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Ward> Wards { get; set; }
        public DbSet<TreatmentRecord> TreatmentRecords { get; set; }
        public DbSet<Bed> Beds { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<DoctorSchedule> DoctorSchedules { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        //public DbSet<Billing> Billings { get; set; }
        //public DbSet<Payment> Payments { get; set; }
        //public DbSet<Medicine> Medicines { get; set; }
        //public DbSet<TreatmentMedicine> TreatmentMedicines { get; set; }


        // Configure the model relationships in OnModelCreating
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuring the relationship between Doctor and Team
            modelBuilder.Entity<Doctor>()
                .HasOne(d => d.Team) // A Doctor has one Team
                .WithMany(t => t.Doctors) // A Team has many Doctors
                .HasForeignKey(d => d.TeamId) // Foreign key on Doctor's TeamId
                .OnDelete(DeleteBehavior.SetNull); // Optional: Set TeamId to null when the team is deleted

            modelBuilder.Entity<TreatmentMedicine>()
            .HasKey(tm => new { tm.TreatmentId, tm.MedicineId });

            modelBuilder.Entity<TreatmentMedicine>()
                .HasOne(tm => tm.Medicine)
                .WithMany()
                .HasForeignKey(tm => tm.MedicineId);

            // You can add other relationship configurations here if needed

        }
    }
}