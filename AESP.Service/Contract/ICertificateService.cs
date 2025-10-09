using AESP.Common.DTOs;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Service.Contract
{
    public interface ICertificateService
    {
        Task<ResponseDTO> UploadCertificateAsync(Guid reviewerProfileId, IFormFile file, string certificateName);
        Task<ResponseDTO> GetByReviewerProfileIdAsync(Guid reviewerProfileId);
        Task<ResponseDTO> DeleteAsync(Guid certificateId);

    }
}
