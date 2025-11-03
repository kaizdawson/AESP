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
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mô tả không được để trống.")]
        [MaxLength(1000, ErrorMessage = "Mô tả tối đa 1000 ký tự.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Giá không được để trống.")]
        [Range(1, double.MaxValue, ErrorMessage = "Giá phải > 0.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Số lượng coin không được để trống.")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng coin phải > 0.")]
        public int NumberOfCoin { get; set; }

        [Range(0, 100, ErrorMessage = "Phần trăm thưởng phải trong khoảng 0–100.")]
        public double BonusPercent { get; set; }

        [RegularExpression(@"^(Active|Inactive)$", ErrorMessage = "Trạng thái chỉ được là 'Active' hoặc 'Inactive'.")]
        public string? Status { get; set; } = "Active";
    }

    public class UpdateServicePackageDto
    {
        [Required(ErrorMessage = "Tên gói không được để trống.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Tên gói dịch vụ phải có từ 2–100 ký tự.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Mô tả không được để trống.")]
        [MaxLength(1000, ErrorMessage = "Mô tả tối đa 1000 ký tự.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Giá không được để trống.")]
        [Range(1, double.MaxValue, ErrorMessage = "Giá phải > 0.")]
        public double Price { get; set; }

        [Required(ErrorMessage = "Số coin không được để trống.")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng coin phải > 0.")]
        public int NumberOfCoin { get; set; }

        [Range(0, 100, ErrorMessage = "Phần trăm thưởng phải nằm trong khoảng 0–100.")]
        public double BonusPercent { get; set; }

        [RegularExpression(@"^(Active|Inactive)$", ErrorMessage = "Trạng thái chỉ được là 'Active' hoặc 'Inactive'.")]
        public string? Status { get; set; }
    }

}
