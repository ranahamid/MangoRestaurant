using AutoMapper; 
using Mango.Services.Email.Messages;
using Mango.Services.Email.Repository; 
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
//using PaymentProcessor;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading.Channels;

namespace Mango.Services.Email.Messaging
{
    public class RabbitMqPaymentConsumer : BackgroundService
    {

        private readonly EmailRepository _emailRepository;
        private IConnection _connection;
        private IModel _channel;
        private readonly IConfiguration _configuration;
        private IMapper _mapper;
        private readonly string orderPaymentProcessTopic;
        private readonly string orderUpdatePaymentProcessTopic;
        private const string ExchangeName = "DirectPaymentUpdate_Exchange";

        private const string PaymentOrderUpdateQueueName = "PaymentOrderUpdateQueueName";
        private const string PaymentEmailUpdateQueueName = "PaymentEmailUpdateQueueName";

        //private readonly IProcessPayment _processPayment;
        //private readonly IRabbitMqPaymentMessageSender _rabbitMqPaymentMessageSender;
        string queueName = string.Empty;
        public RabbitMqPaymentConsumer(EmailRepository emailRepository,IConfiguration configuration, IMapper mapper
        //  , IProcessPayment processPayment, IRabbitMqPaymentMessageSender rabbitMqPaymentMessageSender
            )
        {
            _emailRepository = emailRepository;
            //_processPayment = processPayment;
            //_rabbitMqPaymentMessageSender = rabbitMqPaymentMessageSender;

            _configuration = configuration;
            orderPaymentProcessTopic = _configuration.GetValue<string>("OrderPaymentProcessTopic");
            orderUpdatePaymentProcessTopic = _configuration.GetValue<string>("OrderUpdatePaymentProcessTopic");

            _mapper = mapper;
            


            var factory = new ConnectionFactory { HostName = "localhost" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(exchange: ExchangeName, type: ExchangeType.Direct, durable: false);

            //_channel.QueueDeclare(queue: orderPaymentProcessTopic,
            //                     durable: false,
            //                     exclusive: false,
            //                     autoDelete: false,
            //                     arguments: null);

            _channel.QueueDeclare(PaymentEmailUpdateQueueName, false, false, false, null);
            _channel.QueueBind(PaymentEmailUpdateQueueName, exchange: ExchangeName, routingKey: "PaymentEmail");
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (ch, ea) =>
            {
                var content = Encoding.UTF8.GetString(ea.Body.ToArray());
                var paymentResultMessage = JsonConvert.DeserializeObject<UpdatePaymentResultMessage>(content);



                HandleMessage(paymentResultMessage).GetAwaiter().GetResult();
                _channel.BasicAck(ea.DeliveryTag, false);
            };
            _channel.BasicConsume(queue: PaymentEmailUpdateQueueName, autoAck: false, consumer: consumer);
            return Task.CompletedTask;
        }

        private async Task HandleMessage(UpdatePaymentResultMessage paymentResultMessage)
        {

            try
            {
                await _emailRepository.SendAndLogEmail(paymentResultMessage);

                //RabbitMQ
                //_rabbitMqPaymentMessageSender.SendMessage(paymentResultMessage);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
