using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeDevelopmentMS.Models
{
    public class RegisterUser
    {
        public int UserID { get; set; }

        [Required(ErrorMessage = "Моля въведете име.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Моля въведете фамилия.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Моля въведете потребителско име.")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Моля въведете имейл.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Моля въведете парола.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Моля потвърдете парола.")]
        public string ConfirmPassword { get; set; }

        public string PhoneNumber { get; set; }
        public bool IsActive { get; set; }

        [Required(ErrorMessage = "Моля въведете компания.")]
        public string CompanyName { get; set; }

        public string FullName
        {
            get
            {
                return FirstName + " " + LastName;
            }
        }
    }
}
