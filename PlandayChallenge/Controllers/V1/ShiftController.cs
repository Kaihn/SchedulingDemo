using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlandayChallenge.Contracts;
using PlandayChallenge.Contracts.V1.Requests;
using PlandayChallenge.Contracts.V1.Responses;
using PlandayChallenge.Domain;
using PlandayChallenge.Services;


namespace PlandayChallenge.Controllers.V1
{
    public class ShiftController : Controller
    {
        private readonly IShiftService _shiftService;
        private readonly IEmployeeService _employeeService;

        public ShiftController(IShiftService shiftService, IEmployeeService employeeService)
        {
            _shiftService = shiftService;
            _employeeService = employeeService;
        }

        /// <summary>
        /// Get all the Shifts in the database as a list.
        /// </summary>
        /// <returns></returns>
        [HttpGet(ApiRoutes.Shifts.GetAll)]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _shiftService.GetAllShiftsAsync());
        }

        /// <summary>
        /// Get all the shifts for a specific Employee as a list, based on the given Employee Id.
        /// </summary>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        [HttpGet(ApiRoutes.Shifts.GetForSpecificEmployee)]
        public async Task<IActionResult> GetForSpecificEmployee([FromRoute] Guid employeeId)
        {
            List<Shift> shift = await _shiftService.GetShiftsForSpecificEmployeeByIdAsync(employeeId);

            if (shift == null)
            {
                return NotFound();
            }
            return Ok(shift);
        }

        /// <summary>
        /// Create a new shift based on a FromBody shift request, assigned to the employee with the given Id.
        /// </summary>
        /// <param name="employeeId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost(ApiRoutes.Shifts.Create)]
        public async Task<IActionResult> Create([FromRoute] Guid employeeId, [FromBody] CreateShiftRequest request)
        {
            #region Error-handling
            // NOTE:    An issue with amount of days per month where a month might have 30 days, but 31 is a valid input 
            //          Would be better to have a type like DateTime that handles this
            if (request.Day < 1 || request.Day > 31 || request.Month < 1 || request.Month > 12 || request.Year < 1 || 
                new DateTime(request.Year, request.Month, request.Day) < DateTime.Now)
            {
                return StatusCode(StatusCodes.Status406NotAcceptable, "The given date for the shift is not valid!" +
                    "\nMake sure the date is not in the past, and that Day is between 1-31, and Month is between 1-12!");
            }
            // Handle out of bounds start time
            if (request.StartTime < 0 || request.StartTime > 23)
            {
                return StatusCode(StatusCodes.Status406NotAcceptable, "The time given for the start of the shift is not valid! Must be between 0 and 23!");
            }
            // Handle out of bounds end time
            if (request.EndTime <= request.StartTime || request.EndTime > 24)
            {
                return StatusCode(StatusCodes.Status406NotAcceptable, "The time given for the end of the shift is not valid! Must be higher than the start time (" + request.StartTime + "), but no higher than 24!");
            }
            // Check that shift does not overlap with an existing shift for the given employee
            List<Shift> employeeShifts = await _shiftService.GetShiftsForSpecificEmployeeByIdAsync(employeeId);
            for (int i = 0; i < employeeShifts.Count; i++)
            {
                if (DoesShiftsOverlap(employeeShifts[i], request))
                {
                    return StatusCode(StatusCodes.Status406NotAcceptable, "The new time for this shift overlaps with a different shift (Id:" + employeeShifts[i].Id + ") for this employee (Id:" + employeeId + ")");
                }
            }
            #endregion

            Shift shift = new Shift { ShiftOwnerId = employeeId, Day = request.Day, Month = request.Month, 
                Year = request.Year, StartTime = request.StartTime, EndTime = request.EndTime};

            await _shiftService.CreateShiftAsync(shift);

            string baseUrl = HttpContext.Request.Scheme + "//" + HttpContext.Request.Host.ToUriComponent();
            string locationUri = baseUrl + "/" + ApiRoutes.Shifts.Get.Replace("{shiftId}", shift.Id.ToString());

            ShiftResponse response = new ShiftResponse { Id = shift.Id };
            return Created(locationUri, response);
        }

        /// <summary>
        /// Edit an existing shift given its Id and based on a FromBody request. Also has the option to reassign the shift to a different employee, 
        /// by changing the employeeId in the request.
        /// </summary>
        /// <param name="shiftId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut(ApiRoutes.Shifts.Edit)]
        public async Task<IActionResult> Edit([FromRoute] Guid shiftId, [FromBody] EditShiftRequest request)
        {
            #region Error handling
            // In case of bad user input, gets Guid as string an parses to Guid - this shouldn't be nessecary with proper front-end setup
            if (!Guid.TryParse(request.ShiftOwnerId, out Guid requestOwnerId))
            {
                return StatusCode(StatusCodes.Status406NotAcceptable, "The given Id is not a valid Guid!");
            }
            List<Employee> allEmployees = await _employeeService.GetAllEmployeesAsync();
            bool _ownerIdExists = false;
            for (int i = 0; i < allEmployees.Count; i++)
            {
                if (allEmployees[i].Id == requestOwnerId)
                {
                    _ownerIdExists = true;
                    break;
                }
            }
            if (!_ownerIdExists)
            {
                return StatusCode(StatusCodes.Status406NotAcceptable, "No owner with the given Id exists!");
            }

            if (request.Day < 1 || request.Day > 31 || request.Month < 1 || request.Month > 12 || request.Year < 1 || 
                new DateTime(request.Year, request.Month, request.Day) < DateTime.Now)
            {
                return StatusCode(StatusCodes.Status406NotAcceptable, "The given date for the shift is not valid!" +
                    "\nMake sure the date is not in the past, and that Day is between 1-31, and Month is between 1-12!");
            }
            if (request.StartTime < 0 || request.StartTime > 23)
            {
                return StatusCode(StatusCodes.Status406NotAcceptable, "The time given for the start of the shift is not valid! Must be between 0 and 23!");
            }
            if (request.EndTime <= request.StartTime || request.EndTime > 24)
            {
                return StatusCode(StatusCodes.Status406NotAcceptable, "The time given for the end of the shift is not valid! Must be higher than the start time (" + request.StartTime + "), but no higher than 24!");
            }
            List<Shift> allShiftForEmployee = await _shiftService.GetShiftsForSpecificEmployeeByIdAsync(requestOwnerId);
            for (int i = 0; i < allShiftForEmployee.Count; i++)
            {
                if (shiftId != allShiftForEmployee[i].Id && DoesShiftsOverlap(allShiftForEmployee[i], request))
                {
                    return StatusCode(StatusCodes.Status406NotAcceptable, "The new time for this shift overlaps with a different shift (Id:" + allShiftForEmployee[i].Id + ") for this employee (Id:" + allShiftForEmployee[i].ShiftOwnerId + ")");
                }
            }
            #endregion

            // NOTE:    Ideally i would simply create a new shift, and update the database with the given shift,
            //          However this causes a tracking error for the given Id, which is only allowed 1 tracker,
            //          So as a patchwerk solution i instead change each individual value for that shift.
            Shift shift = await _shiftService.GetShiftByIdAsync(shiftId);
            shift.ShiftOwnerId = Guid.Parse(request.ShiftOwnerId);
            shift.Day = request.Day;
            shift.Month = request.Month;
            shift.Year = request.Year;
            shift.StartTime = request.StartTime;
            shift.EndTime = request.EndTime;

            bool updated = await _shiftService.UpdateShiftAsync(shift);

            if (updated)
            {
                return Ok(shift);
            }
            return NotFound();
        }

        /// <summary>
        /// Delete a specific shift given its Id.
        /// </summary>
        /// <param name="shiftId"></param>
        /// <returns></returns>
        [HttpDelete(ApiRoutes.Shifts.Delete)]
        public async Task<IActionResult> Delete([FromRoute] Guid shiftId)
        {
            bool deleted = await _shiftService.DeleteShiftAsync(shiftId);

            // NOTE:    If succesfully deleted, return NoContent to display that there is no longer any content.
            //          It could be considered to store the shift and display that this was the shift that was deleted.
            if (deleted)
            {
                return StatusCode(StatusCodes.Status204NoContent, "Shift successfully deleted!");
            }
            return NotFound();
        }

        /// <summary>
        /// Delete all shifts in the database, or specific employees' shifts given by an array of Guid strings, 
        /// based on the request input.
        /// </summary>
        /// <returns></returns>
        [HttpDelete(ApiRoutes.Shifts.DeleteMultipleShifts)]
        public async Task<IActionResult> DeleteMultipleShifts([FromBody] DeleteShiftsRequest request)
        {
            // As this request includes an array, check to see if array was input correctly (correct commas and quotations)
            if (request == null)
            {
                return StatusCode(StatusCodes.Status406NotAcceptable, "Bad user input! Check the request input again.");
            }

            if (request.DeleteAllShifts)
            {
                bool deleted = await _shiftService.DeleteAllShiftsAsync();
                if (deleted)
                {
                    return StatusCode(StatusCodes.Status204NoContent, "All shifts in the database have been successfully deleted!");
                }
                return NotFound();
            }
            else
            {
                /* Convert string array to Guid array, by parsing each individual string as Guid.
                 * If any of the strings cannot be parsed, or any of the parsed Guids do not match 
                 * employee Ids from the database, return an appropriate error message.
                 */
                int length = request.employeeIdsToDeleteShifts.Length;
                Guid[] employeeIds = new Guid[length];
                List<Employee> allEmployees = await _employeeService.GetAllEmployeesAsync();
                for (int i = 0; i < length; i++)
                {
                    if (!Guid.TryParse(request.employeeIdsToDeleteShifts[i], out Guid requestEmployeeId))
                    {
                        return StatusCode(StatusCodes.Status406NotAcceptable, "While trying to delete specific employee(s) shifts, " +
                            "a given employee Id was not a valid Guid! Aborting...");
                    }
                    bool _employeeIdExists = false;
                    for (int j = 0; j < allEmployees.Count; j++)
                    {
                        if (allEmployees[j].Id == requestEmployeeId)
                        {
                            _employeeIdExists = true;
                            break;
                        }
                    }
                    if (!_employeeIdExists)
                    {
                        return StatusCode(StatusCodes.Status406NotAcceptable, "While trying to delete specific employee(s) shifts, " +
                            "a given employee Id did not exist in the database! Aborting...");
                    }
                    else
                    {
                        employeeIds[i] = requestEmployeeId;
                    }
                }
                bool deleted = await _shiftService.DeleteShiftsForSpecificEmployeesByIdAsync(employeeIds);

                if (deleted)
                {
                    return StatusCode(StatusCodes.Status204NoContent, "All the specified employees' shifts were successfully deleted!");
                }
                return NotFound();
            }
        }

        /// <summary>
        /// Swap 2 shifts given their shift Ids. 
        /// Does not swap if that would cause an overlap with an existing shift for that employee,
        /// or if the shifts belong to the same employee.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut(ApiRoutes.Shifts.Swap)]
        public async Task<IActionResult> Swap([FromBody] SwapShiftsRequest request)
        {
            #region Error handling - String parsing to Guid 
            // Parse input strings to Guids, if parsing fails abort
            if (!Guid.TryParse(request.ShiftAId, out Guid ShiftAGuid))
            {
                return StatusCode(StatusCodes.Status406NotAcceptable, "Shift Id A (" + request.ShiftAId + ") is not a valid Guid! Aborting swap...");
            }
            if (!Guid.TryParse(request.ShiftBId, out Guid ShiftBGuid))
            {
                return StatusCode(StatusCodes.Status406NotAcceptable, "Shift Id B (" + request.ShiftBId + ") is not a valid Guid! Aborting swap...");
            }
            #endregion

            Shift a = await _shiftService.GetShiftByIdAsync(ShiftAGuid);
            Shift b = await _shiftService.GetShiftByIdAsync(ShiftBGuid);

            #region Error handling
            // Guids are valid, but check if they actually match valid Shift Ids
            if (a == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, "Shift A Id:" + ShiftAGuid + " is a valid Guid, but does not match any shift Ids in the database!");
            }
            if (b == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, "Shift B Id:" + ShiftBGuid + " is a valid Guid, but does not match any shift Ids in the database!");
            }

            // Shift Ids are valid, check that the user is not trying to swap shifts that belong to the same employee
            if (a.ShiftOwnerId == b.ShiftOwnerId)
            {
                return StatusCode(StatusCodes.Status406NotAcceptable, "Trying to swap two shifts that are assigned to the same employee, " +
                    "when two different employees are expected! Aborting swap...");
            }

            // Check if Shift a overlaps with any shifts from Employee B, and likewise for Shift B and Employee A 
            List<Shift> allShiftForEmployeeA = await _shiftService.GetShiftsForSpecificEmployeeByIdAsync(a.ShiftOwnerId);
            List<Shift> allShiftForEmployeeB = await _shiftService.GetShiftsForSpecificEmployeeByIdAsync(b.ShiftOwnerId);
            for (int i = 0; i < allShiftForEmployeeA.Count; i++)
            {
                if (DoesShiftsOverlap(allShiftForEmployeeA[i], b))
                {
                    return StatusCode(StatusCodes.Status406NotAcceptable, "During swap, shift B overlaps with an existing shift (Id:" + allShiftForEmployeeA[i].Id + 
                        ") for Employee A (Id:" + a.ShiftOwnerId + ")! Aborting...");
                }
            }
            for (int i = 0; i < allShiftForEmployeeB.Count; i++)
            {
                if (DoesShiftsOverlap(allShiftForEmployeeB[i], a))
                {
                    return StatusCode(StatusCodes.Status406NotAcceptable, "During swap, shift A overlaps with an existing shift (Id:" + allShiftForEmployeeB[i].Id +
                        ") for Employee B (Id:" + b.ShiftOwnerId + ")! Aborting...");
                }
            }
            #endregion

            // Simply swap the shift owners for each shift
            Guid temp = a.ShiftOwnerId;
            a.ShiftOwnerId = b.ShiftOwnerId;
            b.ShiftOwnerId = temp;

            // Inform the database that the shifts have been updated
            bool aUpdated = await _shiftService.UpdateShiftAsync(a);
            bool bUpdated = await _shiftService.UpdateShiftAsync(b);

            if (aUpdated && bUpdated)
            {
                return Ok("Succesful swap!");
            }
            return NotFound();
        }

        #region Helper methods (might not follow REST architecture procedure?)
        /// <summary>
        /// Return true if the shifts overlap, return false if they do not.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool DoesShiftsOverlap(Shift a, Shift b)
        {
            if (a.Year == b.Year &&
                a.Month == b.Month &&
                a.Day == b.Day &&
                !(a.EndTime <= b.StartTime || a.StartTime >= b.EndTime))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Return true if the shifts overlap, return false if they do not.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool DoesShiftsOverlap(Shift a, CreateShiftRequest b)
        {
            if (a.Year == b.Year &&
                a.Month == b.Month &&
                a.Day == b.Day &&
                !(a.EndTime <= b.StartTime || a.StartTime >= b.EndTime))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Return true if the shifts overlap, return false if they do not.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool DoesShiftsOverlap(Shift a, EditShiftRequest b)
        {
            if (a.Year == b.Year &&
                a.Month == b.Month &&
                a.Day == b.Day &&
                !(a.EndTime <= b.StartTime || a.StartTime >= b.EndTime))
            {
                return true;
            }
            return false;
        }
        #endregion
    }
}
