using AESP.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Service.Contract
{
    public interface IExerciseService
    {
        Task<ResponseDTO> GetAllExercisesAsync(int pageNumber, int pageSize, Guid? chapterId = null, string? keyword = null);
        Task<ResponseDTO> GetExerciseByIdAsync(Guid id);
        Task<ResponseDTO> CreateExerciseAsync(Guid chapterId, CreateExerciseDTO request);
        Task<ResponseDTO> UpdateExerciseAsync(Guid id, UpdateExerciseDTO dto);
        Task<ResponseDTO> DeleteExerciseAsync(Guid id);
        Task<ResponseDTO> GetExercisesByChapterIdAsync(Guid chapterId);

    }
}
