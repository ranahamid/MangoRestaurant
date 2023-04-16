using Mango.MessageBus;
using RabbitMQ.Client;

namespace Mango.Services.ShoppingCartAPI.RabbitMqSender
{
    public class RabbitMqCartMessageSender : IRabbitMqCartMessageSender
    {
        private readonly string _hostName;
        private readonly string _password;
        private readonly string _userName;
        public RabbitMqCartMessageSender()
        {
            _hostName = "localhost";
            _password = "guest";
            _userName = "guest";
        }

        public void SendMessage(BaseMessage message, string queueName)
        {
            var factory = new ConnectionFactory
            {
                HostName = _hostName,
                Password = _password,
                UserName = _userName,
            };
            var _connection = factory.CreateConnection();
            using var channel = _connection.CreateModel();
            channel.QueueDeclare(queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
        }
    }
}
