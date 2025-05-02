using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quanlysinhvien.Models
{
    public class User
    {
        public string IdStudent { get; set; } // Mã sinh viên
        public string Username { get; set; } // Tên đăng nhập
        public string Password { get; set; } // Mật khẩu
        public string Note { get; set; } // Ghi chú
        public bool Status { get; set; } // Trạng thái (kích hoạt hay không)
        public DateTime CreatedAt { get; set; } // Ngày tạo
        public DateTime ModifiedAt { get; set; } // Ngày sửa
    }
}