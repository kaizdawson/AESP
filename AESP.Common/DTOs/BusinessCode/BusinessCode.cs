using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Common.DTOs.BusinessCode
{
    public enum BusinessCode
    {
    GET_DATA_SUCCESSFULLY = 2000,
    EXCEPTION = 4000,
    SIGN_UP_SUCCESSFULLY = 2001,
    SIGN_UP_FAILED = 2002,
    EXISTED_USER = 2003,
    AUTH_NOT_FOUND = 401,
    WRONG_PASSWORD = 405,
    ACCESS_DENIED = 403,
    INSERT_SUCESSFULLY = 2005,
    UPDATE_SUCESSFULLY = 2004,
    DELETE_SUCESSFULLY = 2006,
    INVALID_INPUT = 2007,
    INVALID_ACTION = 2008,
    DATA_NOT_FOUND = 2009,
    ALREADY_ACTIVE = 3000,
    VALIDATION_ERROR = 3001,
    INTERNAL_ERROR = 3002


    }
}
