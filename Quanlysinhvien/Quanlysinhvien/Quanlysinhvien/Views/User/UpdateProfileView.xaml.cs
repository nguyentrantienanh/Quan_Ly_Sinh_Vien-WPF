using System.Windows;
using System.Windows.Controls;

namespace Quanlysinhvien.Views.User
{
    public partial class UpdateProfileView : UserControl
    {
        public UpdateProfileView(string studentId)
        {
            InitializeComponent();
            var viewModel = new Quanlysinhvien.ViewModels.UpdateProfileViewModel(studentId);
            DataContext = viewModel;

            // Đồng bộ PasswordBox với ViewModel
            NewPasswordBox.PasswordChanged += (s, e) =>
            {
                viewModel.NewPassword = NewPasswordBox.Password;
            };
        }
    }
}