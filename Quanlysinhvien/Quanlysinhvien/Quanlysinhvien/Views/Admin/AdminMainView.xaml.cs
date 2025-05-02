using Quanlysinhvien.Views;
using Quanlysinhvien.Views.Admin;
using Quanlysinhvien.Views.Login;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Quanlysinhvien.Views.Admin
{
    public partial class AdminMainView : UserControl
    {
        private readonly DispatcherTimer _timer;

        public AdminMainView()
        {
            InitializeComponent();

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

        private void ManageStudent_Click(object sender, RoutedEventArgs e)
        {
            AdminContent.Content = new StudentManagementView();
        }

        private void ManageUser_Click(object sender, RoutedEventArgs e)
        {
            AdminContent.Content = new UserManagementView();
        }

        private void ManageRole_Click(object sender, RoutedEventArgs e)
        {
            AdminContent.Content = new RoleManagementView();
        }

        private void ManageUserRole_Click(object sender, RoutedEventArgs e)
        {
            AdminContent.Content = new UserRoleManagementView();
        }

        private void ManageSubject_Click(object sender, RoutedEventArgs e)
        {
            AdminContent.Content = new SubjectManagementView();
        }

        private void ManageProvince_Click(object sender, RoutedEventArgs e)
        {
            AdminContent.Content = new ProvinceManagementView();
        }

        private void ViewEnrolments_Click(object sender, RoutedEventArgs e)
        {
            AdminContent.Content = new EnrolManagementView();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            window.Content = new LoginView(); // Chuyển hướng về trang đăng nhập
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