using Quanlysinhvien.Helpers;
using Quanlysinhvien.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Quanlysinhvien.Views.Admin
{
    public partial class SubjectManagementView : UserControl
    {
        private readonly DatabaseHelper dbHelper;
        private List<Subject> _allSubjects; // Danh sách đầy đủ
        private Subject? selectedSubject; // Cho phép null
        private const int PageSize = 25; // Số dòng mỗi trang
        private int _currentPage = 1; // Trang hiện tại
        private int _totalPages = 1; // Tổng số trang

        public SubjectManagementView()
        {
            InitializeComponent();
            dbHelper = new DatabaseHelper();
            LoadSubjects();
            InitializeForm(); // Initialize form with a new ID
        }

        private void InitializeForm()
        {
            IdTextBox.Text = GenerateNewSubjectId(); // Set new auto-incremented ID
            NameTextBox.Text = string.Empty;
            selectedSubject = null;
            SubjectDataGrid.SelectedItem = null;
        }

        // Tải danh sách môn học vào DataGrid
        private void LoadSubjects()
        {
            try
            {
                _allSubjects = dbHelper.GetAllSubjects() ?? new List<Subject>(); // Xử lý null
                UpdatePagination();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách môn học: {ex.Message}",
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdatePagination()
        {
            try
            {
                // Tính tổng số trang
                _totalPages = (int)Math.Ceiling((double)_allSubjects.Count / PageSize);

                // Đảm bảo trang hiện tại hợp lệ
                if (_currentPage < 1) _currentPage = 1;
                if (_currentPage > _totalPages && _totalPages > 0) _currentPage = _totalPages;

                // Lấy dữ liệu cho trang hiện tại
                var pageData = _allSubjects
                    .Skip((_currentPage - 1) * PageSize)
                    .Take(PageSize)
                    .ToList();

                // Cập nhật DataGrid
                SubjectDataGrid.ItemsSource = pageData;

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

        // Generate a new subject ID starting with "MHCTT" followed by 5 digits
        private string GenerateNewSubjectId()
        {
            string maxId = dbHelper.GetMaxSubjectId();
            if (string.IsNullOrEmpty(maxId) || !maxId.StartsWith("MHCTT") || !int.TryParse(maxId.Substring(5), out int idNumber))
            {
                return "MHCTT00001"; // Default starting ID if table is empty or invalid
            }

            // Increment ID and format with leading zeros
            int newId = idNumber + 1;
            while (_allSubjects.Exists(s => s.Id == $"MHCTT{newId:D5}"))
            {
                newId++;
            }
            return $"MHCTT{newId:D5}";
        }

        // Xử lý sự kiện khi chọn một môn học trong DataGrid
        private void SubjectDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedSubject = SubjectDataGrid.SelectedItem as Subject;
            if (selectedSubject != null)
            {
                IdTextBox.Text = selectedSubject.Id;
                NameTextBox.Text = selectedSubject.Name;
            }
            else
            {
                InitializeForm(); // Reset form if no subject is selected
            }
        }

        // Nút Thêm
        private void AddSubject_Click(object sender, RoutedEventArgs e)
        {
            if (selectedSubject != null)
            {
                // Show confirmation message before resetting the form
                var result = MessageBox.Show("Mời bạn nhập thông tin môn học mới.", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    InitializeForm(); // Reset form for adding a new subject
                }
                return;
            }

            try
            {
                if (string.IsNullOrWhiteSpace(IdTextBox.Text) || string.IsNullOrWhiteSpace(NameTextBox.Text))
                {
                    MessageBox.Show("Vui lòng điền đầy đủ thông tin (Mã môn học, Tên môn học)!",
                        "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Kiểm tra xem môn học đã tồn tại với Id này chưa
                if (_allSubjects.Exists(s => s.Id == IdTextBox.Text))
                {
                    MessageBox.Show("Mã môn học đã tồn tại!",
                        "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var newSubject = new Subject
                {
                    Id = IdTextBox.Text,
                    Name = NameTextBox.Text
                };

                dbHelper.AddSubject(newSubject);
                MessageBox.Show("Thêm môn học thành công!",
                    "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadSubjects();
                InitializeForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thêm môn học: {ex.Message}",
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Nút Sửa
        private void EditSubject_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (selectedSubject == null)
                {
                    MessageBox.Show("Vui lòng chọn môn học để sửa!",
                        "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (string.IsNullOrWhiteSpace(IdTextBox.Text) || string.IsNullOrWhiteSpace(NameTextBox.Text))
                {
                    MessageBox.Show("Vui lòng điền đầy đủ thông tin (Mã môn học, Tên môn học)!",
                        "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Prevent ID modification during edit
                if (IdTextBox.Text != selectedSubject.Id)
                {
                    MessageBox.Show("Không được phép sửa Mã môn học!",
                        "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    IdTextBox.Text = selectedSubject.Id;
                    return;
                }

                selectedSubject.Name = NameTextBox.Text;

                dbHelper.UpdateSubject(selectedSubject);
                MessageBox.Show("Sửa môn học thành công!",
                    "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadSubjects();
                InitializeForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi sửa môn học: {ex.Message}",
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Nút Xóa
        private void DeleteSubject_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (selectedSubject == null)
                {
                    MessageBox.Show("Vui lòng chọn môn học để xóa!",
                        "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa môn học {selectedSubject.Name}?",
                    "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    // Kiểm tra xem môn học có được sử dụng trong bảng Enrol không
                    if (dbHelper.IsSubjectInUse(selectedSubject.Id))
                    {
                        MessageBox.Show("Môn học này đang được sử dụng trong bảng Enrol, không thể xóa!",
                            "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    dbHelper.DeleteSubject(selectedSubject.Id);
                    MessageBox.Show("Xóa môn học thành công!",
                        "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadSubjects();
                    InitializeForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa môn học: {ex.Message}",
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}