namespace PlandayChallenge.Contracts.V1.Requests
{
    /// <summary>
    /// Request that contains a bool, if the user wants to delete all shifts in the database,
    /// and an array of employee Ids as strings, if they instead want to delete specific employees' shifts.
    /// Remeber to correctly parse the strings as appropriate Ids.
    /// </summary>
    public class DeleteShiftsRequest
    {
        public bool DeleteAllShifts { get; set; }
        public string[] employeeIdsToDeleteShifts { get; set; }
    }
}
