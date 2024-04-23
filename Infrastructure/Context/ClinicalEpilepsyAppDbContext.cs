using ClinicalEpilepsyApp.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicalEpilepsyApp.Infrastructure.Context
{
	public class ClinicalEpilepsyAppDbContext : DbContext
	{
		public ClinicalEpilepsyAppDbContext(DbContextOptions<ClinicalEpilepsyAppDbContext> options) : base(options)
		{
		}

		public DbSet<Patient> Patients { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Patient>().HasKey(p => p.Id);
		}

		
	}
}
