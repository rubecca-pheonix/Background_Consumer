using System;
using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace RabbitMQConsumer
{
    public class RabbitMqManager
    {
        private readonly ApplicationConfigurations _configurations;
        public IConnection? Connection { get; private set; }
        private IModel? Channel { get; set; }
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RabbitMqManager> _logger;

        public RabbitMqManager(IOptions<ApplicationConfigurations> config,
            IServiceProvider serviceProvider, ILogger<RabbitMqManager> logger)
        {
            _configurations = config.Value;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public void RegisterMq()
        {
            CreateConnection();
            RegisterExchanges();
            ReqisterQueues();
            RegisterConsumer();
        }

        private void CreateConnection()
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _configurations.RabbitMqProperties.Host,
                    Port = _configurations.RabbitMqProperties.Port,
                    Ssl = new SslOption
                    {
                        Enabled = true,
                        ServerName = _configurations.RabbitMqProperties.Host
                    }
                };
                factory.UserName = _configurations.RabbitMqProperties.Username;
                factory.Password = _configurations.RabbitMqProperties.Password;
                Connection = factory.CreateConnection();
                Channel = Connection.CreateModel();
                Channel.BasicQos(0, 1, false);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }

        }

        private void ReqisterQueues()
        {

            try
            {
                if (Channel != null)
                {
                    Dictionary<String, Object> argumentsForOtpQ = new Dictionary<string, object>();
                    argumentsForOtpQ.Add("x-dead-letter-exchange", _configurations.Consumer.DeadLetterExchange);
                    argumentsForOtpQ.Add("x-dead-letter-routing-key", _configurations.Consumer.RoutingKey);

                    Channel.QueueDeclare(_configurations.Consumer.ListnerQueue, true, false, true, argumentsForOtpQ);
                    Channel.QueueBind(_configurations.Consumer.ListnerQueue, _configurations.Consumer.Exchange, _configurations.Consumer.RoutingKey);

                    Dictionary<String, Object> argumentsForRetryQ = new Dictionary<string, object>();
                    argumentsForRetryQ.Add("x-dead-letter-exchange", _configurations.Consumer.Exchange);
                    argumentsForRetryQ.Add("x-dead-letter-routing-key", _configurations.Consumer.RoutingKey);
                    argumentsForRetryQ.Add("x-message-ttl", 5000);

                    Channel.QueueDeclare(_configurations.Consumer.RetryQueue, true, false, true, argumentsForRetryQ);
                    Channel.QueueBind(_configurations.Consumer.RetryQueue, _configurations.Consumer.DeadLetterExchange, _configurations.Consumer.RoutingKey);
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }


            
        }

        private void RegisterExchanges()
        {
            try
            {
                Channel.ExchangeDeclare(_configurations.Consumer.DeadLetterExchange, ExchangeType.Topic);

            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        private void RegisterConsumer()
        {
            try
            {
                if (Channel != null)
                {
                    var logger = _serviceProvider.GetRequiredService<ILogger<MessageReceiver>>();
                    MessageReceiver messageReceiver = new MessageReceiver(Channel, _serviceProvider, logger);
                    Channel.BasicConsume(_configurations.Consumer?.ListnerQueue, false, messageReceiver);
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }
    }
}

