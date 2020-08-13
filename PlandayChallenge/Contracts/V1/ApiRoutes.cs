using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlandayChallenge.Contracts
{
    public static class ApiRoutes
    {
        public const string Root = "api";

        public const string Version = "v1";

        public const string Base = Root + "/" + Version;
        public static class Shifts
        {
            public const string GetAll = Base + "/shifts";
            public const string Create = Base + "/shifts/{employeeId}";
            public const string Get = Base + "/shifts/{shiftId}";
            public const string GetForSpecificEmployee = Base + "/shifts/{employeeId}";
            public const string Edit = Base + "/shifts/{shiftId}";
            public const string Swap = Base + "/shifts/";
            public const string Delete = Base + "/shifts/{shiftId}";
        }

        public static class Employees
        {
            public const string GetAll = Base + "/employees";
            public const string Create = Base + "/employees";
            public const string Get = Base + "/employees/{employeeId}";
            public const string Edit = Base + "/employees/{employeeId}";
            public const string Delete = Base + "/employees/{employeeId}";
        }
    }
}
