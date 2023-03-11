using EmployeeDevelopmentMS.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static EmployeeDevelopmentMS.Areas.Identity.Pages.Account.RegisterModel;

namespace EmployeeDevelopmentMS.Utils
{
    public class DBUtil
    {
        private string _connectionString;
        public DBUtil(string connectionString)
        {
            _connectionString = connectionString;
        }

        public bool DoesUserNameExist(string userName)
        {
            bool userNameExists = false;
            var sqlConn = new SqlConnection(_connectionString);
            sqlConn.Open();

            try
            {
                string SQL = @"SELECT NULL
                               FROM dbo.AspNetUsers
                               WHERE UserName = @UserName";

                SqlCommand command = new SqlCommand(SQL, sqlConn);
                command.Parameters.Add("@UserName", System.Data.SqlDbType.NVarChar).Value = userName;

                SqlDataReader dataReader = command.ExecuteReader();

                if (dataReader.Read())
                {
                    userNameExists = true;
                }
                dataReader.Close();
            }
            finally
            {
                sqlConn.Close();
            }

            return userNameExists;
        }

        public bool DoesEmailExist(string email)
        {
            bool emailExists = false;
            var sqlConn = new SqlConnection(_connectionString);
            sqlConn.Open();

            try
            {
                string SQL = @"SELECT NULL
                               FROM dbo.AspNetUsers
                               WHERE Email = @Email";

                SqlCommand command = new SqlCommand(SQL, sqlConn);
                command.Parameters.Add("@Email", System.Data.SqlDbType.NVarChar).Value = email;

                SqlDataReader dataReader = command.ExecuteReader();

                if (dataReader.Read())
                {
                    emailExists = true;
                }
                dataReader.Close();
            }
            finally
            {
                sqlConn.Close();
            }

            return emailExists;
        }

        public bool DoesCompanyExist(string company)
        {
            bool companyExists = false;
            var sqlConn = new SqlConnection(_connectionString);
            sqlConn.Open();

            try
            {
                string SQL = @"SELECT NULL
                               FROM dbo.Companies
                               WHERE CompanyName = @CompanyName";

                SqlCommand command = new SqlCommand(SQL, sqlConn);
                command.Parameters.Add("@CompanyName", System.Data.SqlDbType.NVarChar).Value = company;

                SqlDataReader dataReader = command.ExecuteReader();

                if (dataReader.Read())
                {
                    companyExists = true;
                }
                dataReader.Close();
            }
            finally
            {
                sqlConn.Close();
            }

            return companyExists;
        }
                
        public int InsertCompany(string companyName)
        {
            int companyID = 0;
            var sqlConn = new SqlConnection(_connectionString);
            sqlConn.Open();

            try
            {
                string SQL = @"INSERT INTO dbo.Companies (CompanyName)
	                           VALUES (@CompanyName)
                                
                               SELECT @@IDENTITY as CompanyID";

                SqlCommand command = new SqlCommand(SQL, sqlConn);
                command.Parameters.Add("@CompanyName", System.Data.SqlDbType.NVarChar).Value = companyName;

                companyID = int.Parse(command.ExecuteScalar().ToString());
            }
            finally
            {
                sqlConn.Close();
            }

            return companyID;
        }

        public void AddUserCompany(string userID, int companyID)
        {
            var sqlConn = new SqlConnection(_connectionString);
            sqlConn.Open();

            try
            {
                string SQL = @"INSERT INTO dbo.UserCompany (UserID,CompanyID)
	                           VALUES (@UserID,@CompanyID)";

                SqlCommand command = new SqlCommand(SQL, sqlConn);
                command.Parameters.Add("@UserID", System.Data.SqlDbType.NVarChar).Value = userID;
                command.Parameters.Add("@CompanyID", System.Data.SqlDbType.Int).Value = companyID;

                command.ExecuteNonQuery();
            }
            finally
            {
                sqlConn.Close();
            }
        }

        public string ValidatePassword(string password, int requiredLength)
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
    }
}
