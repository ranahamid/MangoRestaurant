using Mango.MessageBus;

using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace Mango.Services.PaymentAPI.RabbitMqSender
{
    public class RabbitMqPaymentMessageSender : IRabbitMqPaymentMessageSender
    {
        private readonly string _hostName;
        private readonly string _password;
        private readonly string _userName;


        private IConnection _connection;

        private const string ExchangeName = "DirectPaymentUpdate_Exchange";

        private const string PaymentOrderUpdateQueueName = "PaymentOrderUpdateQueueName";
        private const string PaymentEmailUpdateQueueName = "PaymentEmailUpdateQueueName";

        public RabbitMqPaymentMessageSender()
        {
            _hostName = "localhost";
            _password = "guest";
            _userName = "guest";
        }

        public void SendMessage(BaseMessage message)
        {
            try
            {
                if (ConnectionExists())
                {
                    using var channel = _connection.CreateModel();

                    //channel.QueueDeclare(queue: queueName,
                    //                     durable: false,
                    //                     exclusive: false,
                    //                     autoDelete: false,
                    //                     arguments: null);

                    channel.ExchangeDeclare(exchange: ExchangeName, type: ExchangeType.Direct, durable: false);

                    channel.QueueDeclare(PaymentOrderUpdateQueueName, false, false, false, null);
                    channel.QueueDeclare(PaymentEmailUpdateQueueName, false, false, false, null);

                    channel.QueueBind(PaymentOrderUpdateQueueName, ExchangeName, "PaymentOrder");
                    channel.QueueBind(PaymentEmailUpdateQueueName, ExchangeName,"PaymentEmail");

                    var json = JsonConvert.SerializeObject(message);
                    var body = Encoding.UTF8.GetBytes(json);

                    channel.BasicPublish(exchange: ExchangeName,
                                         routingKey: "PaymentOrder",
                                         basicProperties: null,
                                         body: body);

                    channel.BasicPublish(exchange: ExchangeName,
                                        routingKey: "PaymentEmail",
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
