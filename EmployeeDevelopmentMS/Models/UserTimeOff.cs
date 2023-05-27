using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeDevelopmentMS.Models
{
    public class UserTimeOff
    {
        public int? TimeOffID { get; set; }
        public RegularUser EmployeeOnTimeOff { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TimeOffDays
        {
            get
            {
                return (EndDate - StartDate).Days + 1;
            }
        }
    }
}
