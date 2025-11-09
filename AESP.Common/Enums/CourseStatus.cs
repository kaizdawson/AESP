using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Common.Enums
{
    public enum CourseStatus
    {
        Pending,     // Đăng ký nhưng chưa học
        Enrolled,    // Đang học
        Completed,   // Đã hoàn thành
        Expired,     // Quá hạn
        Cancelled    // Bị hủy
    }
}
