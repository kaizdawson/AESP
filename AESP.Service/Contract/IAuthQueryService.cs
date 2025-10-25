using AESP.Common.DTOs;
using AESP.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Service.Contract
{
    public interface IAuthQueryService
    {
        Task<ResponseDTO> GetUserInfoAsync(Guid userId);

    }
}
