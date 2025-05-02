using Quanlysinhvien.Helpers;
using Quanlysinhvien.Models;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace Quanlysinhvien.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseHelper dbHelper;
        private string username;
        private string password;

        public string Username
        {
            get => username;
            set
            {
                username = value;
                OnPropertyChanged(nameof(Username));
            }
        }

        public string Password
        {
            get => password;
            set
            {
                password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        public ICommand LoginCommand { get; }
        public ICommand ExitCommand { get; } // Thêm ExitCommand

        public LoginViewModel()
        {
            dbHelper = new DatabaseHelper();
            LoginCommand = new RelayCommand(ExecuteLogin, CanExecuteLogin);
            ExitCommand = new RelayCommand(ExecuteExit); // Khởi tạo ExitCommand
        }

        private bool CanExecuteLogin(object parameter)
        {
            return !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);
        }

        private void ExecuteLogin(object parameter)
        {
            try
            {
                var user = dbHelper.GetUserByUsernameAndPassword(Username, Password);
                if (user == null)
                {
                    MessageBox.Show("Tên đăng nhập hoặc mật khẩu không đúng!",
                        "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var roles = dbHelper.GetRolesByUser(user.IdStudent);
                if (roles == null || roles.Count == 0)
                {
                    MessageBox.Show("Người dùng chưa được phân vai trò!",
                        "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (parameter is Window window)
                {
                    if (roles.Contains("admin"))
                    {
                        window.Content = new Quanlysinhvien.Views.Admin.AdminMainView();
                    }
                    else if (roles.Contains("user"))
                    {
                        string studentName = dbHelper.GetStudentName(user.IdStudent);
                        window.Content = new Quanlysinhvien.Views.User.UserMainView(studentName, user.IdStudent);
                    }
                    else
                    {
                        MessageBox.Show("Vai trò không hợp lệ! Vai trò phải là 'admin' hoặc 'user'.",
                            "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Lỗi khi đăng nhập: {ex.Message}",
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Thêm phương thức ExecuteExit
        private void ExecuteExit(object parameter)
        {
            if (parameter is Window window)
            {
                window.Close(); // Đóng cửa sổ đăng nhập
            }
            else
            {
                Application.Current.Shutdown(); // Đóng ứng dụng nếu không tìm thấy cửa sổ
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> execute;
        private readonly Func<object, bool> canExecute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return canExecute == null || canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            execute(parameter);
        }
    }
}