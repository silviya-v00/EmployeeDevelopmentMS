using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EmployeeDevelopmentMS.Utils
{
    public static class CommonUtil
    {
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
    }
}
