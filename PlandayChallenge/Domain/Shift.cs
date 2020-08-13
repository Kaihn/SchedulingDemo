using System;
using System.ComponentModel.DataAnnotations;

namespace PlandayChallenge.Domain
{
    public class Shift
    {
        [Key]
        public Guid Id { get; set; }
        public Guid ShiftOwnerId { get; set; }
        public int Day { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int StartTime { get; set; }
        public int Duration { get; set; }
    }
}
