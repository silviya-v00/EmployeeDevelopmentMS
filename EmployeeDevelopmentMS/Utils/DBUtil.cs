using EmployeeDevelopmentMS.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
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

        public bool IsUserActive(string userName)
        {
            bool isUserActive = false;
            var sqlConn = new SqlConnection(_connectionString);
            sqlConn.Open();

            try
            {
                string SQL = @"SELECT NULL
                               FROM dbo.AspNetUsers
                               WHERE IsActive = 1
                                     AND UserName = @UserName";

                SqlCommand command = new SqlCommand(SQL, sqlConn);
                command.Parameters.Add("@UserName", System.Data.SqlDbType.NVarChar).Value = userName;

                SqlDataReader dataReader = command.ExecuteReader();

                if (dataReader.Read())
                {
                    isUserActive = true;
                }
                dataReader.Close();
            }
            finally
            {
                sqlConn.Close();
            }

            return isUserActive;
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

        public List<RegularUser> GetAllUsers()
        {
            List<RegularUser> allUsers = new List<RegularUser>();
            var sqlConn = new SqlConnection(_connectionString);
            sqlConn.Open();

            try
            {
                string SQL = @"SELECT u.Id as UserID, c.CompanyName, u.FirstName, u.LastName, r.Name as RoleName, u.RegistrationDate, u.LastLoginDate, u.IsActive
                               FROM dbo.AspNetUsers u
                               INNER JOIN dbo.AspNetUserRoles ur ON u.Id = ur.UserId
                               INNER JOIN dbo.AspNetRoles r ON ur.RoleId = r.Id
                               LEFT OUTER JOIN dbo.UserCompany uc ON u.Id = uc.UserId
                               LEFT OUTER JOIN dbo.Companies c ON uc.CompanyID = c.CompanyID
                               WHERE r.Name <> 'ADMIN'
                               ORDER BY c.CompanyName, r.Name, u.FirstName, u.LastName, u.RegistrationDate";

                SqlCommand command = new SqlCommand(SQL, sqlConn);
                SqlDataReader dataReader = command.ExecuteReader();

                while (dataReader.Read())
                {
                    RegularUser user = new RegularUser();
                    user.UserID = dataReader["UserID"].ToString();
                    user.CompanyName = dataReader["CompanyName"].ToString();
                    user.FirstName = dataReader["FirstName"].ToString();
                    user.LastName = dataReader["LastName"].ToString();
                    user.Role = new Role();
                    user.Role.RoleName = CommonUtil.GetCorrectRoleName(dataReader["RoleName"].ToString());
                    user.RegistrationDate = (dataReader["RegistrationDate"] is DateTime) ? (DateTime)dataReader["RegistrationDate"] : null;
                    user.LastLoginDate = (dataReader["LastLoginDate"] is DateTime) ? (DateTime)dataReader["LastLoginDate"] : null;
                    user.IsActive = (bool)dataReader["IsActive"];

                    allUsers.Add(user);
                }
                dataReader.Close();
            }
            finally
            {
                sqlConn.Close();
            }

            return allUsers;
        }

        public List<RegularUser> GetAllInactiveUsers()
        {
            List<RegularUser> allInactiveUsers = new List<RegularUser>();
            var sqlConn = new SqlConnection(_connectionString);
            sqlConn.Open();

            try
            {
                string SQL = @"SELECT u.Id as UserID, c.CompanyName, u.UserName, u.FirstName, u.LastName, u.Email, r.Name as RoleName, u.RegistrationDate, u.IsActive
                               FROM dbo.AspNetUsers u
                               INNER JOIN dbo.AspNetUserRoles ur ON u.Id = ur.UserId
                               INNER JOIN dbo.AspNetRoles r ON ur.RoleId = r.Id
                               LEFT OUTER JOIN dbo.UserCompany uc ON u.Id = uc.UserId
                               LEFT OUTER JOIN dbo.Companies c ON uc.CompanyID = c.CompanyID
                               WHERE r.Name <> 'ADMIN'
	                                 AND u.IsActive = 0
                               ORDER BY c.CompanyName, r.Name, u.FirstName, u.LastName";

                SqlCommand command = new SqlCommand(SQL, sqlConn);
                SqlDataReader dataReader = command.ExecuteReader();

                while (dataReader.Read())
                {
                    RegularUser user = new RegularUser();
                    user.UserID = dataReader["UserID"].ToString();
                    user.CompanyName = dataReader["CompanyName"].ToString();
                    user.UserName = dataReader["UserName"].ToString();
                    user.FirstName = dataReader["FirstName"].ToString();
                    user.LastName = dataReader["LastName"].ToString();
                    user.Email = dataReader["Email"].ToString();
                    user.Role = new Role();
                    user.Role.RoleName = CommonUtil.GetCorrectRoleName(dataReader["RoleName"].ToString());
                    user.RegistrationDate = (dataReader["RegistrationDate"] is DateTime) ? (DateTime)dataReader["RegistrationDate"] : null;
                    user.IsActive = (bool)dataReader["IsActive"];

                    allInactiveUsers.Add(user);
                }
                dataReader.Close();
            }
            finally
            {
                sqlConn.Close();
            }

            return allInactiveUsers;
        }

        public void UpdateUserActiveStatus(string userID, bool isActive)
        {
            var sqlConn = new SqlConnection(_connectionString);
            sqlConn.Open();

            try
            {
                string SQL = @"UPDATE dbo.AspNetUsers
                               SET IsActive = @IsActive
                               WHERE Id = @UserID";

                SqlCommand command = new SqlCommand(SQL, sqlConn);
                command.Parameters.Add("@UserID", System.Data.SqlDbType.NVarChar).Value = userID;
                command.Parameters.Add("@IsActive", System.Data.SqlDbType.Bit).Value = isActive;

                command.ExecuteNonQuery();
            }
            finally
            {
                sqlConn.Close();
            }
        }

        public List<Company> GetAllCompanies()
        {
            List<Company> allCompanies = new List<Company>();
            var sqlConn = new SqlConnection(_connectionString);
            sqlConn.Open();

            try
            {
                string SQL = @"SELECT c.CompanyID, c.CompanyName
                               FROM dbo.Companies c
                               ORDER BY c.CompanyName";

                SqlCommand command = new SqlCommand(SQL, sqlConn);
                SqlDataReader dataReader = command.ExecuteReader();

                while (dataReader.Read())
                {
                    Company companyRow = new Company();
                    companyRow.CompanyID = (int)dataReader["CompanyID"];
                    companyRow.CompanyName = dataReader["CompanyName"].ToString();

                    allCompanies.Add(companyRow);
                }
                dataReader.Close();
            }
            finally
            {
                sqlConn.Close();
            }

            return allCompanies;
        }
    }
}
