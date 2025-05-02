using System;
using System.Windows.Input;

namespace Quanlysinhvien.Helpers
{
    // GHI CHÚ: Định nghĩa lớp RelayCommand<T> triển khai ICommand, hỗ trợ tham số kiểu generic T
    public class RelayCommand<T> : ICommand
    {
        // GHI CHÚ: Biến lưu trữ phương thức thực thi lệnh
        private readonly Action<T> _execute;
        // GHI CHÚ: Biến lưu trữ phương thức kiểm tra điều kiện thực thi lệnh
        private readonly Func<T, bool> _canExecute;

        // GHI CHÚ: Constructor nhận hai tham số: phương thức kiểm tra (canExecute) và thực thi (execute)
        public RelayCommand(Func<T, bool> canExecute, Action<T> execute)
        {
            // GHI CHÚ: Kiểm tra null để đảm bảo các tham số hợp lệ
            _canExecute = canExecute ?? throw new ArgumentNullException(nameof(canExecute));
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        // GHI CHÚ: Sự kiện CanExecuteChanged để thông báo khi trạng thái lệnh thay đổi
        public event EventHandler CanExecuteChanged
        {
            // GHI CHÚ: Đăng ký sự kiện với CommandManager để tự động kiểm tra lại trạng thái
            add { CommandManager.RequerySuggested += value; }
            // GHI CHÚ: Hủy đăng ký sự kiện khi không cần thiết
            remove { CommandManager.RequerySuggested -= value; }
        }

        // GHI CHÚ: Kiểm tra xem lệnh có thể thực thi với tham số đầu vào hay không
        public bool CanExecute(object parameter)
        {
            // GHI CHÚ: Chuyển đổi tham số sang kiểu T và gọi phương thức _canExecute
            return _canExecute((T)parameter);
        }

        // GHI CHÚ: Thực thi lệnh với tham số đầu vào
        public void Execute(object parameter)
        {
            // GHI CHÚ: Chuyển đổi tham số sang kiểu T và gọi phương thức _execute
            _execute((T)parameter);
        }
    }
}