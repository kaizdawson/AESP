using AESP.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Service.Contract
{
    public interface IRecordCategoryService
    {
        Task<ResponseDTO> CreateCategoryAsync(Guid learnerProfileId, CreateRecordCategoryDTO dto);
        Task<ResponseDTO> RenameCategoryAsync(Guid learnerProfileId, Guid categoryId, string newName);
        Task<ResponseDTO> DeleteCategoryAsync(Guid learnerProfileId, Guid categoryId);
        Task<ResponseDTO> GetAllCategoriesAsync(Guid learnerProfileId);
    }

}
