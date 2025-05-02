using Quanlysinhvien.Helpers;
using Quanlysinhvien.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Quanlysinhvien.Views.Admin
{
    public partial class RoleManagementView : UserControl
    {
        private readonly DatabaseHelper dbHelper;
        private List<Role> _allRoles; // Danh sách đầy đủ
        private Role? selectedRole; // Cho phép null
        private const int PageSize = 25; // Số dòng mỗi trang
        private int _currentPage = 1; // Trang hiện tại
        private int _totalPages = 1; // Tổng số trang

        public RoleManagementView()
        {
            InitializeComponent();
            dbHelper = new DatabaseHelper();
            LoadRoles();
            InitializeForm();
        }

        private void InitializeForm()
        {
            IdTextBox.Text = GenerateNewRoleId();
            NameTextBox.Text = string.Empty;
            StatusCheckBox.IsChecked = false;
            selectedRole = null;
            RoleDataGrid.SelectedItem = null;
        }

        private void LoadRoles()
        {
            try
            {
                _allRoles = dbHelper.GetAllRoles() ?? new List<Role>();
                UpdatePagination();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách vai trò: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdatePagination()
        {
            try
            {
                // Tính tổng số trang
                _totalPages = (int)Math.Ceiling((double)_allRoles.Count / PageSize);

                // Đảm bảo trang hiện tại hợp lệ
                if (_currentPage < 1) _currentPage = 1;
                if (_currentPage > _totalPages && _totalPages > 0) _currentPage = _totalPages;

                // Lấy dữ liệu cho trang hiện tại
                var pageData = _allRoles
                    .Skip((_currentPage - 1) * PageSize)
                    .Take(PageSize)
                    .ToList();

                // Cập nhật DataGrid
                RoleDataGrid.ItemsSource = pageData;

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

        private string GenerateNewRoleId()
        {
            string maxId = dbHelper.GetMaxRoleId();
            if (string.IsNullOrEmpty(maxId) || !maxId.StartsWith("R") || !int.TryParse(maxId.Substring(1), out int idNumber))
            {
                return "R1"; // Default starting ID if table is empty or invalid
            }

            // Increment ID and check for existence
            int newId = idNumber + 1;
            while (dbHelper.CheckRoleIdExists($"R{newId:D3}"))
            {
                newId++;
            }
            return $"R{newId:D1}";
        }

        private void RoleDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedRole = RoleDataGrid.SelectedItem as Role;
            if (selectedRole != null)
            {
                IdTextBox.Text = selectedRole.Id;
                NameTextBox.Text = selectedRole.Name;
                StatusCheckBox.IsChecked = selectedRole.Status;
            }
            else
            {
                InitializeForm(); // Reset form if no role is selected
            }
        }

        private void AddRole_Click(object sender, RoutedEventArgs e)
        {
            if (selectedRole != null)
            {
                // Show confirmation message before resetting the form
                var result = MessageBox.Show("Mời bạn nhập thông tin role mới.", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    InitializeForm(); // Reset form for adding a new role
                }
                return;
            }

            try
            {
                if (string.IsNullOrWhiteSpace(NameTextBox.Text))
                {
                    MessageBox.Show("Vui lòng điền đầy đủ thông tin (Tên Role)!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var newRole = new Role
                {
                    Id = IdTextBox.Text,
                    Name = NameTextBox.Text,
                    Status = StatusCheckBox.IsChecked ?? false
                };

                dbHelper.AddRole(newRole);
                MessageBox.Show("Thêm vai trò thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadRoles();
                InitializeForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thêm vai trò: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditRole_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (selectedRole == null)
                {
                    MessageBox.Show("Vui lòng chọn vai trò để sửa!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (string.IsNullOrWhiteSpace(NameTextBox.Text))
                {
                    MessageBox.Show("Vui lòng điền đầy đủ thông tin (Tên Role)!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Prevent ID modification
                if (IdTextBox.Text != selectedRole.Id)
                {
                    MessageBox.Show("Không được phép sửa Mã Role!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    IdTextBox.Text = selectedRole.Id;
                    return;
                }

                var updatedRole = new Role
                {
                    Id = selectedRole.Id,
                    Name = NameTextBox.Text,
                    Status = StatusCheckBox.IsChecked ?? false
                };

                dbHelper.UpdateRole(updatedRole);
                MessageBox.Show("Sửa vai trò thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadRoles();
                InitializeForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi sửa vai trò: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteRole_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (selectedRole == null)
                {
                    MessageBox.Show("Vui lòng chọn vai trò để xóa!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa vai trò {selectedRole.Name}?",
                    "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    dbHelper.DeleteRole(selectedRole.Id);
                    MessageBox.Show("Xóa vai trò thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadRoles();
                    InitializeForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa vai trò: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}