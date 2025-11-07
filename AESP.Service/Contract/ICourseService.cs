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
        Task<ResponseDTO> GetFullCourseByIdAsync(Guid id);
        Task<ResponseDTO> CreateFullCourseAsync(CreateCourseFullDTO request);
        Task<ResponseDTO> UpdateCourseAsync(Guid id, UpdateSimpleCourseDTO request);
        Task<ResponseDTO> DeleteFullCourseAsync(Guid id);

        Task<ResponseDTO> GetCoursesByLevelAsync(string level);


    }
}
