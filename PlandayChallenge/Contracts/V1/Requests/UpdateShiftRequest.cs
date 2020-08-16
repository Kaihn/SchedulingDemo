namespace PlandayChallenge.Contracts.V1.Requests
{
    public class EditShiftRequest
    {
        public string ShiftOwnerId { get; set; }
        public int Day { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int StartTime { get; set; }
        public int EndTime { get; set; }
    }
}
