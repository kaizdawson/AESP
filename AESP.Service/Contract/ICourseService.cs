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
        Task<ResponseDTO> GetAllAsync(int pageNumber, int pageSize, string? level = null, string? keyword = null);
        Task<ResponseDTO> GetByIdAsync(Guid id);
        Task<ResponseDTO> CreateAsync(CreateCourseDTO request);
        Task<ResponseDTO> UpdateAsync(Guid id, UpdateCourseDTO request);
        Task<ResponseDTO> DeleteAsync(Guid id); // soft delete
    }
}
