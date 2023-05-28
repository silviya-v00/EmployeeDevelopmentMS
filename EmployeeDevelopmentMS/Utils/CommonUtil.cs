using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EmployeeDevelopmentMS.Utils
{
    public static class CommonUtil
    {
        public const int MaxVacationDays = 20;
        public const int RequiredPasswordLength = 8;
        public const string UpperPattern = @"[A-Z]";
        public const string LowerPattern = @"[a-z]";
        public const string CharacterPattern = @"[!@#$%^&*\(\)_\+\-\={}<>,\.\|""'~`:;\\?\/\[\] ]";
        public const string NumberPattern = @"[0-9]";

        private static Random random = new Random();

        public static string GenerateRandomPassword()
        {
            StringBuilder password = new StringBuilder();

            password.Append(GenerateRandomCharacter(UpperPattern));
            password.Append(GenerateRandomCharacter(LowerPattern));
            password.Append(GenerateRandomCharacter(CharacterPattern));
            password.Append(GenerateRandomCharacter(NumberPattern));

            for (int i = password.Length; i < RequiredPasswordLength; i++)
            {
                char randomChar = GenerateRandomCharacter();
                password.Append(randomChar);
            }

            var shuffledPassword = new string(password.ToString().ToCharArray().OrderBy(_ => random.Next()).ToArray());

            return shuffledPassword;
        }

        private static char GenerateRandomCharacter()
        {
            const string allCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()_+-={}<>,.|\"'~`:;\\?/[] ";
            int randomIndex = random.Next(0, allCharacters.Length);
            return allCharacters[randomIndex];
        }

        private static char GenerateRandomCharacter(string pattern)
        {
            Regex regex = new Regex(pattern);
            char randomChar;

            do
            {
                randomChar = GenerateRandomCharacter();
            } while (!regex.IsMatch(randomChar.ToString()));

            return randomChar;
        }

        public static string ValidatePassword(string password, int requiredLength)
        {
            string errorMessage = "";
            int counter = 0;
            List<string> patterns = new List<string>();
            patterns.Add(LowerPattern);
            patterns.Add(UpperPattern);
            patterns.Add(NumberPattern);
            patterns.Add(CharacterPattern);

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
