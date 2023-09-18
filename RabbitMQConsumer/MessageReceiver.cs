using System;
using System.Collections;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQConsumer.Models;
using RabbitMQConsumer.Services;

namespace RabbitMQConsumer
{
    public class MessageReceiver : DefaultBasicConsumer
    {
        private const int MaxRetries = 5;
        private readonly IModel _channel;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MessageReceiver> _logger;

        public MessageReceiver(IModel channel, IServiceProvider serviceProvider, ILogger<MessageReceiver> logger)
        {
            _channel = channel;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }
        public override void HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange,
            string routingKey, IBasicProperties properties, ReadOnlyMemory<Byte> body)
        {
            try
            {
                int noOfRetries = 0;
                if (properties.Headers != null)
                {
                    noOfRetries = Convert.ToInt32(((Dictionary<string, object>)((List<object>)properties.Headers
                        .First(x => x.Key == "x-death").Value).First())
                        .First(x => x.Key == "count").Value);
                }

                string utfString = Encoding.UTF8.GetString(body.ToArray(), 0, body.ToArray().Length);

                var message = JsonSerializer.Deserialize<LoginNotificationMessage>(utfString);

                bool isOtpSent = false;

                if (!string.IsNullOrEmpty(message?.Username))
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var otpService = scope.ServiceProvider.GetRequiredService<IOtpSender>();
                        isOtpSent = otpService.SendOtp(message.Username);
                    }
                }
                if (isOtpSent || noOfRetries == MaxRetries)
                {
                    _channel.BasicAck(deliveryTag, false);

                }
                else
                {
                    _channel.BasicNack(deliveryTag, false, false);

                }

                if (noOfRetries == MaxRetries)
                {
                    //move data to some new exchange and queue or some databases for later processing if required.
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                _channel.BasicNack(deliveryTag, false, false);
            }
        }
    }
}   