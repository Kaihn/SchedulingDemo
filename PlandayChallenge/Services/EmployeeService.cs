using Microsoft.EntityFrameworkCore;
using PlandayChallenge.Data;
using PlandayChallenge.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlandayChallenge.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly DataContext _dataContext;
        public EmployeeService(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<List<Employee>> GetAllEmployeesAsync()
        {
            return await _dataContext.Employees.ToListAsync();
        }

        public async Task<Employee> GetEmployeeByIdAsync(Guid employeeId)
        {
            return await _dataContext.Employees.SingleOrDefaultAsync(x => x.Id == employeeId);
        }

        public async Task<bool> CreateEmployeeAsync(Employee employee)
        {
            await _dataContext.Employees.AddAsync(employee);
            var created = await _dataContext.SaveChangesAsync();
            return created > 0;
        }

        public async Task<bool> EditEmployeeAsync(Employee employeeToUpdate)
        {
            _dataContext.Employees.Update(employeeToUpdate);
            var updated = await _dataContext.SaveChangesAsync();
            return updated > 0;
        }

        public async Task<bool> DeleteEmployeeAsync(Guid employeeId)
        {
            var employee = await GetEmployeeByIdAsync(employeeId);

            if (employee == null)
            {
                return false;
            }

            _dataContext.Employees.Remove(employee);
            var deleted = await _dataContext.SaveChangesAsync();
            return deleted > 0;
        }
    }
}
