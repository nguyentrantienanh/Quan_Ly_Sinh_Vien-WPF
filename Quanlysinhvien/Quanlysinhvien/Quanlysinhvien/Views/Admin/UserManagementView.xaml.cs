using Quanlysinhvien.Helpers;
using Quanlysinhvien.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Quanlysinhvien.Views.Admin
{
    public partial class UserManagementView : UserControl
    {
        private readonly DatabaseHelper _dbHelper;
        private List<Quanlysinhvien.Models.User> _allUsers;
        private List<Student> _students;
        private Quanlysinhvien.Models.User _selectedUser;
        private int _currentPage = 1;
        private int _totalPages = 1;
        private const int PageSize = 25;

        public UserManagementView()
        {
            InitializeComponent();
            _dbHelper = new DatabaseHelper();
            _allUsers = new List<Quanlysinhvien.Models.User>();
            _students = new List<Student>();
            LoadStudents();
            LoadUsers();
            ClearForm();
        }

        private void LoadStudents()
        {
            try
            {
                _students = _dbHelper.GetAllStudents() ?? new List<Student>();
                var comboBoxItems = _students.Select(s => new
                {
                    Id = s.Id,
                    DisplayText = _allUsers.Any(u => u.IdStudent == s.Id)
                        ? $"{s.Id} - Đã cấp tài khoản"
                        : $"{s.Id} - Chưa cấp tài khoản"
                }).ToList();

                IdStudentComboBox.ItemsSource = comboBoxItems;
                IdStudentComboBox.DisplayMemberPath = "DisplayText";
                IdStudentComboBox.SelectedValuePath = "Id";

                if (_students.Count == 0)
                {
                    MessageBox.Show("Không có sinh viên nào trong hệ thống.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách sinh viên: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadUsers()
        {
            try
            {
                _allUsers = _dbHelper.GetAllUsers() ?? new List<Quanlysinhvien.Models.User>();
                UpdatePagination();
                LoadStudents(); // Cập nhật lại danh sách sinh viên để hiển thị trạng thái tài khoản
                if (_allUsers.Count == 0)
                {
                    MessageBox.Show("Không có người dùng nào trong hệ thống.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách người dùng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdatePagination()
        {
            _totalPages = (int)Math.Ceiling((double)_allUsers.Count / PageSize);
            if (_currentPage < 1) _currentPage = 1;
            if (_currentPage > _totalPages && _totalPages > 0) _currentPage = _totalPages;

            var pageData = _allUsers
                .Skip((_currentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            UserDataGrid.ItemsSource = pageData;
            PageInfoTextBlock.Text = _totalPages > 0 ? $"Trang {_currentPage}/{_totalPages}" : "Trang 0/0";
            PreviousButton.IsEnabled = _currentPage > 1;
            NextButton.IsEnabled = _currentPage < _totalPages;
        }

        private void IdStudentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var selectedId = IdStudentComboBox.SelectedValue?.ToString();
                if (string.IsNullOrEmpty(selectedId))
                {
                    ClearStudentInfo();
                    ClearUserInfo();
                    return;
                }

                LoadStudentInfo(selectedId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi chọn sinh viên: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UserDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                _selectedUser = UserDataGrid.SelectedItem as Quanlysinhvien.Models.User;
                if (_selectedUser != null)
                {
                    IdStudentComboBox.SelectedValue = _selectedUser.IdStudent;
                    UsernameTextBox.Text = _selectedUser.Username;
                    PasswordBox.Password = _selectedUser.Password;
                    NoteTextBox.Text = _selectedUser.Note ?? string.Empty;
                    StatusCheckBox.IsChecked = _selectedUser.Status;
                    LoadStudentInfo(_selectedUser.IdStudent);
                }
                else
                {
                    ClearForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi chọn người dùng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadStudentInfo(string idStudent)
        {
            var student = _students.FirstOrDefault(s => s.Id == idStudent);
            if (student != null)
            {
                StudentNameTextBox.Text = student.Name;
                BOFDatePicker.SelectedDate = student.BOF;
                ProvinceTextBox.Text = student.ProvinceName ?? "Chưa chọn";
                GenderTextBox.Text = student.Gender;
            }
            else
            {
                ClearStudentInfo();
            }

            var user = _allUsers.FirstOrDefault(u => u.IdStudent == idStudent);
            if (user != null)
            {
                UsernameTextBox.Text = user.Username;
                PasswordBox.Password = user.Password;
                NoteTextBox.Text = user.Note ?? string.Empty;
                StatusCheckBox.IsChecked = user.Status;
                _selectedUser = user;
            }
            else
            {
                ClearUserInfo();
            }
        }

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Kiểm tra các trường bắt buộc
                if (string.IsNullOrWhiteSpace(UsernameTextBox.Text) || string.IsNullOrWhiteSpace(PasswordBox.Password))
                {
                    MessageBox.Show("Vui lòng điền Username và Password!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string idStudent = IdStudentComboBox.SelectedValue?.ToString();

                // Nếu không chọn mã sinh viên, tạo sinh viên mới
                if (string.IsNullOrEmpty(idStudent))
                {
                    idStudent = GenerateNewStudentId();
                    var newStudent = new Student
                    {
                        Id = idStudent,
                        Name = "",
                        BOF = DateTime.Now,
                        ProvinceId = null,
                        Gender = "Nam"
                    };
                    var newUser = new Quanlysinhvien.Models.User
                    {
                        IdStudent = idStudent,
                        Username = UsernameTextBox.Text,
                        Password = PasswordBox.Password,
                        Note = NoteTextBox.Text,
                        Status = StatusCheckBox.IsChecked ?? false,
                        CreatedAt = DateTime.Now,
                        ModifiedAt = DateTime.Now
                    };

                    // Kiểm tra username đã tồn tại
                    if (_dbHelper.CheckUserExistsByUsername(newUser.Username))
                    {
                        MessageBox.Show("Username đã tồn tại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Thêm người dùng và sinh viên mới
                    _dbHelper.AddUser(newUser, newStudent);
                    MessageBox.Show("Đã cấp tài khoản thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Kiểm tra xem sinh viên đã được cấp tài khoản chưa
                    if (_allUsers.Any(u => u.IdStudent == idStudent))
                    {
                        MessageBox.Show("Sinh viên này đã được cấp tài khoản!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    // Tạo người dùng mới với mã sinh viên đã chọn
                    var newUser = new Quanlysinhvien.Models.User
                    {
                        IdStudent = idStudent,
                        Username = UsernameTextBox.Text,
                        Password = PasswordBox.Password,
                        Note = NoteTextBox.Text,
                        Status = StatusCheckBox.IsChecked ?? false,
                        CreatedAt = DateTime.Now,
                        ModifiedAt = DateTime.Now
                    };

                    // Kiểm tra username đã tồn tại
                    if (_dbHelper.CheckUserExistsByUsername(newUser.Username))
                    {
                        MessageBox.Show("Username đã tồn tại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Thêm người dùng mới
                    _dbHelper.AddUser(newUser);
                    MessageBox.Show("Đã cấp tài khoản thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                // Cập nhật danh sách và làm mới form
                LoadUsers();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thêm người dùng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditUser_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_selectedUser == null)
                {
                    MessageBox.Show("Vui lòng chọn người dùng để sửa!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (string.IsNullOrWhiteSpace(UsernameTextBox.Text) || string.IsNullOrWhiteSpace(PasswordBox.Password))
                {
                    MessageBox.Show("Vui lòng điền Username và Password!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (_dbHelper.CheckUserExistsByUsername(UsernameTextBox.Text, _selectedUser.IdStudent))
                {
                    MessageBox.Show("Username đã tồn tại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _selectedUser.Username = UsernameTextBox.Text;
                _selectedUser.Password = PasswordBox.Password;
                _selectedUser.Note = NoteTextBox.Text;
                _selectedUser.Status = StatusCheckBox.IsChecked ?? false;
                _selectedUser.ModifiedAt = DateTime.Now;

                _dbHelper.UpdateUser(_selectedUser);
                MessageBox.Show("Sửa người dùng thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadUsers();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi sửa người dùng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_selectedUser == null)
                {
                    MessageBox.Show("Vui lòng chọn người dùng để xóa!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa người dùng {_selectedUser.Username}?",
                    "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    bool deleteStudent = !_dbHelper.CheckStudentIdExists(_selectedUser.IdStudent);
                    _dbHelper.DeleteUser(_selectedUser.IdStudent, deleteStudent);
                    MessageBox.Show("Xóa người dùng thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadUsers();
                    ClearForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa người dùng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                UpdatePagination();
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage < _totalPages)
            {
                _currentPage++;
                UpdatePagination();
            }
        }

        private string GenerateNewStudentId()
        {
            string maxId = _dbHelper.GetMaxStudentId();
            if (string.IsNullOrEmpty(maxId) || !long.TryParse(maxId, out long idNumber))
            {
                return "1361000";
            }

            long newId = idNumber + 1;
            while (_dbHelper.CheckStudentIdExists(newId.ToString()))
            {
                newId++;
            }
            return newId.ToString();
        }

        private void ClearForm()
        {
            IdStudentComboBox.SelectedItem = null;
            ClearStudentInfo();
            ClearUserInfo();
            UserDataGrid.SelectedItem = null;
            _selectedUser = null;
        }

        private void ClearStudentInfo()
        {
            StudentNameTextBox.Text = string.Empty;
            BOFDatePicker.SelectedDate = null;
            ProvinceTextBox.Text = string.Empty;
            GenderTextBox.Text = string.Empty;
        }

        private void ClearUserInfo()
        {
            UsernameTextBox.Text = string.Empty;
            PasswordBox.Password = string.Empty;
            NoteTextBox.Text = string.Empty;
            StatusCheckBox.IsChecked = false;
        }
    }
}