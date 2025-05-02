using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quanlysinhvien.Models
{
    public class Enrol
    {
        public string IdStudent { get; set; } // Mã sinh viên
        public string StudentName { get; set; } // Tên sinh viên (dùng để hiển thị)
        public string IdSubject { get; set; } // Mã môn học
        public string SubjectName { get; set; } // Tên môn học (dùng để hiển thị)
        public decimal? Mark { get; set; } // Điểm (có thể null nếu chưa có điểm)

    }
}