using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeDevelopmentMS.Models
{
    public class Role
    {
        public string RoleID { get; set; }
        public string RoleName { get; set; }
    }

    public class UserRole
    {
        public string UserID { get; set; }
        public string RoleID { get; set; }
    }
}
