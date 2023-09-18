using System;
namespace RabbitMQConsumer
{
    public class ApplicationConfigurations
    {
        public RabbitMqProperties? RabbitMqProperties { get; set; }
        public Consumer? Consumer { get; set; }
    }

    public class RabbitMqProperties
    {
        public required string Host { get; set; }
        public int Port { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
    }

    public class Consumer
    {
        public required string Exchange { get; set; }
        public required string Type { get; set; }
        public required string RoutingKey { get; set; }
        public required string ListnerQueue { get; set; }
        public required string RetryQueue { get; set; }
        public required string DeadLetterExchange { get; set; }
        public bool Persistent { get; set; }
        public bool Durable { get; set; }
        public bool AutoDelete { get; set; }

    }
}

