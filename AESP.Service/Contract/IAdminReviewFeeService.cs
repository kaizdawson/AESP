using AESP.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Service.Contract
{
    public interface IAdminReviewFeeService
    {
        Task<ResponseDTO> CreateReviewFeePackageAndDetailAsync(CreateReviewFeePackageDto dto);
        Task<ResponseDTO> ScheduleNewReviewFeeDetailAsync(UpdateReviewFeeDetailDto dto);
        Task<ResponseDTO> GetAllReviewFeePackagesAsync(int pageNumber, int pageSize);
        Task<ResponseDTO> GetReviewFeePackageDetailAsync(Guid reviewFeeId);
        Task<ResponseDTO> GetAllReviewFeePackagesAsync();

    }
}
