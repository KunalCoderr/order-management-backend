using OrderManagement.DTOsModels;
using OrderManagement.Models;
using OrderManagement.Repositories.Contracts;
using OrderManagement.Services.Contracts;
using OrderManagement.Utilities;
using System;
using System.Collections.Concurrent;

namespace OrderManagement.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;

        private static ConcurrentDictionary<string, (int id, string Username, DateTime Expiry)> _sessionStore
            = new ConcurrentDictionary<string, (int id, string Username, DateTime Expiry)>();

        public UserService(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        public bool Register(UserDTO dto)
        {
            try
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
            catch (Exception ex)
            {
                CommonUtils.CommonUtils.LogMessage(
                    $"Unexpected error during registration: {ex.Message}\n{ex.StackTrace}");

                throw;
            }
        }

        public string Login(UserDTO dto)
        {
            try
            {
                var user = _userRepo.GetByUsername(dto.Username);
                if (user == null) return null;

                bool isValid = PasswordHelper.VerifyPassword(dto.Password, user.PasswordHash, user.PasswordSalt);
                if (!isValid) return null;

                // Generate a new session token
                var token = Guid.NewGuid().ToString();
                var expiry = DateTime.UtcNow.AddHours(1);

                _sessionStore[token] = (user.Id, user.Username, expiry);

                return token;
            }
            catch (Exception ex)
            {
                CommonUtils.CommonUtils.LogMessage(
                    $"Unexpected error during login for '{dto.Username}': {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        public bool IsTokenValid(string token)
        {
            try
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
            catch (Exception ex)
            {
                CommonUtils.CommonUtils.LogMessage(
                    $"Unexpected error during token validation: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        public SessionInfo GetSession(string token)
        {
            try
            {
                if (_sessionStore.TryGetValue(token, out var session))
                {
                    if (session.Expiry < DateTime.UtcNow)
                    {
                        _sessionStore.TryRemove(token, out _);
                        return null;
                    }

                    return new SessionInfo
                    {
                        UserId = session.id,
                        Username = session.Username,
                        Expiry = session.Expiry
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                CommonUtils.CommonUtils.LogMessage(
                    $"Unexpected error during session retrieval for token '{token}': {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }
    }
}