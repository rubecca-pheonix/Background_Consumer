using System;
using Microsoft.Extensions.Logging;

namespace RabbitMQConsumer.Services
{
	public class OtpSender : IOtpSender
	{
        private readonly IUserDetailsService _userDetailsService;
        private readonly ILogger<OtpSender> _logger;

        public OtpSender(IUserDetailsService userDetailsService, ILogger<OtpSender> logger)
		{
            _userDetailsService = userDetailsService;
            _logger = logger;
		}

        public bool SendOtp(string username)
        {
            try
            {
                var emailId = _userDetailsService.GetUserEmail(username);
                if (string.IsNullOrEmpty(emailId))
                {
                    return false;
                }
                Random rad = new Random();
                var otp = rad.Next(00000, 99999);

                //Inreal life this will replace with calls to send otp to phone or email
                Console.Write(otp);
                return true;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }
    }
}

