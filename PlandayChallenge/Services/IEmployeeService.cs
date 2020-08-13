using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlandayChallenge.Domain;

namespace PlandayChallenge.Services
{
    public interface IEmployeeService
    {
        Task<List<Employee>> GetAllEmployeesAsync();
        Task<Employee> GetEmployeeByIdAsync(Guid employeeId);
        Task<bool> CreateEmployeeAsync(Employee employee);
        Task<bool> EditEmployeeAsync(Employee employeeToUpdate);
        Task<bool> DeleteEmployeeAsync(Guid employeeId);
    }
}
