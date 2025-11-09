using AESP.Common.DTOs;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Service.Contract
{
    public interface IAvatarService
    {
        Task<ResponseDTO> UploadAvatarAsync(Guid userId, IFormFile file);
        Task<ResponseDTO> UpdateAvatarAsync(Guid userId, IFormFile file);
    }
}
