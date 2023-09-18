using System;
namespace RabbitMQConsumer.Services
{
	public interface IUserDetailsService
	{
		string GetUserEmail(string username);
	}
}

