using AESP.Common.DTOs;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Service.Contract
{
    public interface ICourseService
    {
        Task<ResponseDTO> GetAllCourseAsync(int pageNumber, int pageSize, string? level = null, string? keyword = null);
        Task<ResponseDTO> GetByCourseIdAsync(Guid id);
        Task<ResponseDTO> CreateCourseAsync(CreateCourseDTO request);
        Task<ResponseDTO> UpdateCourseAsync(Guid id, UpdateCourseDTO request);
        Task<ResponseDTO> DeleteCourseAsync(Guid id); // soft delete
    }
}
