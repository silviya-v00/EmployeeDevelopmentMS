using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EmployeeDevelopmentMS.Utils
{
    public static class CommonUtil
    {
        public static int MaxVacationDays = 20;

        public static string ValidatePassword(string password, int requiredLength)
        {
            string errorMessage = "";
            int counter = 0;
            List<string> patterns = new List<string>();
            patterns.Add(@"[a-z]");
            patterns.Add(@"[A-Z]");
            patterns.Add(@"[0-9]");
            patterns.Add(@"[!@#$%^&*\(\)_\+\-\={}<>,\.\|""'~`:;\\?\/\[\] ]");

            // count type of different chars in password
            foreach (string p in patterns)
            {
                if (Regex.IsMatch(password, p))
                {
                    counter++;
                }
            }

            if (String.IsNullOrEmpty(password) || password.Length < requiredLength)
            {
                errorMessage = $"Невалидна парола! Паролата трябва да е с дължина поне {requiredLength} символа!";
            }
            else if (counter < patterns.Count)
            {
                errorMessage = "Невалидна парола! Моля използвайте: главни букви, малки букви, цифри и специални символи!";
            }

            return errorMessage;
        }

        public static string GetCorrectRoleName(string role)
        {
            string roleName = "";

            if (role.Equals("ADMIN"))
            {
                roleName = "Администратор";
            }
            else if (role.Equals("MANAGER"))
            {
                roleName = "Управител";
            }
            else if (role.Equals("EMPLOYEE"))
            {
                roleName = "Служител";
            }

            return roleName;
        }

        public static string CalculateExperience(DateTime? startDate, DateTime endDate)
        {
            string experience = "";

            if (startDate.HasValue)
            {
                int timeInCompany = 0;
                timeInCompany = ((endDate.Year - startDate.Value.Year) * 12) + endDate.Month - startDate.Value.Month;
                int yearsNum = timeInCompany / 12;
                int monthsNum = timeInCompany % 12;
                string yearsStr = (yearsNum != 0 ? (yearsNum.ToString() + " г.") : "");
                string monthsStr = (monthsNum != 0 ? (monthsNum.ToString() + " м.") : "");
                experience = yearsStr;
                experience += ((String.IsNullOrEmpty(yearsStr) || String.IsNullOrEmpty(monthsStr)) ? "" : " ");
                experience += monthsStr;

                if (!String.IsNullOrEmpty(experience))
                {
                    experience = " (" + experience + ")";
                }
            }

            return experience;
        }
    }
}
