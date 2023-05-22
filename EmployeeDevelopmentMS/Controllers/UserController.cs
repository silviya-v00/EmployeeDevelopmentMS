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

        public IActionResult RegisterEmployee()
        {
            if (!User.IsInRole("MANAGER"))
            {
                ModelState.AddModelError("invalidUserRole", "Нямате достъп до тази страница!");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegisterNewEmployee(RegularUser model)
        {
            var user = new ApplicationUser();
            string errorMessage = "";

            if (_dbUtil.DoesUserNameExist(model.UserName))
            {
                ModelState.AddModelError("invalidRegistration", "Потребителското име е заето!");

                return View("RegisterEmployee", model);
            }
            else
            {
                if (_dbUtil.DoesEmailExist(model.Email))
                {
                    ModelState.AddModelError("invalidRegistration", "Имейлът е зает!");

                    return View("RegisterEmployee", model);
                }
                else
                {
                    errorMessage = CommonUtil.ValidatePassword(model.Password, 8);
                    if (!String.IsNullOrEmpty(errorMessage))
                    {
                        ModelState.AddModelError("invalidRegistration", errorMessage);

                        return View("RegisterEmployee", model);
                    }
                    else
                    {
                        user = new ApplicationUser { UserName = model.UserName, Email = model.Email, FirstName = model.FirstName, LastName = model.LastName, PhoneNumber = model.PhoneNumber, RegistrationDate = DateTime.Now };

                        var result = await _userManager.CreateAsync(user, model.Password);
                        if (result.Succeeded)
                        {
                            await _userManager.AddToRoleAsync(user, "EMPLOYEE");
                            var currentUser = await _userManager.GetUserAsync(User);
                            int companyID = _dbUtil.GetCompanyIDByUserID(currentUser.Id);
                            string userID = await _userManager.GetUserIdAsync(user);
                            _dbUtil.AddUserCompany(userID, companyID);

                            ModelState.AddModelError("validRegistration", "Регистрацията е успешна! Предстои активиране на профила от администратор!");

                            return View("RegisterEmployee", model);
                        }
                        else
                        {
                            foreach (var error in result.Errors)
                            {
                                ModelState.AddModelError("invalidRegistration", error.Description);
                            }

                            return View("RegisterEmployee", model);
                        }
                    }
                }
            }
        }

        public async Task<IActionResult> TaskManagement()
        {
            List<Employee> employeesInCompany = new List<Employee>();
            string currentUserID = "";
            List<EmployeeTask> allTasksByManager = new List<EmployeeTask>();

            if (User.IsInRole("ADMIN"))
            {
                ModelState.AddModelError("invalidUserRole", "Нямате достъп до тази страница!");
            }
            else
            {
                var currentUser = await _userManager.GetUserAsync(User);
                currentUserID = currentUser.Id;
                int companyID = _dbUtil.GetCompanyIDByUserID(currentUserID);
                employeesInCompany = _dbUtil.GetEmployeesByCompanyID(companyID);

                if (User.IsInRole("MANAGER"))
                {
                    allTasksByManager = _dbUtil.GetAllTasksByUserID(currentUserID, true);
                }
                else
                {
                    allTasksByManager = _dbUtil.GetAllTasksByUserID(currentUserID, false);
                }
            }

            ViewBag.EmployeesInCompany = employeesInCompany;
            ViewBag.CurrentUserID = currentUserID;
            ViewBag.AllTasksByManager = allTasksByManager;
            return View();
        }

        [AcceptVerbs("Post")]
        public IActionResult SaveTask(string json)
        {
            EmployeeTask task = JsonConvert.DeserializeObject<EmployeeTask>(json);

            _dbUtil.SaveTask(task);

            return Json(new { redirectToUrl = Url.Action("TaskManagement", "User") });
        }

        [AcceptVerbs("Post")]
        public IActionResult DeleteTask(string json)
        {
            EmployeeTask task = JsonConvert.DeserializeObject<EmployeeTask>(json);

            _dbUtil.DeleteTask(task.TaskID.Value);

            return Json(new { redirectToUrl = Url.Action("TaskManagement", "User") });
        }

        [AcceptVerbs("Post")]
        public IActionResult CompleteTask(string json)
        {
            EmployeeTask task = JsonConvert.DeserializeObject<EmployeeTask>(json);

            _dbUtil.CompleteTask(task);

            return Json(new { redirectToUrl = Url.Action("TaskManagement", "User") });
        }

        [AcceptVerbs("Post")]
        public IActionResult RateTask(string json)
        {
            EmployeeTask task = JsonConvert.DeserializeObject<EmployeeTask>(json);

            _dbUtil.RateTask(task);

            return Json(new { redirectToUrl = Url.Action("TaskManagement", "User") });
        }
    }
}
