using ClinicalEpilepsyApp.Domain.DBModels;
using Microsoft.EntityFrameworkCore;

namespace ClinicalEpilepsyApp.Infrastructure.Context
{
	public class ClinicalEpilepsyAppDbContext : DbContext
	{
		public ClinicalEpilepsyAppDbContext(DbContextOptions<ClinicalEpilepsyAppDbContext> options) : base(options)
		{
		}

		public DbSet<Patient> Patients { get; set; }
		public DbSet<EcgAlarm> EcgAlarms { get; set; }
		public DbSet<EcgProcessedMeasurement> EcgProcessedMeasurements { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Patient>().HasKey(p => p.Id);
			modelBuilder.Entity<EcgAlarm>().HasKey(e => e.Id);
			modelBuilder.Entity<EcgProcessedMeasurement>().HasKey(e => e.ProcessedMeasurementId);
        }

		
	}
}
