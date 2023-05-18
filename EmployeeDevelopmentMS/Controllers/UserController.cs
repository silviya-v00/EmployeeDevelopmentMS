using EmployeeDevelopmentMS.Models;
using EmployeeDevelopmentMS.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeDevelopmentMS.Controllers
{
    public class UserController : Controller
    {
        private readonly IConfiguration _configuration;
        private DBUtil _dbUtil;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public UserController(IConfiguration configuration, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _configuration = configuration;
            _dbUtil = new DBUtil(_configuration.GetConnectionString("DefaultConnection"));
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult ActivateUsers()
        {
            List<RegularUser> allInactiveUsers = new List<RegularUser>();
            if (!User.IsInRole("ADMIN"))
            {
                ModelState.AddModelError("invalidUserRole", "Нямате достъп до тази страница!");
            }
            else
            {
                allInactiveUsers = _dbUtil.GetAllInactiveUsers();
            }

            return View(allInactiveUsers);
        }

        [HttpPost]
        public ActionResult SaveActivation([FromBody] RegularUser[] data)
        {
            foreach (var dataRow in data)
            {
                _dbUtil.UpdateUserActiveStatus(dataRow.UserID, dataRow.IsActive);
            }

            return Json(new { redirectToUrl = Url.Action("AdminHome", "Home") });
        }

        public IActionResult AllUsers()
        {
            List<RegularUser> allUsers = new List<RegularUser>();
            List<Company> companies = new List<Company>();

            if (!User.IsInRole("ADMIN"))
            {
                ModelState.AddModelError("invalidUserRole", "Нямате достъп до тази страница!");
            }
            else
            {
                allUsers = _dbUtil.GetAllUsers();
                companies = _dbUtil.GetAllCompanies();
            }

            ViewBag.Companies = companies;

            return View(allUsers);
        }

        [AcceptVerbs("Post")]
        public IActionResult SearchUsers(string json)
        {
            SearchUser searchUser = JsonConvert.DeserializeObject<SearchUser>(json);

            List<RegularUser> filteredUsers = new List<RegularUser>();
            filteredUsers = _dbUtil.GetFilteredUsers(searchUser);

            return Json(filteredUsers);
        }
    }
}
