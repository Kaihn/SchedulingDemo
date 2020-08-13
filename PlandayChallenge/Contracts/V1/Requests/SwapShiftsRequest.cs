using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlandayChallenge.Contracts.V1.Requests
{
    public class SwapShiftsRequest
    {
        public string ShiftAId { get; set; }
        public string ShiftBId { get; set; }
    }
}
