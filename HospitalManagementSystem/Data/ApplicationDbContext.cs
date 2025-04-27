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
        public DbSet<Billing> Billings { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Medicine> Medicines { get; set; }
        public DbSet<TreatmentMedicine> TreatmentMedicines { get; set; }

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

            // Configuring the composite key for TreatmentMedicine
            modelBuilder.Entity<TreatmentMedicine>()
                .HasKey(tm => new { tm.TreatmentId, tm.MedicineId }); // Composite primary key

            // Configuring the relationship between TreatmentMedicine and Treatment
            modelBuilder.Entity<TreatmentMedicine>()
                .HasOne(tm => tm.Treatment)
                .WithMany(t => t.TreatmentMedicines) // A Treatment has many TreatmentMedicines
                .HasForeignKey(tm => tm.TreatmentId)
                .OnDelete(DeleteBehavior.Cascade); // Optional: Cascade delete for Treatment

            // Configuring the relationship between TreatmentMedicine and Medicine
            modelBuilder.Entity<TreatmentMedicine>()
                .HasOne(tm => tm.Medicine)
                .WithMany() // Assuming Medicine does not have a navigation property for TreatmentMedicines
                .HasForeignKey(tm => tm.MedicineId)
                .OnDelete(DeleteBehavior.Restrict); // Optional: Restrict delete for Medicine

            // Billing and Doctor relationship (no cascade delete)
            modelBuilder.Entity<Billing>()
                .HasOne(b => b.Doctor)
                .WithMany()
                .HasForeignKey(b => b.DoctorId)
                .OnDelete(DeleteBehavior.Restrict); // Avoid cascading delete for Doctor

            // Billing and Patient relationship (no cascade delete)
            modelBuilder.Entity<Billing>()
                .HasOne(b => b.Patient)
                .WithMany()
                .HasForeignKey(b => b.PatientId)
                .OnDelete(DeleteBehavior.NoAction); // Change this to explicitly use NoAction // Avoid cascading delete for Patient

            // Staff and Ward relationship (no cascading delete)
            modelBuilder.Entity<Staff>() // Correctly specifying Staff entity
                .HasOne(s => s.AssignedWard)
                .WithMany() // Assuming Ward does not have a navigation property for Staff
                .HasForeignKey(s => s.AssignedWardId)
                .OnDelete(DeleteBehavior.SetNull); // Optional: Set AssignedWardId to null when Ward is deleted

            // You can add other relationship configurations here if needed
        }
    }
}