using AESP.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Service.Contract
{
    public interface IChapterService
    {
        Task<ResponseDTO> GetAllChaptersAsync(int pageNumber, int pageSize, Guid? courseId = null, string? keyword = null);
        Task<ResponseDTO> GetChapterByIdAsync(Guid id);
        Task<ResponseDTO> CreateChapterAsync(CreateChapterDTO dto);
        Task<ResponseDTO> UpdateChapterAsync(Guid id, UpdateChapterDTO dto);
        Task<ResponseDTO> DeleteChapterAsync(Guid id);
    }
}
