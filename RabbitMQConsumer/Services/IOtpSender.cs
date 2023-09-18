using System;
namespace RabbitMQConsumer.Services
{
	public interface IOtpSender
	{
		bool SendOtp(string username);
	}
}

