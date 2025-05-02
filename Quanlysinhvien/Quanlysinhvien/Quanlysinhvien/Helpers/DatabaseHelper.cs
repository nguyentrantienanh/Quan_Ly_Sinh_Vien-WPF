using Microsoft.Data.SqlClient;
using Quanlysinhvien.Models;
using System;
using System.Collections.Generic;

namespace Quanlysinhvien.Helpers
{
    public class DatabaseHelper
    {
        private readonly string connectionString = "Server=DESKTOP-U7AT1OT;Initial Catalog=Quanlysinhvien;Trusted_Connection=True;TrustServerCertificate=True";

        // Authenticate user
        public User AuthenticateUser(string username, string password)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT * FROM [User] WHERE Username = @Username AND Password = @Password AND Status = 1";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@Password", password);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new User
                            {
                                IdStudent = reader["IdStudent"].ToString(),
                                Username = reader["Username"].ToString(),
                                Password = reader["Password"].ToString(),
                                Note = reader["Note"]?.ToString(),
                                Status = Convert.ToBoolean(reader["Status"]),
                                CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                                ModifiedAt = Convert.ToDateTime(reader["ModifiedAt"])
                            };
                        }
                    }
                }
            }
            return null;
        }

        // Get roles by user
        public List<string> GetRolesByUser(string idStudent)
        {
            List<string> roles = new List<string>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT r.Name FROM Role r LEFT JOIN UserRole ur ON r.Id = ur.IdRole WHERE ur.IdStudent = @IdStudent AND r.Name IS NOT NULL";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdStudent", idStudent);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            roles.Add(reader["Name"].ToString());
                        }
                    }
                }
            }
            return roles;
        }

        // Get all students
        public List<Student> GetAllStudents()
        {
            List<Student> students = new List<Student>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT s.*, p.Name AS ProvinceName FROM Student s LEFT JOIN Province p ON s.IdProvince = p.Id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            students.Add(new Student
                            {
                                Id = reader["Id"].ToString(),
                                Name = reader["Name"].ToString(),
                                BOF = Convert.ToDateTime(reader["BOF"]),
                                ProvinceId = reader["IdProvince"] != DBNull.Value ? reader["IdProvince"].ToString() : null,
                                ProvinceName = reader["ProvinceName"]?.ToString(),
                                Gender = Convert.ToBoolean(reader["Gender"]) ? "Nam" : "Nữ"
                            });
                        }
                    }
                }
            }
            return students;
        }

        // Get student by ID
        public Student GetStudentById(string id)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT s.*, p.Name AS ProvinceName FROM Student s LEFT JOIN Province p ON s.IdProvince = p.Id WHERE s.Id = @Id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Student
                            {
                                Id = reader["Id"].ToString(),
                                Name = reader["Name"].ToString(),
                                BOF = Convert.ToDateTime(reader["BOF"]),
                                ProvinceId = reader["IdProvince"] != DBNull.Value ? reader["IdProvince"].ToString() : null,
                                ProvinceName = reader["ProvinceName"]?.ToString(),
                                Gender = Convert.ToBoolean(reader["Gender"]) ? "Nam" : "Nữ"
                            };
                        }
                    }
                }
            }
            return null;
        }

        // Add a student
        public void AddStudent(Student student)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string checkQuery = "SELECT COUNT(*) FROM Student WITH (UPDLOCK, HOLDLOCK) WHERE Id = @Id";
                        using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn, transaction))
                        {
                            checkCmd.Parameters.AddWithValue("@Id", student.Id);
                            int count = (int)checkCmd.ExecuteScalar();
                            if (count > 0)
                            {
                                throw new Exception("Mã sinh viên đã tồn tại.");
                            }
                        }

                        string query = "INSERT INTO Student (Id, Name, BOF, IdProvince, Gender) VALUES (@Id, @Name, @BOF, @IdProvince, @Gender)";
                        using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@Id", student.Id);
                            cmd.Parameters.AddWithValue("@Name", student.Name ?? "");
                            cmd.Parameters.AddWithValue("@BOF", student.BOF);
                            cmd.Parameters.AddWithValue("@IdProvince", (object)student.ProvinceId ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Gender", student.Gender == "Nam" ? 1 : 0);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        // Update a student
        public void UpdateStudent(Student student)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "UPDATE Student SET Name = @Name, BOF = @BOF, IdProvince = @IdProvince, Gender = @Gender WHERE Id = @Id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", student.Id);
                    cmd.Parameters.AddWithValue("@Name", student.Name ?? "");
                    cmd.Parameters.AddWithValue("@BOF", student.BOF);
                    cmd.Parameters.AddWithValue("@IdProvince", (object)student.ProvinceId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Gender", student.Gender == "Nam" ? 1 : 0);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Delete a student
        public void DeleteStudent(string id)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string[] queries = {
                        "DELETE FROM Enrol WHERE IdStudent = @Id",
                        "DELETE FROM UserRole WHERE IdStudent = @Id",
                        "DELETE FROM [User] WHERE IdStudent = @Id",
                        "DELETE FROM Student WHERE Id = @Id"
                    };

                        foreach (var query in queries)
                        {
                            using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@Id", id);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        // Get max student ID
        public string GetMaxStudentId()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT MAX(CAST(Id AS BIGINT)) FROM Student WHERE ISNUMERIC(Id) = 1";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        object result = cmd.ExecuteScalar();
                        return result?.ToString();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in GetMaxStudentId: {ex.Message}");
                    return null;
                }
            }
        }

        // Check if student ID exists
        public bool CheckStudentIdExists(string id)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT COUNT(*) FROM Student WHERE Id = @Id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        // Get max role ID
        public string GetMaxRoleId()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT MAX(Id) FROM Role WHERE Id LIKE 'R[0-9]%'";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        object result = cmd.ExecuteScalar();
                        return result?.ToString();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in GetMaxRoleId: {ex.Message}");
                    return null;
                }
            }
        }

        // Check if role ID exists
        public bool CheckRoleIdExists(string id)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT COUNT(*) FROM Role WHERE Id = @Id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        // Get max subject ID
        public string GetMaxSubjectId()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT MAX(Id) FROM Subject WHERE Id LIKE 'MHCTT[0-9]%'";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        object result = cmd.ExecuteScalar();
                        return result?.ToString();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in GetMaxSubjectId: {ex.Message}");
                    return null;
                }
            }
        }

        // Get max province ID
        public int? GetMaxProvinceId()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT MAX(Id) FROM Province";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        object result = cmd.ExecuteScalar();
                        if (result == DBNull.Value)
                        {
                            return null;
                        }
                        return Convert.ToInt32(result);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in GetMaxProvinceId: {ex.Message}");
                    return null;
                }
            }
        }

        // Get all provinces
        public List<Province> GetAllProvinces()
        {
            List<Province> provinces = new List<Province>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT * FROM Province";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            provinces.Add(new Province
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Name = reader["Name"].ToString()
                            });
                        }
                    }
                }
            }
            return provinces;
        }

        // Add a province
        public void AddProvince(Province province)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "INSERT INTO Province (Id, Name) VALUES (@Id, @Name)";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", province.Id);
                    cmd.Parameters.AddWithValue("@Name", province.Name);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Update a province
        public void UpdateProvince(Province province)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "UPDATE Province SET Name = @Name WHERE Id = @Id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", province.Id);
                    cmd.Parameters.AddWithValue("@Name", province.Name);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Delete a province
        public void DeleteProvince(string id)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "DELETE FROM Province WHERE Id = @Id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", int.Parse(id));
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Get all users
        public List<User> GetAllUsers()
        {
            List<User> users = new List<User>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT u.*, s.Name AS StudentName FROM [User] u LEFT JOIN Student s ON u.IdStudent = s.Id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add(new User
                            {
                                IdStudent = reader["IdStudent"].ToString(),
                                Username = reader["Username"].ToString(),
                                Password = reader["Password"].ToString(),
                                Note = reader["Note"]?.ToString(),
                                Status = Convert.ToBoolean(reader["Status"]),
                                CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                                ModifiedAt = Convert.ToDateTime(reader["ModifiedAt"])
                            });
                        }
                    }
                }
            }
            return users;
        }

        // Add a user (optionally create student)
        public void AddUser(User user, Student student = null)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Add student if provided
                        if (student != null)
                        {
                            string checkQuery = "SELECT COUNT(*) FROM Student WITH (UPDLOCK, HOLDLOCK) WHERE Id = @Id";
                            using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn, transaction))
                            {
                                checkCmd.Parameters.AddWithValue("@Id", student.Id);
                                int count = (int)checkCmd.ExecuteScalar();
                                if (count > 0)
                                {
                                    throw new Exception("Mã sinh viên đã tồn tại.");
                                }
                            }

                            string studentQuery = "INSERT INTO Student (Id, Name, BOF, IdProvince, Gender) VALUES (@Id, @Name, @BOF, @IdProvince, @Gender)";
                            using (SqlCommand cmd = new SqlCommand(studentQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@Id", student.Id);
                                cmd.Parameters.AddWithValue("@Name", student.Name ?? "");
                                cmd.Parameters.AddWithValue("@BOF", student.BOF);
                                cmd.Parameters.AddWithValue("@IdProvince", (object)student.ProvinceId ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@Gender", student.Gender == "Nam" ? 1 : 0);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        // Add user
                        string userQuery = "INSERT INTO [User] (IdStudent, Username, Password, Note, Status, CreatedAt, ModifiedAt) " +
                                         "VALUES (@IdStudent, @Username, @Password, @Note, @Status, @CreatedAt, @ModifiedAt)";
                        using (SqlCommand cmd = new SqlCommand(userQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@IdStudent", user.IdStudent);
                            cmd.Parameters.AddWithValue("@Username", user.Username ?? "");
                            cmd.Parameters.AddWithValue("@Password", user.Password ?? "");
                            cmd.Parameters.AddWithValue("@Note", (object)user.Note ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Status", user.Status);
                            cmd.Parameters.AddWithValue("@CreatedAt", user.CreatedAt);
                            cmd.Parameters.AddWithValue("@ModifiedAt", user.ModifiedAt);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        // Update a user
        public void UpdateUser(User user)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "UPDATE [User] SET Username = @Username, Password = @Password, Note = @Note, Status = @Status, ModifiedAt = @ModifiedAt " +
                              "WHERE IdStudent = @IdStudent";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdStudent", user.IdStudent);
                    cmd.Parameters.AddWithValue("@Username", user.Username ?? "");
                    cmd.Parameters.AddWithValue("@Password", user.Password ?? "");
                    cmd.Parameters.AddWithValue("@Note", (object)user.Note ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", user.Status);
                    cmd.Parameters.AddWithValue("@ModifiedAt", user.ModifiedAt);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Delete a user (optionally delete student)
        public void DeleteUser(string idStudent, bool deleteStudent = false)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string[] queries = deleteStudent
                            ? new[] {
                            "DELETE FROM UserRole WHERE IdStudent = @Id",
                            "DELETE FROM [User] WHERE IdStudent = @Id",
                            "DELETE FROM Student WHERE Id = @Id"
                            }
                            : new[] {
                            "DELETE FROM UserRole WHERE IdStudent = @Id",
                            "DELETE FROM [User] WHERE IdStudent = @Id"
                            };

                        foreach (var query in queries)
                        {
                            using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@Id", idStudent);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        // Get all roles
        public List<Role> GetAllRoles()
        {
            List<Role> roles = new List<Role>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT * FROM Role";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            roles.Add(new Role
                            {
                                Id = reader["Id"].ToString(),
                                Name = reader["Name"].ToString(),
                                Status = Convert.ToBoolean(reader["Status"])
                            });
                        }
                    }
                }
            }
            return roles;
        }

        // Add a role
        public void AddRole(Role role)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "INSERT INTO Role (Id, Name, Status) VALUES (@Id, @Name, @Status)";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", role.Id);
                    cmd.Parameters.AddWithValue("@Name", role.Name);
                    cmd.Parameters.AddWithValue("@Status", role.Status);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Update a role
        public void UpdateRole(Role role)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "UPDATE Role SET Name = @Name, Status = @Status WHERE Id = @Id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", role.Id);
                    cmd.Parameters.AddWithValue("@Name", role.Name);
                    cmd.Parameters.AddWithValue("@Status", role.Status);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Delete a role
        public void DeleteRole(string id)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "DELETE FROM Role WHERE Id = @Id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Get all user roles
        public List<UserRole> GetAllUserRoles()
        {
            List<UserRole> userRoles = new List<UserRole>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
                SELECT ur.Id, ur.IdStudent, ur.IdRole, r.Name AS RoleName, s.Name AS StudentName
                FROM UserRole ur
                LEFT JOIN Role r ON ur.IdRole = r.Id
                LEFT JOIN Student s ON ur.IdStudent = s.Id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            userRoles.Add(new UserRole
                            {
                                Id = reader["Id"].ToString(),
                                IdStudent = reader["IdStudent"].ToString(),
                                IdRole = reader["IdRole"]?.ToString(),
                                RoleName = reader["RoleName"]?.ToString()
                            });
                        }
                    }
                }
            }
            return userRoles;
        }

        // Add a user role
        public void AddUserRole(UserRole userRole)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string checkQuery = "SELECT COUNT(*) FROM UserRole WITH (UPDLOCK, HOLDLOCK) WHERE Id = @Id";
                        using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn, transaction))
                        {
                            checkCmd.Parameters.AddWithValue("@Id", userRole.Id);
                            int count = (int)checkCmd.ExecuteScalar();
                            if (count > 0)
                            {
                                throw new Exception("Mã phân vai trò đã tồn tại.");
                            }
                        }

                        string query = "INSERT INTO UserRole (Id, IdStudent, IdRole) VALUES (@Id, @IdStudent, @IdRole)";
                        using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@Id", userRole.Id);
                            cmd.Parameters.AddWithValue("@IdStudent", userRole.IdStudent);
                            cmd.Parameters.AddWithValue("@IdRole", (object)userRole.IdRole ?? DBNull.Value);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        // Update a user role
        public void UpdateUserRole(UserRole userRole)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string query = "UPDATE UserRole SET IdStudent = @IdStudent, IdRole = @IdRole WHERE Id = @Id";
                        using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@Id", userRole.Id);
                            cmd.Parameters.AddWithValue("@IdStudent", userRole.IdStudent);
                            cmd.Parameters.AddWithValue("@IdRole", (object)userRole.IdRole ?? DBNull.Value);
                            int rowsAffected = cmd.ExecuteNonQuery();
                            if (rowsAffected == 0)
                            {
                                throw new Exception("Không tìm thấy phân vai trò để cập nhật.");
                            }
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        // Delete a user role
        public void DeleteUserRole(string id)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "DELETE FROM UserRole WHERE Id = @Id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Delete user role by student
        public void DeleteUserRoleByStudent(string idStudent)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "DELETE FROM UserRole WHERE IdStudent = @IdStudent";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdStudent", idStudent);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Get max user role ID
        public string GetMaxUserRoleId()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT MAX(CAST(Id AS BIGINT)) FROM UserRole WHERE ISNUMERIC(Id) = 1";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        object result = cmd.ExecuteScalar();
                        return result?.ToString();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in GetMaxUserRoleId: {ex.Message}");
                    return null;
                }
            }
        }

        // Get user role by student
        public UserRole GetUserRoleByStudent(string idStudent)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
                SELECT ur.Id, ur.IdStudent, ur.IdRole, r.Name AS RoleName, s.Name AS StudentName
                FROM UserRole ur
                LEFT JOIN Role r ON ur.IdRole = r.Id
                LEFT JOIN Student s ON ur.IdStudent = s.Id
                WHERE ur.IdStudent = @IdStudent";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdStudent", idStudent);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new UserRole
                            {
                                Id = reader["Id"].ToString(),
                                IdStudent = reader["IdStudent"].ToString(),
                                IdRole = reader["IdRole"]?.ToString(),
                                RoleName = reader["RoleName"]?.ToString()
                            };
                        }
                    }
                }
            }
            return null;
        }

        // Get all subjects
        public List<Subject> GetAllSubjects()
        {
            List<Subject> subjects = new List<Subject>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT * FROM Subject";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            subjects.Add(new Subject
                            {
                                Id = reader["Id"].ToString(),
                                Name = reader["Name"].ToString()
                            });
                        }
                    }
                }
            }
            return subjects;
        }

        // Add a subject
        public void AddSubject(Subject subject)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "INSERT INTO Subject (Id, Name) VALUES (@Id, @Name)";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", subject.Id);
                    cmd.Parameters.AddWithValue("@Name", subject.Name);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Update a subject
        public void UpdateSubject(Subject subject)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "UPDATE Subject SET Name = @Name WHERE Id = @Id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", subject.Id);
                    cmd.Parameters.AddWithValue("@Name", subject.Name);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Delete a subject
        public void DeleteSubject(string id)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "DELETE FROM Subject WHERE Id = @Id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Get all enrolments
        public List<Enrol> GetAllEnrols()
        {
            List<Enrol> enrols = new List<Enrol>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT e.*, s.Name AS StudentName, sub.Name AS SubjectName " +
                              "FROM Enrol e " +
                              "JOIN Student s ON e.IdStudent = s.Id " +
                              "JOIN Subject sub ON e.IdSubject = sub.Id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            enrols.Add(new Enrol
                            {
                                IdStudent = reader["IdStudent"].ToString(),
                                IdSubject = reader["IdSubject"].ToString(),
                                Mark = reader["Mark"] != DBNull.Value ? Convert.ToDecimal(reader["Mark"]) : (decimal?)null,
                                StudentName = reader["StudentName"].ToString(),
                                SubjectName = reader["SubjectName"].ToString()
                            });
                        }
                    }
                }
            }
            return enrols;
        }

        // Get enrolments by student
        public List<Enrol> GetEnrolsByStudent(string idStudent)
        {
            List<Enrol> enrols = new List<Enrol>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT e.*, s.Name AS StudentName, sub.Name AS SubjectName " +
                              "FROM Enrol e " +
                              "JOIN Student s ON e.IdStudent = s.Id " +
                              "JOIN Subject sub ON e.IdSubject = sub.Id " +
                              "WHERE e.IdStudent = @IdStudent";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdStudent", idStudent);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            enrols.Add(new Enrol
                            {
                                IdStudent = reader["IdStudent"].ToString(),
                                IdSubject = reader["IdSubject"].ToString(),
                                Mark = reader["Mark"] != DBNull.Value ? Convert.ToDecimal(reader["Mark"]) : (decimal?)null,
                                StudentName = reader["StudentName"].ToString(),
                                SubjectName = reader["SubjectName"].ToString()
                            });
                        }
                    }
                }
            }
            return enrols;
        }

        // Add an enrolment
        public void AddEnrol(Enrol enrol)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "INSERT INTO Enrol (IdStudent, IdSubject, Mark) VALUES (@IdStudent, @IdSubject, @Mark)";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdStudent", enrol.IdStudent);
                    cmd.Parameters.AddWithValue("@IdSubject", enrol.IdSubject);
                    cmd.Parameters.AddWithValue("@Mark", (object)enrol.Mark ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Update an enrolment
        public int UpdateEnrol(Enrol enrol)
        {
            if (enrol == null || string.IsNullOrEmpty(enrol.IdStudent) || string.IsNullOrEmpty(enrol.IdSubject))
            {
                Console.WriteLine("UpdateEnrol: Invalid enrol data");
                return 0;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "UPDATE Enrol SET Mark = @Mark WHERE IdStudent = @IdStudent AND IdSubject = @IdSubject";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdStudent", enrol.IdStudent.Trim());
                        cmd.Parameters.AddWithValue("@IdSubject", enrol.IdSubject.Trim());
                        cmd.Parameters.AddWithValue("@Mark", (object)enrol.Mark ?? DBNull.Value);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        Console.WriteLine($"UpdateEnrol: Rows affected={rowsAffected} for IdStudent={enrol.IdStudent}, IdSubject={enrol.IdSubject}, Mark={enrol.Mark}");

                        // Log similar records if update fails
                        if (rowsAffected == 0)
                        {
                            string debugQuery = "SELECT IdStudent, IdSubject, Mark FROM Enrol WHERE IdStudent LIKE @IdStudent OR IdSubject LIKE @IdSubject";
                            using (SqlCommand debugCmd = new SqlCommand(debugQuery, conn))
                            {
                                debugCmd.Parameters.AddWithValue("@IdStudent", $"%{enrol.IdStudent}%");
                                debugCmd.Parameters.AddWithValue("@IdSubject", $"%{enrol.IdSubject}%");
                                using (SqlDataReader reader = debugCmd.ExecuteReader())
                                {
                                    Console.WriteLine("Similar Enrol records:");
                                    while (reader.Read())
                                    {
                                        Console.WriteLine($"  IdStudent={reader["IdStudent"]}, IdSubject={reader["IdSubject"]}, Mark={reader["Mark"]}");
                                    }
                                }
                            }
                        }

                        return rowsAffected;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdateEnrol error: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        // Helper to check Enrol record existence
        public bool EnrolExists(string idStudent, string idSubject)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM Enrol WHERE IdStudent = @IdStudent AND IdSubject = @IdSubject";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdStudent", idStudent.Trim());
                        cmd.Parameters.AddWithValue("@IdSubject", idSubject.Trim());
                        int count = (int)cmd.ExecuteScalar();
                        Console.WriteLine($"EnrolExists: IdStudent={idStudent}, IdSubject={idSubject}, Exists={count > 0}");
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EnrolExists error: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }

        // Delete an enrolment
        public void DeleteEnrol(string idStudent, string idSubject)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "DELETE FROM Enrol WHERE IdStudent = @IdStudent AND IdSubject = @IdSubject";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdStudent", idStudent);
                    cmd.Parameters.AddWithValue("@IdSubject", idSubject);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Check if username exists
        public bool CheckUserExistsByUsername(string username, string excludeIdStudent = null)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT COUNT(*) FROM [User] WHERE Username = @Username";
                if (!string.IsNullOrEmpty(excludeIdStudent))
                {
                    query += " AND IdStudent != @IdStudent";
                }
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    if (!string.IsNullOrEmpty(excludeIdStudent))
                    {
                        cmd.Parameters.AddWithValue("@IdStudent", excludeIdStudent);
                    }
                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        // Check if subject is in use
        public bool IsSubjectInUse(string subjectId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT COUNT(*) FROM Enrol WHERE IdSubject = @IdSubject";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdSubject", subjectId);
                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        // Check if province is in use
        public bool IsProvinceInUse(string provinceId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT COUNT(*) FROM Student WHERE IdProvince = @IdProvince";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IdProvince", int.Parse(provinceId));
                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        // Get user by username and password
        public User GetUserByUsernameAndPassword(string username, string password)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT * FROM [User] WHERE Username = @Username AND Password = @Password AND Status = 1";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@Password", password);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new User
                            {
                                IdStudent = reader["IdStudent"].ToString(),
                                Username = reader["Username"].ToString(),
                                Password = reader["Password"].ToString()
                            };
                        }
                    }
                }
            }
            return null;
        }

        // Get student name
        public string GetStudentName(string idStudent)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT Name FROM Student WHERE Id = @Id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", idStudent);
                    object result = cmd.ExecuteScalar();
                    return result?.ToString() ?? "Không tìm thấy tên sinh viên";
                }
            }
        }
    }
}
