using AESP.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Service.Contract
{
    public interface IAdminUserService
    {
        Task<ResponseDTO> GetUsersByRoleAsync(string roleName);
        Task<ResponseDTO> GetUserDetailAsync(Guid userId);

    }
}
