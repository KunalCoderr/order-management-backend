using OrderManagement.DTOsModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManagement.Services.Contracts
{
    public interface IUserService
    {
        bool Register(UserDTO dto);
        string Login(UserDTO dto);
        bool IsTokenValid(string token);
    }
}
