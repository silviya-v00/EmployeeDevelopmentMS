using EmployeeDevelopmentMS.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeDevelopmentMS.Models
{
    public class EmployeesInCompanyData
    {
        public string CompanyName { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public bool IsActive { get; set; }
        public string Position { get; set; }
        public int? Salary { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class EmployeesPerformanceData
    {
        public string UserName { get; set; }
        public string TaskCount { get; set; }
        public string HoursRatio { get; set; }
        public string RateRatio { get; set; }
        public int? TimeOff { get; set; }
        public string TimeOffRatio
        {
            get
            {
                return (TimeOff.HasValue ? TimeOff.Value : 0) + " / " + CommonUtil.MaxVacationDays.ToString();
            }
        }
        public string CourseRatio { get; set; }
    }
}
