using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeDevelopmentMS.Models
{
    public class UserPosition
    {
        public int? PositionID { get; set; }
        public string Position { get; set; }
        public RegularUser Employee { get; set; }
        public int? Salary { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? PreviousPositionEndDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedByID { get; set; }
        public bool HasNewPositionRow { get; set; }
    }
}
