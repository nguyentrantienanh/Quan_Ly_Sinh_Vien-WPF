using Quanlysinhvien.Helpers;
using Quanlysinhvien.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Quanlysinhvien.Views.Admin
{
    public partial class ProvinceManagementView : UserControl
    {
        private readonly DatabaseHelper dbHelper;
        private List<Province> _allProvinces; // Danh sách đầy đủ
        private Province? selectedProvince; // Cho phép null
        private const int PageSize = 25; // Số dòng mỗi trang
        private int _currentPage = 1; // Trang hiện tại
        private int _totalPages = 1; // Tổng số trang

        public ProvinceManagementView()
        {
            InitializeComponent();
            dbHelper = new DatabaseHelper();
            LoadProvinces();
            InitializeForm(); // Initialize form with a new ID
        }

        private void InitializeForm()
        {
            IdTextBox.Text = GenerateNewProvinceId().ToString(); // Set new auto-incremented ID
            NameTextBox.Text = string.Empty;
            selectedProvince = null;
            ProvinceDataGrid.SelectedItem = null;
        }

        // Tải danh sách tỉnh/thành phố vào DataGrid
        private void LoadProvinces()
        {
            try
            {
                _allProvinces = dbHelper.GetAllProvinces() ?? new List<Province>(); // Xử lý null
                UpdatePagination();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách tỉnh/thành phố: {ex.Message}",
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdatePagination()
        {
            try
            {
                // Tính tổng số trang
                _totalPages = (int)Math.Ceiling((double)_allProvinces.Count / PageSize);

                // Đảm bảo trang hiện tại hợp lệ
                if (_currentPage < 1) _currentPage = 1;
                if (_currentPage > _totalPages && _totalPages > 0) _currentPage = _totalPages;

                // Lấy dữ liệu cho trang hiện tại
                var pageData = _allProvinces
                    .Skip((_currentPage - 1) * PageSize)
                    .Take(PageSize)
                    .ToList();

                // Cập nhật DataGrid
                ProvinceDataGrid.ItemsSource = pageData;

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

        // Generate a new province ID as an incrementing number
        private int GenerateNewProvinceId()
        {
            int? maxId = dbHelper.GetMaxProvinceId();
            if (!maxId.HasValue)
            {
                return 1; // Default starting ID if table is empty
            }

            // Increment ID and check for existence
            int newId = maxId.Value + 1;
            while (_allProvinces.Exists(p => p.Id == newId))
            {
                newId++;
            }
            return newId;
        }

        // Xử lý sự kiện khi chọn một tỉnh/thành phố trong DataGrid
        private void ProvinceDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedProvince = ProvinceDataGrid.SelectedItem as Province;
            if (selectedProvince != null)
            {
                IdTextBox.Text = selectedProvince.Id.ToString();
                NameTextBox.Text = selectedProvince.Name;
            }
            else
            {
                InitializeForm(); // Reset form if no province is selected
            }
        }

        // Nút Thêm
        private void AddProvince_Click(object sender, RoutedEventArgs e)
        {
            if (selectedProvince != null)
            {
                // Show confirmation message before resetting the form
                var result = MessageBox.Show("Mời bạn nhập thông tin tỉnh/thành phố mới.", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    InitializeForm(); // Reset form for adding a new province
                }
                return;
            }

            try
            {
                if (string.IsNullOrWhiteSpace(IdTextBox.Text) || string.IsNullOrWhiteSpace(NameTextBox.Text))
                {
                    MessageBox.Show("Vui lòng điền đầy đủ thông tin (Mã tỉnh/TP, Tên tỉnh/TP)!",
                        "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!int.TryParse(IdTextBox.Text, out int newId))
                {
                    MessageBox.Show("Mã tỉnh/TP phải là một số nguyên!",
                        "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Kiểm tra xem tỉnh/thành phố đã tồn tại với Id này chưa
                if (_allProvinces.Exists(p => p.Id == newId))
                {
                    MessageBox.Show("Mã tỉnh/TP đã tồn tại!",
                        "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var newProvince = new Province
                {
                    Id = newId,
                    Name = NameTextBox.Text
                };

                dbHelper.AddProvince(newProvince);
                MessageBox.Show("Thêm tỉnh/thành phố thành công!",
                    "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadProvinces();
                InitializeForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thêm tỉnh/thành phố: {ex.Message}",
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Nút Sửa
        private void EditProvince_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (selectedProvince == null)
                {
                    MessageBox.Show("Vui lòng chọn tỉnh/thành phố để sửa!",
                        "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (string.IsNullOrWhiteSpace(IdTextBox.Text) || string.IsNullOrWhiteSpace(NameTextBox.Text))
                {
                    MessageBox.Show("Vui lòng điền đầy đủ thông tin (Mã tỉnh/TP, Tên tỉnh/TP)!",
                        "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!int.TryParse(IdTextBox.Text, out int updatedId))
                {
                    MessageBox.Show("Mã tỉnh/TP phải là một số nguyên!",
                        "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Prevent ID modification during edit
                if (updatedId != selectedProvince.Id)
                {
                    MessageBox.Show("Không được phép sửa Mã tỉnh/TP!",
                        "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    IdTextBox.Text = selectedProvince.Id.ToString();
                    return;
                }

                selectedProvince.Name = NameTextBox.Text;

                dbHelper.UpdateProvince(selectedProvince);
                MessageBox.Show("Sửa tỉnh/thành phố thành công!",
                    "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadProvinces();
                InitializeForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi sửa tỉnh/thành phố: {ex.Message}",
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Nút Xóa
        private void DeleteProvince_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (selectedProvince == null)
                {
                    MessageBox.Show("Vui lòng chọn tỉnh/thành phố để xóa!",
                        "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa tỉnh/thành phố {selectedProvince.Name}?",
                    "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    // Kiểm tra xem tỉnh/thành phố có được sử dụng trong bảng Student không
                    if (dbHelper.IsProvinceInUse(selectedProvince.Id.ToString()))
                    {
                        MessageBox.Show("Tỉnh/thành phố này đang được sử dụng trong bảng Student, không thể xóa!",
                            "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    dbHelper.DeleteProvince(selectedProvince.Id.ToString());
                    MessageBox.Show("Xóa tỉnh/thành phố thành công!",
                        "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadProvinces();
                    InitializeForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa tỉnh/thành phố: {ex.Message}",
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}