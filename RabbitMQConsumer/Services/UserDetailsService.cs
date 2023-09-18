using System;
using Microsoft.Extensions.Logging;
using RabbitMQConsumer.Models;

namespace RabbitMQConsumer.Services
{
    public class UserDetailsService : IUserDetailsService
    {
        private readonly List<UserInfo> userInfos = new List<UserInfo>
        {
            new UserInfo
            {
                UserName = "Test1",
                EmailId = "Test1@test.com"
            },
            new UserInfo
            {
                UserName = "Test2",
                EmailId = "Test2@test.com"
            },
            new UserInfo
            {
                UserName = "Test3",
                EmailId = "Test3@test.com"
            },
        };
        private readonly ILogger<UserDetailsService> _logger;

        public UserDetailsService(ILogger<UserDetailsService> logger)
        {
            _logger = logger;
        }

        public string GetUserEmail(string username)
        {
            try
            {
                return userInfos.First(x => x.UserName == username).EmailId;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                return string.Empty;
            }
        }
    }
}

