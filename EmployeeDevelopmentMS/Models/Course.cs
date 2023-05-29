using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeDevelopmentMS.Models
{
    public class Course
    {
        public int? CourseID { get; set; }
        public string CourseURL { get; set; }
        public RegularUser Employee { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool? IsCompleted { get; set; }
    }
}
