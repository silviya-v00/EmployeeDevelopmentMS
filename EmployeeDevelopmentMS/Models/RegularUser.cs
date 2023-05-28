using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeDevelopmentMS.Models
{
    public class RegularUser
    {
        public string UserID { get; set; }

        [Required(ErrorMessage = "Моля въведете име.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Моля въведете фамилия.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Моля въведете потребителско име.")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Моля въведете имейл.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Моля въведете парола.")]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Моля потвърдете парола.")]
        [Compare("Password", ErrorMessage = "Паролите не съвпадат.")]
        public string ConfirmPassword { get; set; }

        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Моля въведете компания.")]
        public string CompanyName { get; set; }

        public string FullName
        {
            get
            {
                return FirstName + " " + LastName;
            }
        }

        public bool IsActive { get; set; }

        public Role Role { get; set; }

        public DateTime? RegistrationDate { get; set; }

        public DateTime? LastLoginDate { get; set; }

        public string OldUserName { get; set; }
    }
}
