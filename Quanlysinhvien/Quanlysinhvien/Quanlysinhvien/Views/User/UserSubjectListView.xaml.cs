using System.Windows.Controls;

namespace Quanlysinhvien.Views.User
{
    public partial class UserSubjectListView : UserControl
    {
        public UserSubjectListView(string studentId)
        {
            InitializeComponent();
            DataContext = new Quanlysinhvien.ViewModels.UserSubjectListViewModel(studentId);
        }
    }
}