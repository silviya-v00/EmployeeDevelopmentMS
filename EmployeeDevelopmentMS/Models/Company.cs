using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeDevelopmentMS.Models
{
    public class Company
    {
        public int CompanyID { get; set; }
        public string CompanyName { get; set; }
    }

    public class UserCompany
    {
        public int UserID { get; set; }
        public int CompanyID { get; set; }
    }
}
