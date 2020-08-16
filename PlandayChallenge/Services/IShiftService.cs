using PlandayChallenge.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlandayChallenge.Services
{
    public interface IShiftService
    {
        /// <summary>
        /// Get all shifts in the database asynchronously.
        /// </summary>
        /// <returns></returns>
        Task<List<Shift>> GetAllShiftsAsync();

        /// <summary>
        /// Get all shifts for a specific employee in the database, given by the employee's Id asynchronously.
        /// </summary>
        /// <returns></returns>
        Task<List<Shift>> GetShiftsForSpecificEmployeeByIdAsync(Guid employeeId);

        /// <summary>
        /// Get a specific shift by its Id asynchronously.
        /// </summary>
        /// <param name="shiftId"></param>
        /// <returns></returns>
        Task<Shift> GetShiftByIdAsync(Guid shiftId);

        /// <summary>
        /// Create a new shift for a specific employee asynchronously.
        /// </summary>
        /// <param name="shift"></param>
        /// <returns></returns>
        Task<bool> CreateShiftAsync(Shift shift);

        /// <summary>
        /// Update a shift asynchronously.
        /// </summary>
        /// <param name="shiftToUpdate"></param>
        /// <returns></returns>
        Task<bool> UpdateShiftAsync(Shift shiftToUpdate);

        /// <summary>
        /// Delete a specific shift based on its Id asynchronously.
        /// </summary>
        /// <param name="shiftId"></param>
        /// <returns></returns>
        Task<bool> DeleteShiftAsync(Guid shiftId);

        /// <summary>
        /// Delete all shifts for a specific employee based on the empployee's Id asynchronously.
        /// </summary>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        Task<bool> DeleteShiftsForSpecificEmployeesByIdAsync(Guid[] employeeId);

        /// <summary>
        /// Delete all shifts for all employees asynchronously.
        /// </summary>
        /// <returns></returns>
        Task<bool> DeleteAllShiftsAsync();
    }
}
