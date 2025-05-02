using Quanlysinhvien.Helpers;
using Quanlysinhvien.Models;
using Quanlysinhvien.Views.Admin;
using Quanlysinhvien.Views.Login;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Quanlysinhvien.Views.User
{
    public partial class UserMainView : UserControl
    {
        private string loggedInUserName;
        private readonly string currentStudentId; // Lưu IdStudent của sinh viên đang đăng nhập
        private readonly DatabaseHelper dbHelper;
        private readonly DispatcherTimer _timer;

        public string LoggedInUserName
        {
            get => loggedInUserName;
            set
            {
                loggedInUserName = value;
            }
        }

        public UserMainView(string userName, string studentId)
        {
            InitializeComponent();
            dbHelper = new DatabaseHelper();

            LoggedInUserName = userName;
            currentStudentId = studentId; // Lưu IdStudent

            this.DataContext = this;

            // Khởi tạo timer để cập nhật thời gian
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1) // Cập nhật mỗi giây
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();

            // Cập nhật thời gian ngay khi khởi tạo
            UpdateClock();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateClock();
        }

        private void UpdateClock()
        {
            RealTimeClock.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        }

        private void RegisterSubject_Click(object sender, RoutedEventArgs e)
        {
            UserContent.Content = new SubjectRegistrationView(currentStudentId);
        }

        private void ViewSubjects_Click(object sender, RoutedEventArgs e)
        {
            UserContent.Content = new UserSubjectListView(currentStudentId);
        }

        private void UpdateProfile_Click(object sender, RoutedEventArgs e)
        {
            UserContent.Content = new UpdateProfileView(currentStudentId);
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);
            window.Content = new LoginView();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Bạn có chắc muốn thoát?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown(); // Đóng ứng dụng
            }
        }
    }
}