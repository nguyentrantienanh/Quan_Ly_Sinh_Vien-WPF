using System.Windows.Controls;

namespace Quanlysinhvien.Views.User
{
    public partial class SubjectRegistrationView : UserControl
    {
        public SubjectRegistrationView(string studentId)
        {
            InitializeComponent();
            DataContext = new Quanlysinhvien.ViewModels.SubjectRegistrationViewModel(studentId);
        }
    }
}