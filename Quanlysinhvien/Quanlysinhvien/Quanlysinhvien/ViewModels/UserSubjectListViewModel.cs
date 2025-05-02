using Quanlysinhvien.Helpers;
using Quanlysinhvien.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Quanlysinhvien.ViewModels
{
    public class UserSubjectListViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseHelper dbHelper;
        private ObservableCollection<Enrol> registeredSubjects;

        public ObservableCollection<Enrol> RegisteredSubjects
        {
            get => registeredSubjects;
            set
            {
                registeredSubjects = value;
                OnPropertyChanged(nameof(RegisteredSubjects));
            }
        }

        public UserSubjectListViewModel(string studentId)
        {
            dbHelper = new DatabaseHelper();
            RegisteredSubjects = new ObservableCollection<Enrol>(dbHelper.GetEnrolsByStudent(studentId));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}