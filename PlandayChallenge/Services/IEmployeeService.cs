using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PlandayChallenge.Domain;

namespace PlandayChallenge.Services
{
    public interface IEmployeeService
    {
        /// <summary>
        /// Get all employees asynchronously.
        /// </summary>
        /// <returns></returns>
        Task<List<Employee>> GetAllEmployeesAsync();

        /// <summary>
        /// Get a specific employee based on its Id asynchronously.
        /// </summary>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        Task<Employee> GetEmployeeByIdAsync(Guid employeeId);

        /// <summary>
        /// Create a new employee asynchronously.
        /// </summary>
        /// <param name="employee"></param>
        /// <returns></returns>
        Task<bool> CreateEmployeeAsync(Employee employee);

        /// <summary>
        /// Update an existing employee asynchronously.
        /// </summary>
        /// <param name="employeeToUpdate"></param>
        /// <returns></returns>
        Task<bool> UpdateEmployeeAsync(Employee employeeToUpdate);

        /// <summary>
        /// Delete an existing employee based on its Id asynchronously.
        /// </summary>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        Task<bool> DeleteEmployeeAsync(Guid employeeId);
    }
}
