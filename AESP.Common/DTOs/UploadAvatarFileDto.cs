using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Common.DTOs
{
    public class UploadAvatarFileDto
    {
        [Required]
        [FromForm(Name = "file")]
        [SwaggerSchema("Ảnh đại diện (JPG, PNG, WEBP, ...)")]
        public IFormFile File { get; set; } = null!;
    }
}
