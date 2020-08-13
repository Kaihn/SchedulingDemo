using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlandayChallenge.Contracts;
using PlandayChallenge.Contracts.V1.Requests;
using PlandayChallenge.Contracts.V1.Responses;
using PlandayChallenge.Data;
using PlandayChallenge.Domain;
using PlandayChallenge.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

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

        [HttpGet(ApiRoutes.Shifts.GetAll)]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _shiftService.GetAllShiftsAsync());
        }

        [HttpGet(ApiRoutes.Shifts.GetForSpecificEmployee)]
        public async Task<IActionResult> GetForSpecificEmployee([FromRoute] Guid employeeId)
        {
            var shift = await _shiftService.GetShiftsForSpecificEmployeeByIdAsync(employeeId);

            if (shift == null)
            {
                return NotFound();
            }
            return Ok(shift);
        }

        [HttpPost(ApiRoutes.Shifts.Create)]
        public async Task<IActionResult> Create([FromRoute] Guid employeeId, [FromBody] CreateShiftRequest request)
        {
            // Error checking
            // NOTE:    An issue with amount of days per month where a month might have 30 days, but 31 is a valid input 
            //          Would be better to have a type like DateTime that handles this
            if (new DateTime(request.Year, request.Month, request.Day) < DateTime.Now || request.Day < 1 || request.Day > 31 || request.Month < 1 || request.Month > 12)
            {
                return StatusCode(StatusCodes.Status406NotAcceptable, "The given date for the shift is not valid!" +
                    "\nMake sure the date is not in the past, and that Day must be between 1-31, and Month must be between 1-12!");
            }
            if (request.StartTime < 0 || request.StartTime > 23)
            {
                return StatusCode(StatusCodes.Status406NotAcceptable, "The start time for the given shift is not valid! Must be between 0 and 23, but was " + request.StartTime + ".");
            }
            if (request.StartTime + request.Duration > 24 || request.Duration < 1)
            {
                return StatusCode(StatusCodes.Status406NotAcceptable, "The duration for the given shift is not valid, as shifts are not allowed to go beyond the assigned day, and must be bigger than 0. End of shift was " 
                    + (request.StartTime + request.Duration) + ".");
            }

            List<Shift> employeeShifts = await _shiftService.GetShiftsForSpecificEmployeeByIdAsync(employeeId);
            for (int i = 0; i < employeeShifts.Count; i++)
            {
                if (DoesShiftsOverlap(employeeShifts[i], request))
                {
                    return StatusCode(StatusCodes.Status406NotAcceptable, "The new time for this shift overlaps with a different shift (Id: " + employeeShifts[i].Id + ") for this employee (Id:" + employeeId + ")");
                }
            }

            var shift = new Shift { ShiftOwnerId = employeeId, Day = request.Day, Month = request.Month, 
                Year = request.Year, StartTime = request.StartTime, Duration = request.Duration};

            await _shiftService.CreateShiftAsync(shift);

            var baseUrl = HttpContext.Request.Scheme + "//" + HttpContext.Request.Host.ToUriComponent();
            var locationUri = baseUrl + "/" + ApiRoutes.Shifts.Get.Replace("{shiftId}", shift.Id.ToString());

            var response = new ShiftResponse { Id = shift.Id };
            return Created(locationUri, response);
        }

        [HttpPut(ApiRoutes.Shifts.Edit)]
        public async Task<IActionResult> Edit([FromRoute] Guid shiftId, [FromBody] UpdateShiftRequest request)
        {
            // Error checking
            // NOTE: An issue with amount of days per month, would be better to have a type that handles this
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

            if (new DateTime(request.Year, request.Month, request.Day) < DateTime.Now || request.Day < 1 || request.Day > 31 || request.Month < 1 || request.Month > 12)
            {
                return StatusCode(StatusCodes.Status406NotAcceptable, "The given date for the shift is not valid!" +
                    "\nMake sure the date is not in the past, and that Day must be between 1-31, and Month must be between 1-12!");
            }
            if (request.StartTime < 0 || request.StartTime > 23)
            {
                return StatusCode(StatusCodes.Status406NotAcceptable, "The start time for the given shift is not valid! Must be between 0 and 23, but was " + request.StartTime);
            }
            if (request.StartTime + request.Duration > 24 || request.Duration < 1)
            {
                return StatusCode(StatusCodes.Status406NotAcceptable, "The duration for the given shift is not valid, as shifts are not allowed to go beyond the assigned day, and must be bigger than 0. End of shift was "
                    + (request.StartTime + request.Duration) + ".");
            }
            Shift shift = await _shiftService.GetShiftById(shiftId);
            List<Shift> allShiftForEmployee = await _shiftService.GetShiftsForSpecificEmployeeByIdAsync(shift.ShiftOwnerId);
            for (int i = 0; i < allShiftForEmployee.Count; i++)
            {
                if (shiftId != allShiftForEmployee[i].Id && DoesShiftsOverlap(allShiftForEmployee[i], request))
                {
                    return StatusCode(StatusCodes.Status406NotAcceptable, "The new time for this shift overlaps with a different shift (Id: " + allShiftForEmployee[i].Id + ") for this employee (Id:" + allShiftForEmployee[i].ShiftOwnerId + ")");
                }
            }

            shift.ShiftOwnerId = Guid.Parse(request.ShiftOwnerId);
            shift.Day = request.Day;
            shift.Month = request.Month;
            shift.Year = request.Year;
            shift.StartTime = request.StartTime;
            shift.Duration = request.Duration;

            bool updated = await _shiftService.UpdateShiftAsync(shift);

            if (updated)
            {
                return Ok(shift);
            }
            return NotFound();
        }

        [HttpDelete(ApiRoutes.Shifts.Delete)]
        public async Task<IActionResult> Delete([FromRoute] Guid shiftId)
        {
            bool deleted = await _shiftService.DeleteShiftAsync(shiftId);

            if (deleted)
            {
                return NoContent();
            }
            return NotFound();
        }

        //[HttpPost("{id}")]
        //public IActionResult Swap(int id, int otherId)
        //{
        //    // new shift owner Ids, should be passed as arguments somehow
        //    int newShiftOwner = 1;

        //    Shift shift = db.Shifts.Find(id);
        //    if (shift == null)
        //    {
        //        return StatusCode(StatusCodes.Status404NotFound, "No shift with the given Id exists!");
        //    }

        //    if (IsShiftValid(newShiftOwner, shift))
        //    {
        //        shift.employeeId = newShiftOwner;
        //        db.SaveChanges();
        //        return StatusCode(StatusCodes.Status202Accepted);
        //    }
        //    else
        //    {
        //        return StatusCode(StatusCodes.Status417ExpectationFailed, "The new employee for the given shift already has a shift at the given time slot! Aborting swap...");
        //    }
        //}

        //public bool IsSwapValid(int firstShiftId, int secondShiftId)
        //{
        //    Shift firstShift = db.Shifts.Find(firstShiftId);
        //    Shift secondShift = db.Shifts.Find(secondShiftId);
        //    List<Shift> firstEmployeesShifts = new List<Shift>();
        //    List<Shift> secondEmployeesShifts = new List<Shift>();

        //    // Get all shifts for each employee that is part of the shift swap, but not the shifts that are being swapped
        //    foreach (var shift in db.Shifts)
        //    {
        //        if (shift.shiftId == firstShiftId || shift.shiftId == secondShiftId)
        //        {
        //            continue;
        //        }
        //        if (firstShift.employeeId == shift.employeeId)
        //        {
        //            firstEmployeesShifts.Add(shift);
        //        }
        //        else if (secondShift.employeeId == shift.employeeId)
        //        {
        //            secondEmployeesShifts.Add(shift);
        //        }
        //    }

        //    // Check whether swapping the shifts will results in conflicts
        //    for (int i = 0; i < firstEmployeesShifts.Count; i++)
        //    {
        //        if (DoesShiftsOverlap(firstEmployeesShifts[i], secondShift))
        //        {
        //            return false;
        //        }
        //    }

        //    for (int i = 0; i < secondEmployeesShifts.Count; i++)
        //    {
        //        if (DoesShiftsOverlap(secondEmployeesShifts[i], firstShift))
        //        {
        //            return false;
        //        }
        //    }
        //    return true;
        //}


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
                (!(a.StartTime + a.Duration <= b.StartTime) &&
                 !(b.StartTime + b.Duration <= a.StartTime)))
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
                (!(a.StartTime + a.Duration <= b.StartTime) &&
                 !(b.StartTime + b.Duration <= a.StartTime)))
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
        public static bool DoesShiftsOverlap(Shift a, UpdateShiftRequest b)
        {
            if (a.Year == b.Year &&
                a.Month == b.Month &&
                a.Day == b.Day &&
                (!(a.StartTime + a.Duration <= b.StartTime) &&
                 !(b.StartTime + b.Duration <= a.StartTime)))
            {
                return true;
            }
            return false;
        }
    }
}
