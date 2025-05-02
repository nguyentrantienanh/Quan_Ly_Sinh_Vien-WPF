using Quanlysinhvien.Helpers;
using Quanlysinhvien.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;

namespace Quanlysinhvien.Views.Admin
{
    public partial class UserRoleManagementView : UserControl
    {
        private readonly DatabaseHelper dbHelper;
        private List<UserRole> _allUserRoles; // Danh sách đầy đủ
        private List<Quanlysinhvien.Models.User> users = new List<Quanlysinhvien.Models.User>();
        private List<Role> roles = new List<Role>();
        private UserRole selectedUserRole;
        private const int PageSize = 25; // Số dòng mỗi trang
        private int _currentPage = 1; // Trang hiện tại
        private int _totalPages = 1; // Tổng số trang

        public UserRoleManagementView()
        {
            InitializeComponent();
            dbHelper = new DatabaseHelper();
            LoadUsers();
            LoadRoles();
            LoadUserRoles();
            ClearForm();
        }

        private void LoadUsers()
        {
            try
            {
                users = dbHelper.GetAllUsers() ?? new List<Quanlysinhvien.Models.User>();
                IdStudentComboBox.ItemsSource = users.Select(u => new
                {
                    u.IdStudent,
                    DisplayName = $"{u.IdStudent} - {dbHelper.GetStudentName(u.IdStudent)} - {(_allUserRoles?.Any(ur => ur.IdStudent == u.IdStudent) == true ? "Đã phân vai trò" : "Chưa phân vai trò")}"
                }).ToList();
                IdStudentComboBox.DisplayMemberPath = "DisplayName";
                IdStudentComboBox.SelectedValuePath = "IdStudent";
                if (users.Count == 0)
                {
                    MessageBox.Show("Không có người dùng nào trong hệ thống.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách người dùng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadRoles()
        {
            try
            {
                roles = dbHelper.GetAllRoles() ?? new List<Role>();
                IdRoleComboBox.ItemsSource = roles;
                IdRoleComboBox.DisplayMemberPath = "Name";
                IdRoleComboBox.SelectedValuePath = "Id";
                if (roles.Count == 0)
                {
                    MessageBox.Show("Không có vai trò nào trong hệ thống. Vui lòng thêm vai trò trước!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách vai trò: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadUserRoles()
        {
            try
            {
                _allUserRoles = dbHelper.GetAllUserRoles() ?? new List<UserRole>();
                UpdatePagination();
                if (_allUserRoles.Count == 0)
                {
                    MessageBox.Show("Không có phân vai trò nào trong hệ thống.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                // Refresh IdStudentComboBox to update role status
                LoadUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách phân vai trò: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdatePagination()
        {
            try
            {
                // Tính tổng số trang
                _totalPages = (int)Math.Ceiling((double)_allUserRoles.Count / PageSize);

                // Đảm bảo trang hiện tại hợp lệ
                if (_currentPage < 1) _currentPage = 1;
                if (_currentPage > _totalPages && _totalPages > 0) _currentPage = _totalPages;

                // Lấy dữ liệu cho trang hiện tại
                var pageData = _allUserRoles
                    .Skip((_currentPage - 1) * PageSize)
                    .Take(PageSize)
                    .ToList();

                // Cập nhật DataGrid
                UserRoleDataGrid.ItemsSource = pageData;

                // Cập nhật thông tin phân trang
                PageInfoTextBlock.Text = _totalPages > 0 ? $"Trang {_currentPage}/{_totalPages}" : "Trang 0/0";
                PreviousButton.IsEnabled = _currentPage > 1;
                NextButton.IsEnabled = _currentPage < _totalPages;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật phân trang: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PreviousPage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_currentPage > 1)
                {
                    _currentPage--;
                    UpdatePagination();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi chuyển trang trước: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_currentPage < _totalPages)
                {
                    _currentPage++;
                    UpdatePagination();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi chuyển trang sau: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void IdStudentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (IdStudentComboBox.SelectedValue == null)
                {
                    ClearForm();
                    return;
                }

                string idStudent = IdStudentComboBox.SelectedValue.ToString();
                selectedUserRole = dbHelper.GetUserRoleByStudent(idStudent);

                if (selectedUserRole != null)
                {
                    IdTextBox.Text = selectedUserRole.Id;
                    IdRoleComboBox.SelectedValue = selectedUserRole.IdRole;
                }
                else
                {
                    IdTextBox.Text = string.Empty;
                    IdRoleComboBox.SelectedItem = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải thông tin vai trò: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UserRoleDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedUserRole = UserRoleDataGrid.SelectedItem as UserRole;
            if (selectedUserRole != null)
            {
                IdTextBox.Text = selectedUserRole.Id;
                IdStudentComboBox.SelectedValue = selectedUserRole.IdStudent;
                IdRoleComboBox.SelectedValue = selectedUserRole.IdRole;
            }
            else
            {
                ClearForm();
            }
        }

        private void AssignRole_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(IdStudentComboBox.SelectedValue?.ToString()) ||
                    string.IsNullOrWhiteSpace(IdRoleComboBox.SelectedValue?.ToString()))
                {
                    MessageBox.Show("Vui lòng chọn Mã SV và Vai trò!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string idStudent = IdStudentComboBox.SelectedValue.ToString();
                string idRole = IdRoleComboBox.SelectedValue.ToString();
                var existingUserRole = dbHelper.GetUserRoleByStudent(idStudent);

                if (existingUserRole != null)
                {
                    existingUserRole.IdRole = idRole;
                    dbHelper.UpdateUserRole(existingUserRole);
                    MessageBox.Show("Cập nhật vai trò thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    string newId = GenerateNewUserRoleId();
                    var newUserRole = new UserRole
                    {
                        Id = newId,
                        IdStudent = idStudent,
                        IdRole = idRole,
                        RoleName = roles.FirstOrDefault(r => r.Id == idRole)?.Name
                    };

                    dbHelper.AddUserRole(newUserRole);
                    MessageBox.Show("Phân vai trò thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                LoadUserRoles();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi phân vai trò: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteUserRole_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (UserRoleDataGrid.SelectedItem == null)
                {
                    MessageBox.Show("Vui lòng chọn phân vai trò để xóa!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                selectedUserRole = UserRoleDataGrid.SelectedItem as UserRole;
                var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa phân vai trò cho {selectedUserRole.IdStudent}?",
                    "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    dbHelper.DeleteUserRole(selectedUserRole.Id);
                    MessageBox.Show("Xóa phân vai trò thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadUserRoles();
                    ClearForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa phân vai trò: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GenerateNewUserRoleId()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection("Server=DESKTOP-U7AT1OT;Initial Catalog=Quanlysinhvien;Trusted_Connection=True;TrustServerCertificate=True"))
                {
                    conn.Open();
                    string query = "SELECT MAX(CAST(SUBSTRING(Id, 3, LEN(Id) - 2) AS INT)) FROM UserRole WHERE Id LIKE 'UR%'";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        object result = cmd.ExecuteScalar();
                        int maxNumber = result != DBNull.Value && result != null ? Convert.ToInt32(result) : 0;
                        return $"UR{(maxNumber + 1):D4}";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tạo mã phân vai trò: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return "UR0001"; // Fallback ID
            }
        }

        private void ClearForm()
        {
            IdTextBox.Text = string.Empty;
            IdStudentComboBox.SelectedItem = null;
            IdRoleComboBox.SelectedItem = null;
            selectedUserRole = null;
            UserRoleDataGrid.SelectedItem = null;
        }
    }
}