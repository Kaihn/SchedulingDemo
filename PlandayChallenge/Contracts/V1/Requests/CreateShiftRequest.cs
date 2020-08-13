using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlandayChallenge.Contracts.V1.Requests
{
    public class CreateShiftRequest
    {
        public int Day { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int StartTime { get; set; }
        public int Duration { get; set; }
    }
}
