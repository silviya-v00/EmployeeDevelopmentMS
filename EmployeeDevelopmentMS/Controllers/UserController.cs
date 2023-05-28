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

        public async Task<ApplicationUser> GetApplicationUser()
        {
            return await _userManager.GetUserAsync(User);
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
                allInactiveUsers = _dbUtil.GetAllActiveInactiveUsers();
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

            return Json(new { redirectToUrl = Url.Action("ActivateUsers", "User") });
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
                    errorMessage = CommonUtil.ValidatePassword(model.Password, CommonUtil.RequiredPasswordLength);
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
                            var currentUser = await GetApplicationUser();
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
                var currentUser = await GetApplicationUser();
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

        public async Task<IActionResult> EmployeeManagement()
        {
            List<Employee> employeesInCompany = new List<Employee>();
            string currentUserID = "";

            if (!User.IsInRole("MANAGER"))
            {
                ModelState.AddModelError("invalidUserRole", "Нямате достъп до тази страница!");
            }
            else
            {
                var currentUser = await GetApplicationUser();
                currentUserID = currentUser.Id;
                int companyID = _dbUtil.GetCompanyIDByUserID(currentUserID);
                employeesInCompany = _dbUtil.GetEmployeesByCompanyID(companyID);
            }

            ViewBag.EmployeesInCompany = employeesInCompany;

            return View();
        }

        [AcceptVerbs("Post")]
        public IActionResult GetUserPositions(string json)
        {
            UserPosition selectedUser = JsonConvert.DeserializeObject<UserPosition>(json);

            List<UserPosition> userPositions = new List<UserPosition>();
            userPositions = _dbUtil.GetUserPositionsByUserID(selectedUser.Employee.UserID);

            return Json(userPositions);
        }

        [AcceptVerbs("Post")]
        public async Task<IActionResult> SavePosition(string json)
        {
            UserPosition selectedUser = JsonConvert.DeserializeObject<UserPosition>(json);
            var currentUser = await GetApplicationUser();
            selectedUser.CreatedByID = currentUser.Id;

            _dbUtil.SaveNewPosition(selectedUser);

            return Json(null);
        }

        public async Task<IActionResult> UserProfile()
        {
            List<UserPosition> userPositions = new List<UserPosition>();
            List<UserTimeOff> userTimeOff = new List<UserTimeOff>();
            string currentUserID = "";
            string experience = "";
            string timeOff = "";
            int totalVacationDays = 0;

            if (User.IsInRole("ADMIN"))
            {
                ModelState.AddModelError("invalidUserRole", "Нямате достъп до тази страница!");
            }
            else
            {
                var currentUser = await GetApplicationUser();
                currentUserID = currentUser.Id;
                userPositions = _dbUtil.GetUserPositionsByUserID(currentUserID);

                DateTime? minStartDate = null;
                minStartDate = userPositions.Min(x => x.StartDate);
                var maxStartDate = userPositions.Max(x => x.StartDate);
                var endDateWithMaxStartDate = userPositions.Where(x => x.StartDate == maxStartDate)
                                                           .Select(x => x.EndDate)
                                                           .FirstOrDefault();
                var maxEndDate = endDateWithMaxStartDate.HasValue ? endDateWithMaxStartDate.Value : DateTime.Now;

                experience = CommonUtil.CalculateExperience(minStartDate, maxEndDate);

                userTimeOff = _dbUtil.GetUserTimeOffByUserID(currentUserID);
                totalVacationDays = userTimeOff.Select(x => x.TimeOffDays).Sum();
                timeOff = " (" + totalVacationDays.ToString() + " / " + CommonUtil.MaxVacationDays.ToString() + ")";
            }

            ViewBag.TimeInCompany = experience;
            ViewBag.UserTimeOff = userTimeOff;
            ViewBag.TimeOffRange = timeOff;
            ViewBag.TotalVacationDays = totalVacationDays.ToString();
            ViewBag.MaxVacationDays = CommonUtil.MaxVacationDays.ToString();

            return View(userPositions);
        }

        [HttpGet]
        public async Task<JsonResult> PrefillPersonalInfo()
        {
            var currentUser = await GetApplicationUser();

            RegularUser targetUser = _dbUtil.GetPersonalInfoByUserID(currentUser.Id);

            return Json(new { userName = targetUser.UserName, phoneNumber = targetUser.PhoneNumber });
        }

        [AcceptVerbs("Post")]
        public async Task<IActionResult> UpdatePersonalInfo(string json)
        {
            RegularUser targetUser = JsonConvert.DeserializeObject<RegularUser>(json);
            var currentUser = await GetApplicationUser();

            string errorMsg = "";

            if (!targetUser.OldUserName.Equals(targetUser.UserName) && _dbUtil.DoesUserNameExist(targetUser.UserName))
                errorMsg = "Потребителското име е заето!";

            if (String.IsNullOrEmpty(targetUser.UserName))
                errorMsg = "Потребителското име не може да е празно!";

            _dbUtil.UpdatePersonalInfoByUserID(targetUser, currentUser.Id);

            return Json(new { errorMsg = errorMsg, redirectToUrl = Url.Action("UserProfile", "User") });
        }

        [HttpGet]
        public JsonResult GenerateNewPassword()
        {
            string password = "";
            password = CommonUtil.GenerateRandomPassword();

            return Json(new { newPassword = password });
        }

        [AcceptVerbs("Post")]
        public async Task<IActionResult> ChangePassword(string json)
        {
            RegularUser targetUser = JsonConvert.DeserializeObject<RegularUser>(json);
            string newPassword = targetUser.ConfirmPassword;
            var currentUser = await GetApplicationUser();

            string errorMsg = "";

            var result = await _signInManager.CheckPasswordSignInAsync(currentUser, targetUser.Password, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(currentUser);
                var resetResult = await _userManager.ResetPasswordAsync(currentUser, token, newPassword);
            }
            else
                errorMsg = "Неправилна стара парола!";

            return Json(new { errorMsg = errorMsg, redirectToUrl = Url.Action("UserProfile", "User") });
        }

        [AcceptVerbs("Post")]
        public async Task<IActionResult> AddTimeOff(string json)
        {
            UserTimeOff timeOff = JsonConvert.DeserializeObject<UserTimeOff>(json);
            var currentUser = await GetApplicationUser();

            _dbUtil.AddNewTimeOff(timeOff, currentUser.Id);

            return Json(new { redirectToUrl = Url.Action("UserProfile", "User") });
        }
    }
}
