using Quanlysinhvien.Helpers;
using Quanlysinhvien.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Text;
using System.Globalization;

namespace Quanlysinhvien.Views.Admin
{
    public partial class EnrolManagementView : UserControl
    {
        private readonly DatabaseHelper dbHelper;
        private List<Enrol> _allEnrols = new List<Enrol>(); // Danh sách đầy đủ
        private List<Enrol> _filteredEnrols = new List<Enrol>(); // Danh sách sau khi lọc
        private const int PageSize = 25; // Số dòng mỗi trang
        private int _currentPage = 1; // Trang hiện tại
        private int _totalPages = 1; // Tổng số trang

        public EnrolManagementView()
        {
            InitializeComponent();
            dbHelper = new DatabaseHelper();
            LoadEnrols();
        }

        private void LoadEnrols()
        {
            try
            {
                _allEnrols = dbHelper.GetAllEnrols() ?? new List<Enrol>();
                _filteredEnrols = _allEnrols; // Khởi tạo danh sách lọc bằng danh sách đầy đủ
                UpdatePagination();
                Console.WriteLine($"Loaded {_allEnrols.Count} enrols:");
                foreach (var enrol in _allEnrols)
                {
                    Console.WriteLine($"  IdStudent={enrol.IdStudent}, IdSubject={enrol.IdSubject}, Mark={enrol.Mark}, StudentName={enrol.StudentName}, SubjectName={enrol.SubjectName}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LoadEnrols error: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show($"Lỗi khi tải danh sách REGISTER: {ex.Message}",
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                string searchText = SearchTextBox.Text?.Trim();
                if (string.IsNullOrEmpty(searchText))
                {
                    _filteredEnrols = _allEnrols;
                }
                else
                {
                    // Chuẩn hóa chuỗi tìm kiếm
                    string normalizedSearchText = NormalizeString(searchText);
                    _filteredEnrols = _allEnrols.Where(enrol =>
                        (enrol.IdStudent != null && enrol.IdStudent.Contains(searchText, StringComparison.OrdinalIgnoreCase)) ||
                        (enrol.StudentName != null && NormalizeString(enrol.StudentName).Contains(normalizedSearchText, StringComparison.OrdinalIgnoreCase))
                    ).ToList();
                }

                _currentPage = 1; // Reset về trang 1 khi tìm kiếm
                UpdatePagination();
                Console.WriteLine($"Search performed: Text={searchText}, FilteredItems={_filteredEnrols.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SearchTextBox_TextChanged error: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show($"Lỗi khi tìm kiếm đăng ký: {ex.Message}",
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdatePagination()
        {
            try
            {
                // Tính tổng số trang
                _totalPages = (int)Math.Ceiling((double)_filteredEnrols.Count / PageSize);

                // Đảm bảo trang hiện tại hợp lệ
                if (_currentPage < 1) _currentPage = 1;
                if (_currentPage > _totalPages && _totalPages > 0) _currentPage = _totalPages;

                // Lấy dữ liệu cho trang hiện tại
                var pageData = _filteredEnrols
                    .Skip((_currentPage - 1) * PageSize)
                    .Take(PageSize)
                    .ToList();

                // Cập nhật DataGrid
                EnrolDataGrid.ItemsSource = pageData;

                // Cập nhật thông tin phân trang
                PageInfoTextBlock.Text = _totalPages > 0 ? $"Trang {_currentPage}/{_totalPages}" : "Trang 0/0";
                PreviousButton.IsEnabled = _currentPage > 1;
                NextButton.IsEnabled = _currentPage < _totalPages;

                // Cập nhật giao diện
                UpdateNoResultsVisibility(pageData.Count);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdatePagination error: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show($"Lỗi khi cập nhật phân trang: {ex.Message}",
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
                Console.WriteLine($"PreviousPage_Click error: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show($"Lỗi khi chuyển trang trước: {ex.Message}",
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
                Console.WriteLine($"NextPage_Click error: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show($"Lỗi khi chuyển trang sau: {ex.Message}",
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateNoResultsVisibility(int itemCount)
        {
            NoResultsTextBlock.Visibility = itemCount == 0 && _filteredEnrols.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            EnrolDataGrid.Visibility = itemCount == 0 && _filteredEnrols.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        private string NormalizeString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Chuyển đổi chuỗi thành dạng không dấu
            string normalized = input.Normalize(NormalizationForm.FormD);
            var chars = normalized.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray();
            string noDiacritics = new string(chars).Normalize(NormalizationForm.FormC).ToLower();

            // Chuẩn hóa thêm: thay thế các ký tự tiếng Việt thành không dấu nếu cần
            noDiacritics = noDiacritics
                .Replace("đ", "d")
                .Replace("Đ", "D");

            return noDiacritics;
        }
    }
}