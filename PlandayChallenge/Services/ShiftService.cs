using Microsoft.EntityFrameworkCore;
using PlandayChallenge.Data;
using PlandayChallenge.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlandayChallenge.Services
{
    public class ShiftService : IShiftService
    {
        private readonly DataContext _dataContext;
        public ShiftService(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<List<Shift>> GetAllShiftsAsync()
        {
            return await _dataContext.Shifts.ToListAsync();
        }

        public async Task<List<Shift>> GetShiftsForSpecificEmployeeByIdAsync(Guid employeeId)
        {
            return await _dataContext.Shifts.Where(x => x.ShiftOwnerId == employeeId).ToListAsync();
        }

        public async Task<bool> CreateShiftAsync(Shift shift)
        {
            await _dataContext.Shifts.AddAsync(shift);
            var created = await _dataContext.SaveChangesAsync();
            return created > 0;
        }

        public async Task<bool> DeleteShiftAsync(Guid shiftId)
        {
            var shift = await _dataContext.Shifts.SingleOrDefaultAsync(x => x.Id == shiftId);

            if (shift == null)
            {
                return false;
            }

            _dataContext.Shifts.Remove(shift);
            var deleted = await _dataContext.SaveChangesAsync();
            return deleted > 0;
        }

        public async Task<bool> DeleteAllShiftsAsync()
        {
            _dataContext.RemoveRange(_dataContext.Shifts);
            var deleted = await _dataContext.SaveChangesAsync();
            return deleted > 0;
        }

        public async Task<bool> UpdateShiftAsync(Shift shiftToUpdate)
        {
            _dataContext.Shifts.Update(shiftToUpdate);
            var updated = await _dataContext.SaveChangesAsync();
            return updated > 0;
        }

        public async Task<Shift> GetShiftById(Guid shiftId)
        {
            return await _dataContext.Shifts.SingleOrDefaultAsync(x => x.Id == shiftId);
        }
    }
}
