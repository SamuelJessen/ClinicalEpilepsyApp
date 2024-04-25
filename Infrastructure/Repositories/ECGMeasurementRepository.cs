using ClinicalEpilepsyApp.Infrastructure.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClinicalEpilepsyApp.Domain.Models;

namespace ClinicalEpilepsyApp.Infrastructure.Repositories
{
    public class ECGMeasurementRepository
    {
        private readonly ClinicalEpilepsyAppDbContext _dbContext;

        public ECGMeasurementRepository(ClinicalEpilepsyAppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        
    }
}
