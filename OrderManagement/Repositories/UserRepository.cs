using OrderManagement.Models;
using OrderManagement.Repositories.Contracts;
using System;
using System.Linq;
using System.Web;

namespace OrderManagement.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly OrderManagementEntities _context;

        public UserRepository(OrderManagementEntities context)
        {
            _context = context;
        }

        public User GetByUsername(string username)
        {
            return _context.Users.FirstOrDefault(u => u.Username == username);
        }

        public void Add(User user)
        {
            _context.Users.Add(user);
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}