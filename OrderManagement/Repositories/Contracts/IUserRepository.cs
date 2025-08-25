using OrderManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManagement.Repositories.Contracts
{
    public interface IUserRepository
    {
        User GetByUsername(string username);
        void Add(User user);
        void Save();
    }
}
