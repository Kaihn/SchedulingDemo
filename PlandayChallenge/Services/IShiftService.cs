using PlandayChallenge.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlandayChallenge.Services
{
    public interface IShiftService
    {
        Task<List<Shift>> GetAllShiftsAsync();
        Task<List<Shift>> GetShiftsForSpecificEmployeeByIdAsync(Guid employeeId);
        Task<Shift> GetShiftById(Guid shiftId);
        Task<bool> CreateShiftAsync(Shift shift);
        Task<bool> UpdateShiftAsync(Shift shiftToUpdate);
        Task<bool> DeleteShiftAsync(Guid shiftId);
    }
}
