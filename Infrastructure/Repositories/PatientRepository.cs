using ClinicalEpilepsyApp.Domain.DBModels;
using ClinicalEpilepsyApp.Domain.Models;
using ClinicalEpilepsyApp.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace ClinicalEpilepsyApp.Infrastructure.Repositories
{
	public class PatientRepository
	{
		private readonly ClinicalEpilepsyAppDbContext _dbContext;

		public PatientRepository(ClinicalEpilepsyAppDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task<List<Patient>> GetAllPatientsAsync()
		{
			return await _dbContext.Patients.ToListAsync();
		}

		public async Task<Patient> GetPatientByIdAsync(string id)
		{
			return await _dbContext.Patients.FindAsync(id) ?? throw new InvalidOperationException();
		}

		public async Task AddPatientAsync(Patient patient)
		{
			_dbContext.Patients.Add(patient);
			await _dbContext.SaveChangesAsync();
		}
		public async Task UpdatePatientAsync(Patient patient)
		{
			_dbContext.Entry(patient).State = EntityState.Modified;
			await _dbContext.SaveChangesAsync();
		}

		public async Task DeletePatientAsync(string id)
        {
            var patient = await _dbContext.Patients.FindAsync(id);
            if (patient == null)
            {
                throw new InvalidOperationException();
            }

            _dbContext.Patients.Remove(patient);
            await _dbContext.SaveChangesAsync();
        }
	}
}
