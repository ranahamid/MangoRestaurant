using AutoMapper;
using Mango.MessageBus;
using Mango.Services.PaymentAPI.Messages;
using Mango.Services.PaymentAPI.RabbitMqSender;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PaymentProcessor;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading.Channels;

namespace Mango.Services.PaymentAPI.Messaging
{
    public class RabbitMqPaymentConsumer : BackgroundService
    {

        private IConnection _connection;
        private IModel _channel; 
        private readonly IConfiguration _configuration; 
        private IMapper _mapper;
        private readonly string orderPaymentProcessTopic;
        private readonly string orderUpdatePaymentProcessTopic;


        private readonly IProcessPayment _processPayment;
        private readonly IRabbitMqPaymentMessageSender _rabbitMqPaymentMessageSender;
        public RabbitMqPaymentConsumer(IProcessPayment processPayment, IConfiguration configuration ,IMapper mapper,
            IRabbitMqPaymentMessageSender rabbitMqPaymentMessageSender)
        {
            _processPayment = processPayment;
            _configuration = configuration; 
            orderPaymentProcessTopic = _configuration.GetValue<string>("OrderPaymentProcessTopic");
            orderUpdatePaymentProcessTopic = _configuration.GetValue<string>("OrderUpdatePaymentProcessTopic");

            _mapper = mapper;
            _rabbitMqPaymentMessageSender = rabbitMqPaymentMessageSender;


            var factory = new ConnectionFactory { HostName = "localhost" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();


            _channel.QueueDeclare(queue: orderPaymentProcessTopic,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);


        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (ch, ea) =>
            {
                var content = Encoding.UTF8.GetString(ea.Body.ToArray());
                PaymentRequestMessage paymentRequest = JsonConvert.DeserializeObject<PaymentRequestMessage>(content);
               


                HandleMessage(paymentRequest).GetAwaiter().GetResult();
                _channel.BasicAck(ea.DeliveryTag, false);
            };
            _channel.BasicConsume(queue: orderPaymentProcessTopic, autoAck:false,consumer: consumer);
            return Task.CompletedTask;
        }

        private async Task HandleMessage(PaymentRequestMessage paymentRequest)
        {
            var result = _processPayment.PaymentProcessor();
            UpdatePaymentResultMessage updatePaymentResultMessage = new UpdatePaymentResultMessage
            {
                Status = result,
                OrderId = paymentRequest.OrderId,
                Email = paymentRequest.Email,
            };
            try
            {
                //Azure
                //await _messageBus.PublishMessage(paymentRequest, orderPaymentProcessTopic);
                //await args.CompleteMessageAsync(args.Message);

                //RabbitMQ
                _rabbitMqPaymentMessageSender.SendMessage(updatePaymentResultMessage, orderUpdatePaymentProcessTopic);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
