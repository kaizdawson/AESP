using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AESP.Common.DTOs
{
    public class UploadCertificateDto
    {
        [Required]
        [FromForm(Name = "name")]
        [SwaggerSchema("Tên chứng chỉ (ví dụ: TESOL, IELTS...)")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [FromForm(Name = "file")]
        [SwaggerSchema("File chứng chỉ (ảnh hoặc PDF)")]
        public IFormFile File { get; set; } = null!;
    }
}
