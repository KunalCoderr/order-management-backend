using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManagement.Services.Contracts
{
    public interface ICacheService
    {
        void Set<T>(string key, T value, TimeSpan expiry);
        T Get<T>(string key);
        void Remove(string key);
    }
}
