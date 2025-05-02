using Quanlysinhvien.Helpers;
using Quanlysinhvien.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Quanlysinhvien.ViewModels
{
    public class SubjectRegistrationViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseHelper dbHelper;
        private string selectedSubjectId;
        private ObservableCollection<Subject> availableSubjects;
        private ObservableCollection<Enrol> registeredSubjects;
        private readonly string currentStudentId;

        public string SelectedSubjectId
        {
            get => selectedSubjectId;
            set
            {
                selectedSubjectId = value;
                OnPropertyChanged(nameof(SelectedSubjectId));
            }
        }

        public ObservableCollection<Subject> AvailableSubjects
        {
            get => availableSubjects;
            set
            {
                availableSubjects = value;
                OnPropertyChanged(nameof(AvailableSubjects));
            }
        }

        public ObservableCollection<Enrol> RegisteredSubjects
        {
            get => registeredSubjects;
            set
            {
                registeredSubjects = value;
                OnPropertyChanged(nameof(RegisteredSubjects));
            }
        }

        public ICommand RegisterSubjectCommand { get; }
        public ICommand UnregisterSubjectCommand { get; }

        public SubjectRegistrationViewModel(string studentId)
        {
            dbHelper = new DatabaseHelper();
            currentStudentId = studentId;

            // Khởi tạo danh sách môn học và môn học đã đăng ký
            AvailableSubjects = new ObservableCollection<Subject>(dbHelper.GetAllSubjects());
            RegisteredSubjects = new ObservableCollection<Enrol>(dbHelper.GetEnrolsByStudent(currentStudentId));

            // Lọc danh sách môn học khả dụng
            FilterAvailableSubjects();

            RegisterSubjectCommand = new RelayCommand(ExecuteRegisterSubject, CanExecuteRegisterSubject);
            UnregisterSubjectCommand = new RelayCommand(ExecuteUnregisterSubject, CanExecuteUnregisterSubject);
        }

        private void FilterAvailableSubjects()
        {
            var registeredSubjectIds = RegisteredSubjects.Select(e => e.IdSubject).ToList();
            var filteredSubjects = dbHelper.GetAllSubjects()
                .Where(s => !registeredSubjectIds.Contains(s.Id))
                .ToList();
            AvailableSubjects = new ObservableCollection<Subject>(filteredSubjects);
        }

        private bool CanExecuteRegisterSubject(object parameter)
        {
            return !string.IsNullOrEmpty(SelectedSubjectId);
        }

        private void ExecuteRegisterSubject(object parameter)
        {
            try
            {
                if (RegisteredSubjects.Any(e => e.IdSubject == SelectedSubjectId))
                {
                    MessageBox.Show("Môn học này đã được đăng ký!",
                        "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var enrol = new Enrol
                {
                    IdStudent = currentStudentId,
                    IdSubject = SelectedSubjectId,
                    Mark = null
                };

                dbHelper.AddEnrol(enrol);
                RegisteredSubjects = new ObservableCollection<Enrol>(dbHelper.GetEnrolsByStudent(currentStudentId));
                FilterAvailableSubjects();

                MessageBox.Show("Đăng ký môn học thành công!",
                    "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Lỗi khi đăng ký môn học: {ex.Message}",
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanExecuteUnregisterSubject(object parameter)
        {
            return parameter != null && !string.IsNullOrEmpty(parameter.ToString());
        }

        private void ExecuteUnregisterSubject(object parameter)
        {
            try
            {
                string subjectId = parameter.ToString();
                var enrolToRemove = RegisteredSubjects.FirstOrDefault(e => e.IdSubject == subjectId);

                if (enrolToRemove == null)
                {
                    MessageBox.Show("Môn học này chưa được đăng ký!",
                        "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                dbHelper.DeleteEnrol(currentStudentId, subjectId);
                RegisteredSubjects = new ObservableCollection<Enrol>(dbHelper.GetEnrolsByStudent(currentStudentId));
                FilterAvailableSubjects();

                MessageBox.Show("Hủy đăng ký môn học thành công!",
                    "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Lỗi khi hủy đăng ký môn học: {ex.Message}",
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}