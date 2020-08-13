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

        [HttpGet(ApiRoutes.Employees.GetAll)]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _employeeService.GetAllEmployeesAsync());
        }

        [HttpGet(ApiRoutes.Employees.Get)]
        public async Task<IActionResult> Get([FromRoute] Guid employeeId)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);

            if (employee == null)
            {
                return NotFound();
            }
            return Ok(employee);
        }

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

            var employee = new Employee { FirstName = request.FirstName, LastName = request.LastName, Email = request.Email};

            await _employeeService.CreateEmployeeAsync(employee);

            var baseUrl = HttpContext.Request.Scheme + "//" + HttpContext.Request.Host.ToUriComponent();
            var locationUri = baseUrl + "/" + ApiRoutes.Employees.Get.Replace("{employeeId}", employee.Id.ToString());

            var response = new EmployeeResponse { Id = employee.Id };
            return Created(locationUri, response);
        }

        [HttpPut(ApiRoutes.Employees.Edit)]
        public async Task<IActionResult> Edit([FromRoute] Guid employeeId, [FromBody] UpdateEmployeeRequest request)
        {
            List<Employee> allEmployees = await _employeeService.GetAllEmployeesAsync();
            for (int i = 0; i < allEmployees.Count; i++)
            {
                if (allEmployees[i].Email == request.Email && allEmployees[i].Id != employeeId)
                {
                    return StatusCode(StatusCodes.Status406NotAcceptable, "This Email is already in use, please choose a different Email!");
                }
            }

            Employee employee = await _employeeService.GetEmployeeByIdAsync(employeeId);
            employee.FirstName = request.FirstName;
            employee.LastName = request.LastName;
            employee.Email = request.Email;

            bool updated = await _employeeService.EditEmployeeAsync(employee);

            if (updated)
            {
                return Ok(employee);
            }
            return NotFound();
        }


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
