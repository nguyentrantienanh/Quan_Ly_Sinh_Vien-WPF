using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quanlysinhvien.Models
{
    public class Student
    {
        public string Id { get; set; } // Mã sinh viên
        public string Name { get; set; } // Họ tên
        public DateTime BOF { get; set; } // Ngày sinh
        public string ProvinceId { get; set; } // Mã tỉnh/TP
        public string ProvinceName { get; set; } // Tên tỉnh/TP (dùng để hiển thị)
        public string Gender { get; set; } // Giới tính
    }
}
