using ClinicalEpilepsyApp.Infrastructure.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClinicalEpilepsyApp.Domain.Models;
using ClinicalEpilepsyApp.Domain.DBModels;
using Microsoft.EntityFrameworkCore;

namespace ClinicalEpilepsyApp.Infrastructure.Repositories
{
    public class EcgProcessedMeasurementRepository
    {
        private readonly ClinicalEpilepsyAppDbContext _dbContext;

        public EcgProcessedMeasurementRepository(ClinicalEpilepsyAppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<EcgProcessedMeasurement>> GetAllProcessedMeasurementsAsync()
        {
            return await _dbContext.EcgProcessedMeasurements
                .Include(e => e.Patient) // Include related patient data if needed
                .ToListAsync();
        }

        public async Task<EcgProcessedMeasurement> GetProcessedMeasurementByIdAsync(Guid id)
        {
            return await _dbContext.EcgProcessedMeasurements
                .Include(e => e.Patient) // Include related patient data if needed
                .FirstOrDefaultAsync(e => e.ProcessedMeasurementId == id);
        }

    }
}
