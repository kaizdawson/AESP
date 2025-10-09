using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Common.DTOs.BusinessCode
{
    public enum BusinessCode
    {
        //  Success codes (2xxx)
        GET_DATA_SUCCESSFULLY = 2000,
        SIGN_UP_SUCCESSFULLY = 2001,
        SIGN_UP_FAILED = 2002,
        EXISTED_USER = 2003,
        UPDATE_SUCESSFULLY = 2004,
        INSERT_SUCESSFULLY = 2005,
        DELETE_SUCESSFULLY = 2006,

        //  Validation & logical errors (3xxx)
        VALIDATION_FAILED = 3000, // ❗ Thêm mới – dùng khi dữ liệu đầu vào không hợp lệ

        //  Error & exception codes (4xxx)
        AUTH_NOT_FOUND = 401,
        ACCESS_DENIED = 403,
        WRONG_PASSWORD = 405,
        EXCEPTION = 4000
    }
}
