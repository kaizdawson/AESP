using AESP.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Common.Helpers
{
    public static class StatusHelper
    {
        private static readonly Dictionary<string, CourseStatus> _courseStatusMap =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["Pending"] = CourseStatus.Pending,
                ["Enrolled"] = CourseStatus.Enrolled,
                ["Completed"] = CourseStatus.Completed,
                ["Expired"] = CourseStatus.Expired,
                ["Cancelled"] = CourseStatus.Cancelled
            };

        public static CourseStatus ToCourseStatus(string? raw)
        {
            if (raw is null) return CourseStatus.Pending;
            return _courseStatusMap.TryGetValue(raw.Trim(), out var val)
                ? val
                : CourseStatus.Pending;
        }

        public static string ToDbString(this CourseStatus status) => status.ToString();

        public static bool EqualsCourseStatus(string? raw, CourseStatus target)
            => ToCourseStatus(raw) == target;

        public static bool InCourseStatus(string? raw, params CourseStatus[] list)
            => list.Any(x => EqualsCourseStatus(raw, x));
    }
}
