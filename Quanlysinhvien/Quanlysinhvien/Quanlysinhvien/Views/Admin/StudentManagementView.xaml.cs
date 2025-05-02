using Quanlysinhvien.Helpers;
using Quanlysinhvien.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Quanlysinhvien.Views.Admin
{
    public partial class StudentManagementView : UserControl
    {
        private readonly DatabaseHelper dbHelper;
        private List<Student> _allStudents; // Danh sách đầy đủ
        private List<Province> provinces;
        private Student selectedStudent;
        private const int PageSize = 25; // Số dòng mỗi trang
        private int _currentPage = 1; // Trang hiện tại
        private int _totalPages = 1; // Tổng số trang

        public StudentManagementView()
        {
            InitializeComponent();
            dbHelper = new DatabaseHelper();
            LoadProvinces();
            LoadStudents();
            InitializeForm();
        }

        private void InitializeForm()
        {
            // Set IdTextBox to next ID (read-only is set in XAML)
            IdTextBox.Text = GenerateNewStudentId();
            NameTextBox.Text = string.Empty;
            BOFDatePicker.SelectedDate = null;
            ProvinceComboBox.SelectedItem = null;
            GenderComboBox.SelectedItem = null;
            selectedStudent = null;
            StudentDataGrid.SelectedItem = null;
        }

        private void LoadProvinces()
        {
            provinces = dbHelper.GetAllProvinces();
            ProvinceComboBox.ItemsSource = provinces;
            ProvinceComboBox.DisplayMemberPath = "Name";
            ProvinceComboBox.SelectedValuePath = "Id";
        }

        private void LoadStudents()
        {
            try
            {
                _allStudents = dbHelper.GetAllStudents() ?? new List<Student>();
                UpdatePagination();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách sinh viên: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdatePagination()
        {
            try
            {
                // Tính tổng số trang
                _totalPages = (int)Math.Ceiling((double)_allStudents.Count / PageSize);

                // Đảm bảo trang hiện tại hợp lệ
                if (_currentPage < 1) _currentPage = 1;
                if (_currentPage > _totalPages && _totalPages > 0) _currentPage = _totalPages;

                // Lấy dữ liệu cho trang hiện tại
                var pageData = _allStudents
                    .Skip((_currentPage - 1) * PageSize)
                    .Take(PageSize)
                    .ToList();

                // Cập nhật DataGrid
                StudentDataGrid.ItemsSource = pageData;

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

        private string GenerateNewStudentId()
        {
            string maxId = dbHelper.GetMaxStudentId();
            if (string.IsNullOrEmpty(maxId) || !long.TryParse(maxId, out long idNumber))
            {
                return "1361000"; // Default starting ID if table is empty 
            }

            // Increment ID and check for existence
            long newId = idNumber + 1;
            while (dbHelper.CheckStudentIdExists(newId.ToString()))
            {
                newId++;
            }
            return newId.ToString();
        }

        private void StudentDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedStudent = StudentDataGrid.SelectedItem as Student;
            if (selectedStudent != null)
            {
                IdTextBox.Text = selectedStudent.Id;
                NameTextBox.Text = selectedStudent.Name;
                BOFDatePicker.SelectedDate = selectedStudent.BOF;
                ProvinceComboBox.SelectedValue = selectedStudent.ProvinceId;
                GenderComboBox.SelectedItem = GenderComboBox.Items.Cast<ComboBoxItem>()
                    .FirstOrDefault(item => item.Content.ToString() == selectedStudent.Gender);
            }
            else
            {
                InitializeForm(); // Reset form if no student is selected
            }
        }

        private void AddStudent_Click(object sender, RoutedEventArgs e)
        {
            if (selectedStudent != null)
            {
                // Show confirmation message before resetting the form
                var result = MessageBox.Show("Mời bạn nhập thông tin sinh viên mới.", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    InitializeForm(); // Reset form for adding a new student
                }
                return;
            }

            try
            {
                // Kiểm tra các trường bắt buộc
                if (string.IsNullOrWhiteSpace(NameTextBox.Text) ||
                    BOFDatePicker.SelectedDate == null ||
                    ProvinceComboBox.SelectedItem == null ||
                    GenderComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Vui lòng điền đầy đủ thông tin!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Kiểm tra ngày sinh không được trong tương lai
                if (BOFDatePicker.SelectedDate > DateTime.Now)
                {
                    MessageBox.Show("Ngày sinh không được là ngày trong tương lai!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var newStudent = new Student
                {
                    Id = IdTextBox.Text,
                    Name = NameTextBox.Text,
                    BOF = BOFDatePicker.SelectedDate.Value,
                    ProvinceId = ProvinceComboBox.SelectedValue.ToString(),
                    Gender = (GenderComboBox.SelectedItem as ComboBoxItem)?.Content.ToString()
                };

                dbHelper.AddStudent(newStudent);
                MessageBox.Show("Thêm sinh viên thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadStudents();
                InitializeForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thêm sinh viên: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void EditStudent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (selectedStudent == null)
                {
                    MessageBox.Show("Vui lòng chọn sinh viên để sửa!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (string.IsNullOrWhiteSpace(NameTextBox.Text) ||
                    BOFDatePicker.SelectedDate == null ||
                    ProvinceComboBox.SelectedItem == null ||
                    GenderComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Vui lòng điền đầy đủ thông tin!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var updatedStudent = new Student
                {
                    Id = selectedStudent.Id, // Keep original ID
                    Name = NameTextBox.Text,
                    BOF = BOFDatePicker.SelectedDate.Value,
                    ProvinceId = ProvinceComboBox.SelectedValue.ToString(),
                    Gender = (GenderComboBox.SelectedItem as ComboBoxItem)?.Content.ToString()
                };

                dbHelper.UpdateStudent(updatedStudent);
                MessageBox.Show("Sửa sinh viên thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadStudents();
                InitializeForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi sửa sinh viên: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteStudent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (selectedStudent == null)
                {
                    MessageBox.Show("Vui lòng chọn sinh viên để xóa!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa sinh viên {selectedStudent.Name}?",
                    "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    // Delete related records to avoid foreign key constraints
                    using (var conn = new Microsoft.Data.SqlClient.SqlConnection("Server=DESKTOP-U7AT1OT;Initial Catalog=Quanlysinhvien;Trusted_Connection=True;TrustServerCertificate=True"))
                    {
                        conn.Open();
                        string[] queries = {
                            "DELETE FROM Enrol WHERE IdStudent = @Id",
                            "DELETE FROM UserRole WHERE IdStudent = @Id",
                            "DELETE FROM [User] WHERE IdStudent = @Id",
                            "DELETE FROM Student WHERE Id = @Id"
                        };
                        foreach (var query in queries)
                        {
                            using (var cmd = new Microsoft.Data.SqlClient.SqlCommand(query, conn))
                            {
                                cmd.Parameters.AddWithValue("@Id", selectedStudent.Id);
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                    MessageBox.Show("Xóa sinh viên thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadStudents();
                    InitializeForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa sinh viên: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}