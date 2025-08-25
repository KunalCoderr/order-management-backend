using OrderManagement.DTOsModels;
using OrderManagement.Models;
using OrderManagement.Repositories.Contracts;
using OrderManagement.Services.Contracts;
using OrderManagement.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace OrderManagement.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;

        //private static Dictionary<string, (string Username, DateTime Expiry)> _sessionStore = new Dictionary<string, (string Username, DateTime Expiry)>();
        private static ConcurrentDictionary<string, (string Username, DateTime Expiry)> _sessionStore
            = new ConcurrentDictionary<string, (string Username, DateTime Expiry)>();

        public UserService(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        public bool Register(UserDTO dto)
        {
            if (_userRepo.GetByUsername(dto.Username) != null)
                return false;

            PasswordHelper.CreatePasswordHash(dto.Password, out string hash, out string salt);

            var user = new User
            {
                Username = dto.Username,
                PasswordHash = hash,
                PasswordSalt = salt,
                CreatedAt = DateTime.UtcNow
            };

            _userRepo.Add(user);
            _userRepo.Save();

            return true;
        }

        public string Login(UserDTO dto)
        {
            var user = _userRepo.GetByUsername(dto.Username);
            if (user == null) return null;

            bool isValid = PasswordHelper.VerifyPassword(dto.Password, user.PasswordHash, user.PasswordSalt);
            if (!isValid) return null;

            // Generate a new session token
            var token = Guid.NewGuid().ToString();
            var expiry = DateTime.UtcNow.AddHours(1);

            _sessionStore[token] = (user.Username, expiry);

            return token;
        }

        public bool IsTokenValid(string token)
        {
            if (string.IsNullOrEmpty(token)) return false;

            if (_sessionStore.TryGetValue(token, out var session))
            {
                if (session.Expiry > DateTime.UtcNow)
                    return true;
                else
                    _sessionStore.TryRemove(token, out _);
            }

            return false;
        }
    }
}