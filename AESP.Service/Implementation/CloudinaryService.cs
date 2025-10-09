using AESP.Service.Contract;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Service.Implementation
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }

        public async Task<(bool IsSuccess, string Url, string Message)> UploadFileAsync(IFormFile file, string folder)
        {
            try
            {
                string ext = Path.GetExtension(file.FileName).ToLowerInvariant();

                RawUploadParams uploadParams;

                if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".gif" || ext == ".bmp" || ext == ".webp")
                {
                    uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(file.FileName, file.OpenReadStream()),
                        Folder = folder,
                        UseFilename = true,
                        UniqueFilename = false,
                        Overwrite = true
                    };
                }
                else
                {
                    uploadParams = new RawUploadParams
                    {
                        File = new FileDescription(file.FileName, file.OpenReadStream()),
                        Folder = folder,
                        UseFilename = true,
                        UniqueFilename = false,
                        Overwrite = true,
                       
                    };
                }

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                    return (true, uploadResult.SecureUrl.AbsoluteUri, "Upload thành công.");

                return (false, "", "Upload thất bại: " + uploadResult.Error?.Message);
            }
            catch (Exception ex)
            {
                return (false, "", "Lỗi: " + ex.Message);
            }
        }

        public async Task<string> UploadImageAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Không có file để upload.");

            await using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
                throw new Exception("Upload ảnh thất bại.");

            return uploadResult.SecureUrl.ToString();
        }

        public async Task<string> UploadVideoAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Không có file để upload.");

            await using var stream = file.OpenReadStream();

            var uploadParams = new VideoUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
                throw new Exception("Upload video thất bại.");

            return uploadResult.SecureUrl.ToString();
        }

    }
}
