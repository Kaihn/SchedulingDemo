namespace PlandayChallenge.Contracts.V1.Requests
{
    /// <summary>
    /// Request that contains two id strings as user input.
    /// These Ids are strings to avoid compiler errors on bad input, and should be parsed as Guids when used.
    /// </summary>
    public class SwapShiftsRequest
    {
        public string ShiftAId { get; set; } 
        public string ShiftBId { get; set; }
    }
}
