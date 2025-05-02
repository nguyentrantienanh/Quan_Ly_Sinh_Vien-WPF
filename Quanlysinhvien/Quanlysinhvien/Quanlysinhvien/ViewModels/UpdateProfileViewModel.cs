using Quanlysinhvien.Helpers;
using Quanlysinhvien.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace Quanlysinhvien.ViewModels
{
    public class UpdateProfileViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseHelper dbHelper;
        private readonly string studentId;
        private string? name;
        private DateTime birthDate;
        private string? selectedProvinceId;
        private string? selectedGender;
        private string? newPassword;
        private List<Province>? provinces;
        private List<string>? genders;

        public string? Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public DateTime BirthDate
        {
            get => birthDate;
            set
            {
                birthDate = value;
                OnPropertyChanged(nameof(BirthDate));
            }
        }

        public string? SelectedProvinceId
        {
            get => selectedProvinceId;
            set
            {
                selectedProvinceId = value;
                OnPropertyChanged(nameof(SelectedProvinceId));
            }
        }

        public string? SelectedGender
        {
            get => selectedGender;
            set
            {
                selectedGender = value;
                OnPropertyChanged(nameof(SelectedGender));
            }
        }

        public string? NewPassword
        {
            get => newPassword;
            set
            {
                newPassword = value;
                OnPropertyChanged(nameof(NewPassword));
            }
        }

        public List<Province>? Provinces
        {
            get => provinces;
            set
            {
                provinces = value;
                OnPropertyChanged(nameof(Provinces));
            }
        }

        public List<string>? Genders
        {
            get => genders;
            set
            {
                genders = value;
                OnPropertyChanged(nameof(Genders));
            }
        }

        public RelayCommand<object> UpdateProfileCommand { get; }

        public UpdateProfileViewModel(string studentId)
        {
            this.studentId = studentId;
            dbHelper = new DatabaseHelper();

            // Khởi tạo danh sách tỉnh/thành và giới tính
            Provinces = dbHelper.GetAllProvinces();
            Genders = new List<string> { "Nam", "Nữ" };

            // Lấy thông tin sinh viên hiện tại
            LoadStudentInfo();

            UpdateProfileCommand = new RelayCommand<object>(CanExecuteUpdateProfile, ExecuteUpdateProfile);
        }

        private void LoadStudentInfo()
        {
            try
            {
                var student = dbHelper.GetAllStudents().FirstOrDefault(s => s.Id == studentId);
                if (student != null)
                {
                    Name = student.Name;
                    BirthDate = student.BOF;
                    SelectedProvinceId = student.ProvinceId;
                    SelectedGender = student.Gender;
                }
                else
                {
                    MessageBox.Show("Không tìm thấy thông tin sinh viên!",
                        "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải thông tin: {ex.Message}",
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanExecuteUpdateProfile(object parameter)
        {
            return !string.IsNullOrWhiteSpace(Name) &&
                   BirthDate != default &&
                   !string.IsNullOrWhiteSpace(SelectedProvinceId) &&
                   !string.IsNullOrWhiteSpace(SelectedGender);
        }

        private void ExecuteUpdateProfile(object parameter)
        {
            try
            {
                // Cập nhật thông tin sinh viên
                var student = new Student
                {
                    Id = studentId,
                    Name = Name,
                    BOF = BirthDate,
                    ProvinceId = SelectedProvinceId,
                    Gender = SelectedGender
                };
                dbHelper.UpdateStudent(student);

                // Cập nhật mật khẩu nếu có
                if (!string.IsNullOrWhiteSpace(NewPassword))
                {
                    var user = dbHelper.GetAllUsers().FirstOrDefault(u => u.IdStudent == studentId);
                    if (user != null)
                    {
                        user.Password = NewPassword;
                        user.ModifiedAt = DateTime.Now;
                        dbHelper.UpdateUser(user);
                    }
                }

                MessageBox.Show("Cập nhật thông tin thành công!",
                    "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật thông tin: {ex.Message}",
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}