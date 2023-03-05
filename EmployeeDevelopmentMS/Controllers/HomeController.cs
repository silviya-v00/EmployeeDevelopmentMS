using EmployeeDevelopmentMS.Models;
using EmployeeDevelopmentMS.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeDevelopmentMS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        private DBUtil _dbUtil;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _logger = logger;
            _configuration = configuration;
            _dbUtil = new DBUtil(_configuration.GetConnectionString("DefaultConnection"));
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Login()
        {
            ViewBag.IsLoginTabActive = true;
            ViewBag.IsRegisterTabActive = false;

            return View();
        }

        public IActionResult AdminHome()
        {
            if (!User.IsInRole("ADMIN"))
            {
                ModelState.AddModelError("invalidUserRole", "Нямате достъп до тази страница!");
            }

            return View();
        }

        public IActionResult ManagerHome()
        {
            if (!User.IsInRole("MANAGER"))
            {
                ModelState.AddModelError("invalidUserRole", "Нямате достъп до тази страница!");
            }

            return View();
        }

        public IActionResult EmployeeHome()
        {
            if (!User.IsInRole("EMPLOYEE"))
            {
                ModelState.AddModelError("invalidUserRole", "Нямате достъп до тази страница!");
            }

            return View();
        }

        public async Task<RedirectToActionResult> RedirectToHomeByRole(ApplicationUser user)
        {
            if (await _userManager.IsInRoleAsync(user, "ADMIN"))
            {
                return RedirectToAction("AdminHome", "Home");
            }
            else if (await _userManager.IsInRoleAsync(user, "MANAGER"))
            {
                return RedirectToAction("ManagerHome", "Home");
            }
            if (await _userManager.IsInRoleAsync(user, "EMPLOYEE"))
            {
                return RedirectToAction("EmployeeHome", "Home");
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Login(RegisterUser model)
        {
            var user = new ApplicationUser();

            if (_dbUtil.DoesUserNameExist(model.UserName))
            {
                var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, false, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    user = await _userManager.FindByNameAsync(model.UserName);
                    _logger.LogInformation("User logged in.");
                }

                return await RedirectToHomeByRole(user);
            }
            else
            {
                foreach (var modelValue in ModelState.Values)
                {
                    modelValue.Errors.Clear();
                }

                ModelState.AddModelError("invalidUser", "Грешно потребителско име или парола!");

                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterUser model)
        {
            var user = new ApplicationUser();

            if (_dbUtil.DoesUserNameExist(model.UserName))
            {
                ModelState.AddModelError("invalidRegistration", "Потребителското име е заето!");

                ViewBag.IsLoginTabActive = false;
                ViewBag.IsRegisterTabActive = true;

                return View("Login", model);
            }
            else
            {
                if (_dbUtil.DoesEmailExist(model.Email))
                {
                    ModelState.AddModelError("invalidRegistration", "Имейлът е зает!");

                    ViewBag.IsLoginTabActive = false;
                    ViewBag.IsRegisterTabActive = true;

                    return View("Login", model);
                }
                else
                {
                    if (_dbUtil.DoesCompanyExist(model.CompanyName))
                    {
                        ModelState.AddModelError("invalidRegistration", "Не може да се регистрирате като мениджър на компания, която вече е в системата под ръководството на някой друг!");

                        ViewBag.IsLoginTabActive = false;
                        ViewBag.IsRegisterTabActive = true;

                        return View("Login", model);
                    }
                    else
                    {
                        user = new ApplicationUser { UserName = model.UserName, Email = model.Email, FirstName = model.FirstName, LastName = model.LastName, PhoneNumber = model.PhoneNumber };

                        var result = await _userManager.CreateAsync(user, model.Password);
                        if (result.Succeeded)
                        {
                            await _userManager.AddToRoleAsync(user, "MANAGER");
                            await _signInManager.SignInAsync(user, isPersistent: false);

                            string userID = await _userManager.GetUserIdAsync(user);
                            int companyID = _dbUtil.InsertCompany(model.CompanyName);
                            _dbUtil.AddUserCompany(userID, companyID);
                        }
                        else
                        {
                            foreach (var error in result.Errors)
                            {
                                ModelState.AddModelError("invalidRegistration", error.Description);
                            }

                            ViewBag.IsLoginTabActive = false;
                            ViewBag.IsRegisterTabActive = true;

                            return View("Login", model);
                        }
                    }
                }

                return await RedirectToHomeByRole(user);
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
