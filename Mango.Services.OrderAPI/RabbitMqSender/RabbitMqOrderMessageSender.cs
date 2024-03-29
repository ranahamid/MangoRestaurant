﻿using Mango.MessageBus;
 
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace Mango.Services.OrderAPI.RabbitMqSender
{
    public class RabbitMqOrderMessageSender : IRabbitMqOrderMessageSender
    {
        private readonly string _hostName;
        private readonly string _password;
        private readonly string _userName;

        private IConnection _connection;
        public RabbitMqOrderMessageSender()
        {
            _hostName = "localhost";
            _password = "guest";
            _userName = "guest";
        }

        public void SendMessage(BaseMessage message, string queueName)
        {
            try
            {
                if (ConnectionExists())
                {
                    using var channel = _connection.CreateModel();

                    channel.QueueDeclare(queue: queueName,
                                         durable: false,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);
                    var json = JsonConvert.SerializeObject(message);
                    var body = Encoding.UTF8.GetBytes(json);

                    channel.BasicPublish(exchange: string.Empty,
                                         routingKey: queueName,
                                         basicProperties: null,
                                         body: body);


                }

            }
            catch (Exception ex)
            {

            }
        }
        private void CreateConnection()
        {
            try
            {
                var factory = new ConnectionFactory { HostName = "localhost" };
                _connection = factory.CreateConnection();
            }
            catch (Exception ex)
            {

            }
        }
        private bool ConnectionExists()
        {
            if (_connection != null)
            {
                return true;
            }
            CreateConnection();
            return _connection != null;
        }
    }
}
