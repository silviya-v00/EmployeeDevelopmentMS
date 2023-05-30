using EmployeeDevelopmentMS.Models;
using EmployeeDevelopmentMS.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public async Task<IActionResult> CourseManagement()
        {
            List<Course> courses = new List<Course>();
            string currentUserID = "";

            if (!User.IsInRole("EMPLOYEE"))
            {
                ModelState.AddModelError("invalidUserRole", "Нямате достъп до тази страница!");
            }
            else
            {
                var currentUser = await GetApplicationUser();
                currentUserID = currentUser.Id;
                courses = _dbUtil.GetCoursesByUserID(currentUserID);
            }

            return View(courses);
        }

        [AcceptVerbs("Post")]
        public IActionResult AddNewCourse(string json)
        {
            Course newCourse = JsonConvert.DeserializeObject<Course>(json);
            string errorMsg = "";

            Uri uriResult;
            bool isValidURL = Uri.TryCreate(newCourse.CourseURL, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            if (!isValidURL)
                errorMsg = "Линкът не е валиден!";

            if (String.IsNullOrEmpty(errorMsg))
                _dbUtil.AddNewCourse(newCourse);

            return Json(new { errorMsg = errorMsg });
        }

        [AcceptVerbs("Post")]
        public IActionResult SaveCourseStatus(string json)
        {
            List<Course> data = JsonConvert.DeserializeObject<List<Course>>(json);

            foreach (var dataRow in data)
            {
                _dbUtil.UpdateCourse(dataRow.CourseID.Value, dataRow.IsCompleted.Value);
            }

            return Json(new { redirectToUrl = Url.Action("CourseManagement", "User") });
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

        public async Task<IActionResult> Reports()
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
                employeesInCompany = _dbUtil.GetEmployeesByCompanyID(companyID).Where(x => x.EmployeeID != "-1").ToList();
            }

            ViewBag.EmployeesInCompany = employeesInCompany;

            return View();
        }

        public async Task<IActionResult> OnPostGenerateEmployeesInCompany(string selectedEmployee)
        {
            List<EmployeesInCompanyData> employeesInCompany = new List<EmployeesInCompanyData>();
            string currentUserID = "";
            var currentUser = await GetApplicationUser();
            currentUserID = currentUser.Id;
            employeesInCompany = _dbUtil.GetEmployeesInCompany(selectedEmployee, currentUserID);
            
            byte[] fileBytes = GenerateEmployeesInCompanyReport(employeesInCompany);

            return File(fileBytes, "application/vnd.ms-excel", "CompanyEmployees.xls");
        }

        private byte[] GenerateEmployeesInCompanyReport(List<EmployeesInCompanyData> employeesInCompany)
        {
            string company = "";
            company = employeesInCompany.Select(x => x.CompanyName).FirstOrDefault();

            var groupedData = employeesInCompany.GroupBy(x => new
                                                {
                                                    x.CompanyName,
                                                    x.UserName,
                                                    x.FirstName,
                                                    x.LastName,
                                                    x.Email,
                                                    x.PhoneNumber,
                                                    x.RegistrationDate,
                                                    x.IsActive
                                                })
                                                .Select(group => new
                                                {
                                                    EmployeeDetails = group.Key,
                                                    Positions = group.Select(e => new
                                                    {
                                                        e.Position,
                                                        e.Salary,
                                                        e.StartDate,
                                                        e.EndDate
                                                    })
                                                });

            StringBuilder excelContent = new StringBuilder();

            excelContent.AppendLine(@"<html>
                                          <head>
                                            <meta http-equiv='Content-Type' content='text/html; charset=utf-8'>
                                            <style type='text/css'>
		                                        .cellHeader {
			                                        vertical-align:bottom;
			                                        text-align:center;
			                                        border-bottom:1px solid #000000;
			                                        border-top:1px solid #000000;
			                                        border-left:1px solid #000000;
			                                        border-right:1px solid #000000;
			                                        font-weight:bold;
			                                        color:#FFFFFF;
			                                        font-size:12pt;
			                                        background-color:#4BACC6;
		                                        }
		
		                                        .cellContent {
			                                        vertical-align:top;
			                                        text-align:center;
			                                        border-bottom:1px solid #000000;
			                                        border-top:1px solid #000000;
			                                        border-left:1px solid #000000;
			                                        border-right:1px solid #000000;
			                                        color:#000000;
			                                        font-size:11pt;
			                                        background-color:#DAEEF3;
		                                        }
                                            </style>
                                          </head>
                                          <body>
                                            <table>
                                                <col style='width:112.51110982pt;'>
                                                <col style='width:80.35555484pt;'>
                                                <col style='width:80.35555484pt;'>
                                                <col style='width:200.68888724pt;'>
                                                <col style='width:86.07777679pt;'>
                                                <col style='width:74.5555547pt;'>
                                                <col style='width:66.42222146pt;'>
                                                <col style='width:148.43333163pt;'>
                                                <col style='width:53.54444383pt;'>
                                                <col style='width:60.32222153pt;'>
                                                <col style='width:58.28888822pt;'>
                                                <tbody>
                                                  <tr>
                                                    <td class='cellHeader' colspan='11'>" + company + @"</td>
                                                  </tr>
                                                  <tr>
                                                    <td class='cellHeader'>Служител</td>
                                                    <td class='cellHeader'>Име</td>
                                                    <td class='cellHeader'>Фамилия</td>
                                                    <td class='cellHeader'>Имейл</td>
                                                    <td class='cellHeader'>Телефонен номер</td>
                                                    <td class='cellHeader'>Регистрация</td>
                                                    <td class='cellHeader'>Активен / Неактивен</td>
                                                    <td class='cellHeader'>Позиция</td>
                                                    <td class='cellHeader'>Заплата</td>
                                                    <td class='cellHeader'>Начална дата</td>
                                                    <td class='cellHeader'>Крайна дата</td>
                                                  </tr>");

            foreach (var employeeGroup in groupedData)
            {
                int positionsCnt = employeeGroup.Positions.Count();
                int rowNumInGroup = 1;
                var employeeGroupRow = employeeGroup.EmployeeDetails;

                foreach (var position in employeeGroup.Positions)
                {
                    // <span>&nbsp;</span> is needed in order to escape excel cell autoformatting

                    excelContent.AppendLine(@"<tr>");

                    if (rowNumInGroup == 1)
                    {
                        excelContent.AppendLine(@"<td class='cellContent' rowspan='" + positionsCnt + @"'>" + employeeGroupRow.UserName + @"</td>
                                                  <td class='cellContent' rowspan='" + positionsCnt + @"'>" + employeeGroupRow.FirstName + @"</td>
                                                  <td class='cellContent' rowspan='" + positionsCnt + @"'>" + employeeGroupRow.LastName + @"</td>
                                                  <td class='cellContent' rowspan='" + positionsCnt + @"'>" + employeeGroupRow.Email + @"</td>
                                                  <td class='cellContent' rowspan='" + positionsCnt + @"'><span>&nbsp;</span>" + employeeGroupRow.PhoneNumber + @"</td>
                                                  <td class='cellContent' rowspan='" + positionsCnt + @"'><span>&nbsp;</span>" + (employeeGroupRow.RegistrationDate.HasValue ? employeeGroupRow.RegistrationDate.Value.ToString("MM/dd/yyyy") : "") + @"</td>
                                                  <td class='cellContent' rowspan='" + positionsCnt + @"'>" + (employeeGroupRow.IsActive ? "Активен" : "Неактивен") + @"</td>
                                                  ");
                    }
                    excelContent.AppendLine(@"<td class='cellContent'>" + position.Position + @"</td>
                                              <td class='cellContent'>" + (position.Salary.HasValue ? position.Salary.Value.ToString() : "") + @"</td>
                                              <td class='cellContent'><span>&nbsp;</span>" + (position.StartDate.HasValue ? position.StartDate.Value.ToString("MM/dd/yyyy") : "") + @"</td>
                                              <td class='cellContent'><span>&nbsp;</span>" + (position.EndDate.HasValue ? position.EndDate.Value.ToString("MM/dd/yyyy") : "") + @"</td>
                                              ");

                    excelContent.AppendLine(@"</tr>");

                    rowNumInGroup++;
                }
            }

            excelContent.AppendLine(@"</tbody>
                                    </table>
                                </body>
                            </html>");

            byte[] fileBytes = Encoding.UTF8.GetBytes(excelContent.ToString());

            return fileBytes;
        }

        public async Task<IActionResult> OnPostGenerateEmployeesPerformance(string selectedEmployee, string selectedPeriod)
        {
            List<EmployeesPerformanceData> employeesPerformances = new List<EmployeesPerformanceData>();
            string currentUserID = "";
            var currentUser = await GetApplicationUser();
            currentUserID = currentUser.Id;
            DateTime period = DateTime.Parse(selectedPeriod);
            employeesPerformances = _dbUtil.GetEmployeesPerformance(selectedEmployee, currentUserID, period);

            byte[] fileBytes = GenerateEmployeesPerformanceReport(employeesPerformances, period);

            return File(fileBytes, "application/vnd.ms-excel", "EmployeesPerformance.xls");
        }

        private byte[] GenerateEmployeesPerformanceReport(List<EmployeesPerformanceData> employeesPerformances, DateTime period)
        {
            StringBuilder excelContent = new StringBuilder();
            string periodStr = period.ToString("MM/yy");

            excelContent.AppendLine(@"<html>
                                          <head>
                                            <meta http-equiv='Content-Type' content='text/html; charset=utf-8'>
                                            <style type='text/css'>
		                                        .cellHeader {
			                                        vertical-align:bottom;
			                                        text-align:center;
			                                        border-bottom:1px solid #000000;
			                                        border-top:1px solid #000000;
			                                        border-left:1px solid #000000;
			                                        border-right:1px solid #000000;
			                                        font-weight:bold;
			                                        color:#FFFFFF;
			                                        font-size:12pt;
			                                        background-color:#4BACC6;
		                                        }
		
		                                        .cellContent {
			                                        vertical-align:top;
			                                        text-align:center;
			                                        border-bottom:1px solid #000000;
			                                        border-top:1px solid #000000;
			                                        border-left:1px solid #000000;
			                                        border-right:1px solid #000000;
			                                        color:#000000;
			                                        font-size:11pt;
			                                        background-color:#DAEEF3;
		                                        }
                                            </style>
                                          </head>
                                          <body>
                                            <table>
                                                <col style='width:121.9999986pt;'>
                                                <col style='width:74.5555547pt;'>
                                                <col style='width:74.5555547pt;'>
                                                <col style='width:121.9999986pt;'>
                                                <col style='width:74.5555547pt;'>
                                                <col style='width:74.5555547pt;'>
                                                <tbody>
                                                  <tr>
                                                    <td class='cellHeader' colspan='6'>Данни спрямо " + periodStr + @"</td>
                                                  </tr>
                                                  <tr>
                                                    <td class='cellHeader'>Служител</td>
                                                    <td class='cellHeader'>Брой<br />задачи</td>
                                                    <td class='cellHeader'>Изработени<br />часове</td>
                                                    <td class='cellHeader'>Обща оценка на<br />завършени задачи</td>
                                                    <td class='cellHeader'>Отпуск</td>
                                                    <td class='cellHeader'>Завършени<br />курсове</td>
                                                  </tr>");

            foreach (var row in employeesPerformances)
            {
                // <span>&nbsp;</span> is needed in order to escape excel cell autoformatting

                excelContent.AppendLine(@"<tr>
                                             <td class='cellContent'>" + row.UserName + @"</td>
                                             <td class='cellContent'>" + row.TaskCount + @"</td>
                                             <td class='cellContent'><span>&nbsp;</span>" + row.HoursRatio + @"</td>
                                             <td class='cellContent'><span>&nbsp;</span>" + row.RateRatio + @"</td>
                                             <td class='cellContent'><span>&nbsp;</span>" + row.TimeOffRatio + @"</td>
                                             <td class='cellContent'><span>&nbsp;</span>" + row.CourseRatio + @"</td>
                                         </tr>
                                         ");
            }

            excelContent.AppendLine(@"</tbody>
                                    </table>
                                </body>
                            </html>");

            byte[] fileBytes = Encoding.UTF8.GetBytes(excelContent.ToString());

            return fileBytes;
        }
    }
}
