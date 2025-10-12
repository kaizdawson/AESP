using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Common.DTOs
{
    public class CreateServicePackageDto
    {
        [Required(ErrorMessage = "Tên gói dịch vụ không được để trống.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Tên gói dịch vụ phải có từ 2 đến 100 ký tự.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Mô tả không được để trống.")]
        [MinLength(5, ErrorMessage = "Mô tả phải có ít nhất 5 ký tự.")]
        public string Description { get; set; }
        [Required(ErrorMessage = "Cấp độ không được để trống.")]
        public string? Level { get; set; }
        [Required(ErrorMessage = "Giá không được để trống.")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá phải >= 0.")]
        public double Price { get; set; }

        [Required(ErrorMessage = "Thời lượng không được để trống.")]
        [Range(1, int.MaxValue, ErrorMessage = "Thời lượng (ngày) phải >= 1.")]
        public int Duration { get; set; }

        [Required(ErrorMessage = "Số lượt review không được để trống.")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lần review phải >= 0.")]
        public int NumberOfReview { get; set; }
        [Required(ErrorMessage = "Trạng thái không được để trống.")]
        [RegularExpression(@"^(Active|Inactive)$", ErrorMessage = "Trạng thái chỉ được là 'Active' hoặc 'Inactive'.")]
        public string? Status { get; set; }
    }


    public class UpdateServicePackageDto
    {
        [Required(ErrorMessage = "Tên gói dịch vụ không được để trống.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Tên gói dịch vụ phải có từ 2 đến 100 ký tự.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Mô tả không được để trống.")]
        [MinLength(5, ErrorMessage = "Mô tả phải có ít nhất 5 ký tự.")]
        public string Description { get; set; }
        [Required(ErrorMessage = "Cấp độ không được để trống.")]
        public string? Level { get; set; }
        [Required(ErrorMessage = "Giá không được để trống.")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá phải >= 0.")]
        public double Price { get; set; }

        [Required(ErrorMessage = "Thời lượng không được để trống.")]
        [Range(1, int.MaxValue, ErrorMessage = "Thời lượng (ngày) phải >= 1.")]
        public int Duration { get; set; }

        [Required(ErrorMessage = "Số lượt review không được để trống.")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lần review phải >= 0.")]
        public int NumberOfReview { get; set; }
        [Required(ErrorMessage = "Trạng thái không được để trống.")]
        [RegularExpression(@"^(Active|Inactive)$", ErrorMessage = "Trạng thái chỉ được là 'Active' hoặc 'Inactive'.")]
        public string? Status { get; set; }
    }
}
