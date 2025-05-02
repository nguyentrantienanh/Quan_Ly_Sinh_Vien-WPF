using System.Windows;
using System.Windows.Controls;

namespace Quanlysinhvien.Views.Login
{
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
            // GHI CHÚ: Khởi tạo giao diện
            // Truyền giá trị PasswordBox vào ViewModel khi mật khẩu thay đổi
            PasswordBox.PasswordChanged += (s, e) =>
            {
                if (DataContext is Quanlysinhvien.ViewModels.LoginViewModel vm)
                {
                    vm.Password = PasswordBox.Password;
                    // GHI CHÚ: Cập nhật thủ công Password vào ViewModel vì PasswordBox không hỗ trợ ràng buộc trực tiếp
                }
            };
        }
    }
}
