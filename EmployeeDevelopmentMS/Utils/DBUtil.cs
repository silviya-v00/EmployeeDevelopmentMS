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

        public int GetCompanyIDByUserID(string userID)
        {
            int companyID = 0;
            var sqlConn = new SqlConnection(_connectionString);
            sqlConn.Open();

            try
            {
                string SQL = @"SELECT uc.CompanyID
                               FROM dbo.UserCompany uc
                               WHERE uc.UserID = @UserID";

                SqlCommand command = new SqlCommand(SQL, sqlConn);
                command.Parameters.Add("@UserID", System.Data.SqlDbType.NVarChar).Value = userID;

                SqlDataReader dataReader = command.ExecuteReader();

                if (dataReader.Read())
                {
                    companyID = (int)dataReader["CompanyID"];
                }
                dataReader.Close();
            }
            finally
            {
                sqlConn.Close();
            }

            return companyID;
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

        public List<RegularUser> GetFilteredUsers(SearchUser searchUser)
        {
            List<RegularUser> allUsers = new List<RegularUser>();
            var sqlConn = new SqlConnection(_connectionString);
            sqlConn.Open();

            string whereAdditions = "";

            if (!String.IsNullOrEmpty(searchUser.CompanyIDs))
            {
                whereAdditions += " AND c.CompanyID IN (" + searchUser.CompanyIDs + ")";
            }

            if (!String.IsNullOrEmpty(searchUser.FirstName))
            {
                whereAdditions += " AND u.FirstName LIKE N'%" + searchUser.FirstName + "%'";
            }

            if (!String.IsNullOrEmpty(searchUser.LastName))
            {
                whereAdditions += " AND u.LastName LIKE N'%" + searchUser.LastName + "%'";
            }

            if (!searchUser.RoleKey.Equals("ALL"))
            {
                whereAdditions += " AND r.Name = '" + searchUser.RoleKey + "'";
            }

            if (!searchUser.Status.Equals("ALL"))
            {
                string isActive = "0";
                if (searchUser.Status.Equals("ACTIVE"))
                    isActive = "1";

                whereAdditions += " AND u.IsActive = " + isActive;
            }

            try
            {
                string SQL = @"SELECT u.Id as UserID, c.CompanyName, u.FirstName, u.LastName, r.Name as RoleName, u.RegistrationDate, u.LastLoginDate, u.IsActive
                               FROM dbo.AspNetUsers u
                               INNER JOIN dbo.AspNetUserRoles ur ON u.Id = ur.UserId
                               INNER JOIN dbo.AspNetRoles r ON ur.RoleId = r.Id
                               LEFT OUTER JOIN dbo.UserCompany uc ON u.Id = uc.UserId
                               LEFT OUTER JOIN dbo.Companies c ON uc.CompanyID = c.CompanyID
                               WHERE r.Name <> 'ADMIN'" + whereAdditions + @"
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

        public List<Employee> GetEmployeesByCompanyID(int companyID)
        {
            List<Employee> taskEmployees = new List<Employee>();
            taskEmployees.Add(new Employee() { EmployeeID = "-1", EmployeeName = "" });

            var sqlConn = new SqlConnection(_connectionString);
            sqlConn.Open();

            try
            {
                string SQL = @"SELECT a.Id as EmployeeID, a.UserName as EmployeeName
                               FROM dbo.AspNetUsers a
                               INNER JOIN dbo.UserCompany uc ON a.Id = uc.UserID
                               INNER JOIN dbo.AspNetUserRoles ur ON a.Id = ur.UserId
                               INNER JOIN dbo.AspNetRoles r ON ur.RoleId = r.Id
                               WHERE r.Name <> 'ADMIN'
	                                 AND uc.CompanyID = @CompanyID
                               ORDER BY a.UserName";

                SqlCommand command = new SqlCommand(SQL, sqlConn);
                command.Parameters.Add("@CompanyID", System.Data.SqlDbType.Int).Value = companyID;

                SqlDataReader dataReader = command.ExecuteReader();

                while (dataReader.Read())
                {
                    Employee employee = new Employee();
                    employee.EmployeeID = dataReader["EmployeeID"].ToString();
                    employee.EmployeeName = dataReader["EmployeeName"].ToString();

                    taskEmployees.Add(employee);
                }
                dataReader.Close();
            }
            finally
            {
                sqlConn.Close();
            }

            return taskEmployees;
        }

        public List<EmployeeTask> GetAllTasksByUserID(string userID, bool isUserManager)
        {
            List<EmployeeTask> tasks = new List<EmployeeTask>();
            string whereAdditions = "WHERE ";

            if (isUserManager)
            {
                whereAdditions += @"a.CreatedByID = @UserID";
            }
            else
            {
                whereAdditions += @"a.EmployeeID = @UserID";
            }

            var sqlConn = new SqlConnection(_connectionString);
            sqlConn.Open();

            try
            {
                string SQL = @"SELECT a.TaskID, a.TaskTitle, a.TaskDescription, a.EmployeeID, b.UserName,
                               a.WorkedHours, a.EstimatedHours, a.RatePoints, a.IsRated,
							   CASE WHEN a.CompletedDate IS NULL
									THEN 0
									ELSE 1
							   END as IsCompleted
                               FROM dbo.Tasks a
                               INNER JOIN dbo.AspNetUsers b ON a.EmployeeID = b.Id
                               " + whereAdditions + @"
                               ORDER BY a.CreatedDate";

                SqlCommand command = new SqlCommand(SQL, sqlConn);
                command.Parameters.Add("@UserID", System.Data.SqlDbType.NVarChar).Value = userID;

                SqlDataReader dataReader = command.ExecuteReader();

                while (dataReader.Read())
                {
                    EmployeeTask task = new EmployeeTask();
                    task.TaskID = (int)dataReader["TaskID"];
                    task.TaskTitle = dataReader["TaskTitle"].ToString();
                    task.TaskDescription = dataReader["TaskDescription"].ToString();

                    Employee employee = new Employee();
                    employee.EmployeeID = dataReader["EmployeeID"].ToString();
                    employee.EmployeeName = dataReader["UserName"].ToString();
                    task.Employee = employee;

                    task.WorkedHours = dataReader["WorkedHours"] is int ? (int)dataReader["WorkedHours"] : 0;
                    task.EstimatedHours = (int)dataReader["EstimatedHours"];
                    task.RatePoints = dataReader["RatePoints"] is int ? (int)dataReader["RatePoints"] : 0;
                    task.IsRated = dataReader["IsRated"] is bool ? (bool)dataReader["IsRated"] : false;
                    task.IsCompleted = Convert.ToBoolean((int)dataReader["IsCompleted"]);

                    tasks.Add(task);
                }
                dataReader.Close();
            }
            finally
            {
                sqlConn.Close();
            }

            return tasks;
        }

        public void SaveTask(EmployeeTask task)
        {
            var sqlConn = new SqlConnection(_connectionString);
            sqlConn.Open();
            string SQL = "";

            try
            {
                if (task.TaskID.HasValue)
                {
                    SQL = @"UPDATE dbo.Tasks
                            SET TaskTitle		= @TaskTitle,
	                            TaskDescription	= @TaskDescription,
	                            EmployeeID		= @EmployeeID,
	                            EstimatedHours	= @EstimatedHours
                            WHERE TaskID = @TaskID";
                }
                else
                {
                    SQL = @"INSERT INTO dbo.Tasks (TaskTitle,TaskDescription,EmployeeID,EstimatedHours,CreatedDate,CreatedByID)
                            VALUES (@TaskTitle,@TaskDescription,@EmployeeID,@EstimatedHours,@CreatedDate,@CreatedByID)";
                }

                SqlCommand command = new SqlCommand(SQL, sqlConn);
                if (task.TaskID.HasValue)
                {
                    command.Parameters.Add("@TaskID", System.Data.SqlDbType.Int).Value = task.TaskID;
                }
                command.Parameters.Add("@TaskTitle", System.Data.SqlDbType.NVarChar).Value = task.TaskTitle;
                command.Parameters.Add("@TaskDescription", System.Data.SqlDbType.NVarChar).Value = task.TaskDescription;
                command.Parameters.Add("@EmployeeID", System.Data.SqlDbType.NVarChar).Value = task.Employee.EmployeeID;
                command.Parameters.Add("@EstimatedHours", System.Data.SqlDbType.Int).Value = task.EstimatedHours;
                command.Parameters.Add("@CreatedDate", System.Data.SqlDbType.DateTime).Value = DateTime.Now;
                command.Parameters.Add("@CreatedByID", System.Data.SqlDbType.NVarChar).Value = task.CreatedByID;

                command.ExecuteNonQuery();
            }
            finally
            {
                sqlConn.Close();
            }
        }

        public void DeleteTask(int taskID)
        {
            var sqlConn = new SqlConnection(_connectionString);
            sqlConn.Open();

            try
            {
                string SQL = @"DELETE FROM dbo.Tasks
                               WHERE TaskID = @TaskID";

                SqlCommand command = new SqlCommand(SQL, sqlConn);
                command.Parameters.Add("@TaskID", System.Data.SqlDbType.Int).Value = taskID;

                command.ExecuteNonQuery();
            }
            finally
            {
                sqlConn.Close();
            }
        }

        public void CompleteTask(EmployeeTask task)
        {
            var sqlConn = new SqlConnection(_connectionString);
            sqlConn.Open();

            try
            {
                string SQL = @"UPDATE dbo.Tasks
                               SET WorkedHours		= @WorkedHours,
	                               CompletedDate	= @CompletedDate
                               WHERE TaskID = @TaskID";

                SqlCommand command = new SqlCommand(SQL, sqlConn);
                command.Parameters.Add("@TaskID", System.Data.SqlDbType.Int).Value = task.TaskID;
                command.Parameters.Add("@WorkedHours", System.Data.SqlDbType.Int).Value = task.WorkedHours;
                command.Parameters.Add("@CompletedDate", System.Data.SqlDbType.DateTime).Value = DateTime.Now;

                command.ExecuteNonQuery();
            }
            finally
            {
                sqlConn.Close();
            }
        }

        public void RateTask(EmployeeTask task)
        {
            var sqlConn = new SqlConnection(_connectionString);
            sqlConn.Open();
            bool isRated = task.RatePoints.HasValue && task.RatePoints.Value != -1;

            try
            {
                string SQL = @"UPDATE dbo.Tasks
                               SET RatePoints		= @RatePoints,
	                               IsRated	        = @IsRated
                               WHERE TaskID = @TaskID";

                SqlCommand command = new SqlCommand(SQL, sqlConn);
                command.Parameters.Add("@TaskID", System.Data.SqlDbType.Int).Value = task.TaskID;
                command.Parameters.Add("@RatePoints", System.Data.SqlDbType.Int).Value = (isRated ? task.RatePoints.Value : DBNull.Value);
                command.Parameters.Add("@IsRated", System.Data.SqlDbType.Bit).Value = isRated;

                command.ExecuteNonQuery();
            }
            finally
            {
                sqlConn.Close();
            }
        }

        public List<UserPosition> GetUserPositionsByUserID(string userID)
        {
            List<UserPosition> userPositions = new List<UserPosition>();
            var sqlConn = new SqlConnection(_connectionString);
            sqlConn.Open();

            try
            {
                string SQL = @"SELECT a.PositionID, a.UserID, b.UserName, a.Position, a.Salary, a.StartDate, a.EndDate
                               FROM dbo.UserPosition a
                               INNER JOIN dbo.AspNetUsers b ON a.UserID = b.Id
                               WHERE a.UserID = @UserID
                               ORDER BY a.StartDate
                               ";

                SqlCommand command = new SqlCommand(SQL, sqlConn);
                command.Parameters.Add("@UserID", System.Data.SqlDbType.NVarChar).Value = userID;

                SqlDataReader dataReader = command.ExecuteReader();

                while (dataReader.Read())
                {
                    UserPosition position = new UserPosition();
                    position.PositionID = (int)dataReader["PositionID"];

                    RegularUser selectedUser = new RegularUser();
                    selectedUser.UserID = dataReader["UserID"].ToString();
                    selectedUser.UserName = dataReader["UserName"].ToString();
                    position.Employee = selectedUser;

                    position.Position = dataReader["Position"].ToString();
                    position.Salary = (int)dataReader["Salary"];
                    position.StartDate = (dataReader["StartDate"] is DateTime) ? (DateTime)dataReader["StartDate"] : null;
                    position.EndDate = (dataReader["EndDate"] is DateTime) ? (DateTime)dataReader["EndDate"] : null;

                    userPositions.Add(position);
                }
                dataReader.Close();
            }
            finally
            {
                sqlConn.Close();
            }

            return userPositions;
        }

        public void SaveNewPosition(UserPosition selectedUser)
        {
            var sqlConn = new SqlConnection(_connectionString);
            sqlConn.Open();

            try
            {
                string SQL = @"DECLARE @PrevPositionEndDate datetime
                               SET @PrevPositionEndDate = ISNULL(@PreviousPositionEndDate,@StartDate)

                               UPDATE dbo.UserPosition
                               SET EndDate = @PrevPositionEndDate
                               WHERE UserID = @UserID AND EndDate IS NULL
                               ";

                if (selectedUser.HasNewPositionRow)
                {
                    SQL += @"INSERT INTO dbo.UserPosition (UserID,Position,Salary,StartDate,EndDate,CreatedDate,CreatedByID)
                             VALUES (@UserID,@Position,@Salary,@StartDate,@EndDate,@CreatedDate,@CreatedByID)
                             ";
                }

                SqlCommand command = new SqlCommand(SQL, sqlConn);
                command.Parameters.Add("@UserID", System.Data.SqlDbType.NVarChar).Value = selectedUser.Employee.UserID;
                command.Parameters.Add("@PreviousPositionEndDate", System.Data.SqlDbType.DateTime).Value = selectedUser.PreviousPositionEndDate.HasValue ? selectedUser.PreviousPositionEndDate.Value : DBNull.Value;

                if (selectedUser.HasNewPositionRow)
                {
                    command.Parameters.Add("@Position", System.Data.SqlDbType.NVarChar).Value = String.IsNullOrEmpty(selectedUser.Position) ? DBNull.Value : selectedUser.Position;
                    command.Parameters.Add("@Salary", System.Data.SqlDbType.Int).Value = selectedUser.Salary.HasValue ? selectedUser.Salary.Value : DBNull.Value;
                    command.Parameters.Add("@StartDate", System.Data.SqlDbType.DateTime).Value = selectedUser.StartDate.HasValue ? selectedUser.StartDate.Value : DBNull.Value;
                    command.Parameters.Add("@EndDate", System.Data.SqlDbType.DateTime).Value = selectedUser.EndDate.HasValue ? selectedUser.EndDate.Value : DBNull.Value;
                    command.Parameters.Add("@CreatedDate", System.Data.SqlDbType.DateTime).Value = DateTime.Now;
                    command.Parameters.Add("@CreatedByID", System.Data.SqlDbType.NVarChar).Value = selectedUser.CreatedByID;
                }
                else
                {
                    command.Parameters.Add("@StartDate", System.Data.SqlDbType.DateTime).Value = DBNull.Value;
                }

                command.ExecuteNonQuery();
            }
            finally
            {
                sqlConn.Close();
            }
        }

        public List<UserTimeOff> GetUserTimeOffByUserID(string userID)
        {
            List<UserTimeOff> userTimeOffs = new List<UserTimeOff>();
            var sqlConn = new SqlConnection(_connectionString);
            sqlConn.Open();

            try
            {
                string SQL = @"SELECT a.TimeOffID, a.UserID, b.UserName, a.StartDate, a.EndDate
                               FROM dbo.UserTimeOff a
                               INNER JOIN dbo.AspNetUsers b ON a.UserID = b.Id
                               WHERE a.UserID = @UserID
                               ORDER BY a.StartDate
                               ";

                SqlCommand command = new SqlCommand(SQL, sqlConn);
                command.Parameters.Add("@UserID", System.Data.SqlDbType.NVarChar).Value = userID;

                SqlDataReader dataReader = command.ExecuteReader();

                while (dataReader.Read())
                {
                    UserTimeOff timeOff = new UserTimeOff();
                    timeOff.TimeOffID = (int)dataReader["TimeOffID"];

                    RegularUser selectedUser = new RegularUser();
                    selectedUser.UserID = dataReader["UserID"].ToString();
                    selectedUser.UserName = dataReader["UserName"].ToString();
                    timeOff.EmployeeOnTimeOff = selectedUser;

                    timeOff.StartDate = (DateTime)dataReader["StartDate"];
                    timeOff.EndDate = (DateTime)dataReader["EndDate"];

                    userTimeOffs.Add(timeOff);
                }
                dataReader.Close();
            }
            finally
            {
                sqlConn.Close();
            }

            return userTimeOffs;
        }

        public void AddNewTimeOff(UserTimeOff timeOff, string currentUserID)
        {
            var sqlConn = new SqlConnection(_connectionString);
            sqlConn.Open();

            try
            {
                string SQL = @"INSERT INTO dbo.UserTimeOff (UserID,StartDate,EndDate)
                               VALUES (@UserID,@StartDate,@EndDate)
                               ";

                SqlCommand command = new SqlCommand(SQL, sqlConn);
                command.Parameters.Add("@UserID", System.Data.SqlDbType.NVarChar).Value = currentUserID;
                command.Parameters.Add("@StartDate", System.Data.SqlDbType.DateTime).Value = timeOff.StartDate;
                command.Parameters.Add("@EndDate", System.Data.SqlDbType.DateTime).Value = timeOff.EndDate;
                
                command.ExecuteNonQuery();
            }
            finally
            {
                sqlConn.Close();
            }
        }

        public RegularUser GetPersonalInfoByUserID(string userID)
        {
            RegularUser userInfo = new RegularUser();
            var sqlConn = new SqlConnection(_connectionString);
            sqlConn.Open();

            try
            {
                string SQL = @"SELECT a.UserName, a.PhoneNumber
                               FROM dbo.AspNetUsers a
                               WHERE a.Id = @UserID";

                SqlCommand command = new SqlCommand(SQL, sqlConn);
                command.Parameters.Add("@UserID", System.Data.SqlDbType.NVarChar).Value = userID;

                SqlDataReader dataReader = command.ExecuteReader();


                if (dataReader.Read())
                {
                    userInfo.UserName = dataReader["UserName"].ToString();
                    userInfo.PhoneNumber = dataReader["PhoneNumber"].ToString();
                }
                dataReader.Close();
            }
            finally
            {
                sqlConn.Close();
            }

            return userInfo;
        }

        public void UpdatePersonalInfoByUserID(RegularUser userInfo, string userID)
        {
            var sqlConn = new SqlConnection(_connectionString);
            sqlConn.Open();

            try
            {
                string SQL = @"UPDATE dbo.AspNetUsers
                               SET UserName = @UserName,
                                   NormalizedUserName = @NormalizedUserName,
	                               PhoneNumber = @PhoneNumber
                               WHERE Id = @UserID";

                SqlCommand command = new SqlCommand(SQL, sqlConn);
                command.Parameters.Add("@UserID", System.Data.SqlDbType.NVarChar).Value = userID;
                command.Parameters.Add("@UserName", System.Data.SqlDbType.NVarChar).Value = userInfo.UserName;
                command.Parameters.Add("@NormalizedUserName", System.Data.SqlDbType.NVarChar).Value = userInfo.UserName.ToUpper();
                command.Parameters.Add("@PhoneNumber", System.Data.SqlDbType.NVarChar).Value = !String.IsNullOrEmpty(userInfo.PhoneNumber) ? userInfo.PhoneNumber : DBNull.Value;

                command.ExecuteNonQuery();
            }
            finally
            {
                sqlConn.Close();
            }
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
