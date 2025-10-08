using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Service.Contract
{
    public interface ICloudinaryService
    {
        Task<string> UploadImageAsync(IFormFile file, string folder);
        Task<string> UploadVideoAsync(IFormFile file, string folder);
        Task<(bool IsSuccess, string Url, string Message)> UploadFileAsync(IFormFile file, string folder);
    }
}
