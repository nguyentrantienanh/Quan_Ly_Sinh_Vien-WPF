using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quanlysinhvien.Models
{
    public class UserRole
    {
        public string Id { get; set; } // Mã phân quyền
        public string IdStudent { get; set; } // Mã sinh viên
        public string IdRole { get; set; } // Mã vai trò
        public string Username { get; set; } // Tên đăng nhập (dùng để hiển thị)
        public string RoleName { get; set; } // Tên vai trò (dùng để hiển thị)
    }
}
