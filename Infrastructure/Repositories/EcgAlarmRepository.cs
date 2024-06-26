﻿using ClinicalEpilepsyApp.Domain.DBModels;
using ClinicalEpilepsyApp.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace ClinicalEpilepsyApp.Infrastructure.Repositories
{
    public class EcgAlarmRepository
    {
        private readonly ClinicalEpilepsyAppDbContext _dbContext;

        public EcgAlarmRepository(ClinicalEpilepsyAppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<EcgAlarm>> GetAllAlarmsAsync()
        {
            return await _dbContext.EcgAlarms.ToListAsync();
        }

        public async Task AddAlarmAsync(EcgAlarm alarm)
        {
            _dbContext.EcgAlarms.Add(alarm);
            await _dbContext.SaveChangesAsync();
        }
    }
}
