using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using PlandayChallenge.Domain;
using Microsoft.AspNetCore.Http;
using PlandayChallenge.Contracts;
using PlandayChallenge.Contracts.V1.Requests;
using PlandayChallenge.Contracts.V1.Responses;
using PlandayChallenge.Services;
using System.Collections.Generic;

namespace PlandayChallenge.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly IEmployeeService _employeeService;

        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        /// <summary>
        /// Get all employees as a list.
        /// </summary>
        /// <returns></returns>
        [HttpGet(ApiRoutes.Employees.GetAll)]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _employeeService.GetAllEmployeesAsync());
        }

        /// <summary>
        /// Get a specific employee from the employee Id.
        /// </summary>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        [HttpGet(ApiRoutes.Employees.Get)]
        public async Task<IActionResult> Get([FromRoute] Guid employeeId)
        {
            Employee employee = await _employeeService.GetEmployeeByIdAsync(employeeId);

            if (employee == null)
            {
                return NotFound();
            }
            return Ok(employee);
        }

        /// <summary>
        /// Create an employee based on a FromBody request, assuming the email is different from the existing employees in the database.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost(ApiRoutes.Employees.Create)]
        public async Task<IActionResult> Create([FromBody] CreateEmployeeRequest request)
        {
            List<Employee> allEmployees = await _employeeService.GetAllEmployeesAsync();
            for (int i = 0; i < allEmployees.Count; i++)
            {
                if (allEmployees[i].Email == request.Email)
                {
                    return StatusCode(StatusCodes.Status406NotAcceptable, "This Email is already in use, please choose a different Email!");
                }
            }

            Employee employee = new Employee { FirstName = request.FirstName, LastName = request.LastName, Email = request.Email};

            await _employeeService.CreateEmployeeAsync(employee);

            string baseUrl = HttpContext.Request.Scheme + "//" + HttpContext.Request.Host.ToUriComponent();
            string locationUri = baseUrl + "/" + ApiRoutes.Employees.Get.Replace("{employeeId}", employee.Id.ToString());

            EmployeeResponse response = new EmployeeResponse { Id = employee.Id };
            return Created(locationUri, response);
        }

        /// <summary>
        /// Edit an existing employee given its Id and based on a FromBody request.
        /// </summary>
        /// <param name="employeeId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut(ApiRoutes.Employees.Edit)]
        public async Task<IActionResult> Edit([FromRoute] Guid employeeId, [FromBody] EditEmployeeRequest request)
        {
            List<Employee> allEmployees = await _employeeService.GetAllEmployeesAsync();
            for (int i = 0; i < allEmployees.Count; i++)
            {
                if (allEmployees[i].Email == request.Email && allEmployees[i].Id != employeeId)
                {
                    return StatusCode(StatusCodes.Status406NotAcceptable, "This Email is already in use, please choose a different Email!");
                }
            }

            // NOTE:    Ideally i would simply create a new Employee, and simply replace the existing Employee 
            //          in the database with the new one, however this causes a tracking error for the given Id, 
            //          which is only allowed 1 tracker, So as a patchwerk solution i instead change each individual value for that shift.
            Employee employee = await _employeeService.GetEmployeeByIdAsync(employeeId);
            employee.FirstName = request.FirstName;
            employee.LastName = request.LastName;
            employee.Email = request.Email;

            bool updated = await _employeeService.UpdateEmployeeAsync(employee);

            if (updated)
            {
                return Ok(employee);
            }
            return NotFound();
        }

        /// <summary>
        /// Delete a specific employee given its Id.
        /// </summary>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        [HttpDelete(ApiRoutes.Employees.Delete)]
        public async Task<IActionResult> Delete([FromRoute] Guid employeeId)
        {
            bool deleted = await _employeeService.DeleteEmployeeAsync(employeeId);

            if (deleted)
            {
                return NoContent();
            }
            return NotFound();
        }
    }
}
