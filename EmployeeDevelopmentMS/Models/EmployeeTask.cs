using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeDevelopmentMS.Models
{
    public class EmployeeTask
    {
        public int? TaskID { get; set; }
        public string TaskTitle { get; set; }
        public string TaskDescription { get; set; }
        public Employee Employee { get; set; }
        public int EstimatedHours { get; set; }
        public int? WorkedHours { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedByID { get; set; }
        public DateTime? CompletedDate { get; set; }
        public bool IsCompleted { get; set; }
        public int? RatePoints { get; set; }
        public bool? IsRated { get; set; }
    }

    public class Employee
    {
        public string EmployeeID { get; set; }
        public string EmployeeName { get; set; }
    }
}
