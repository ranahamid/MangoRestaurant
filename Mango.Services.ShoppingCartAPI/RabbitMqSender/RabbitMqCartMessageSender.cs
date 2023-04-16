using Mango.MessageBus;
using Mango.Services.ShoppingCartAPI.Migrations;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

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
            var json = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(json);
            channel.BasicPublish(exchange: "", routingKey: queueName, mandatory: false, basicProperties: null, body: body);
        }
    }
}
