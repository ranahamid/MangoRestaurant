using AutoMapper;
using Mango.MessageBus;
using Mango.Services.OrderAPI.Messages;
using Mango.Services.OrderAPI.Models;
using Mango.Services.OrderAPI.RabbitMqSender;
using Mango.Services.OrderAPI.Repository;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading.Channels;

namespace Mango.Services.OrderAPI.Messaging
{
    public class RabbitMqCheckoutConsumer : BackgroundService
    {
        private readonly OrderRepository _orderRepository;
        private IConnection _connection;
        private IModel _channel;
        private readonly string queueName;
        private readonly IConfiguration _configuration; 
        private IMapper _mapper;
        private readonly string orderPaymentProcessTopic;

        private readonly IRabbitMqOrderMessageSender _rabbitMqOrderMessageSender;
        public RabbitMqCheckoutConsumer(OrderRepository orderRepository, IConfiguration configuration ,IMapper mapper,
            IRabbitMqOrderMessageSender rabbitMqOrderMessageSender)
        {
            _orderRepository = orderRepository;
            _configuration = configuration;
            queueName = _configuration.GetValue<string>("checkoutQueue");
            orderPaymentProcessTopic = _configuration.GetValue<string>("OrderPaymentProcessTopic");


            _mapper = mapper;
            _rabbitMqOrderMessageSender = rabbitMqOrderMessageSender;


            var factory = new ConnectionFactory { HostName = "localhost" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();


            _channel.QueueDeclare(queue: queueName,
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
                CheckoutHeaderDto checkoutHeaderDto = JsonConvert.DeserializeObject<CheckoutHeaderDto>(content);
                HandleMessage(checkoutHeaderDto).GetAwaiter().GetResult();
                _channel.BasicAck(ea.DeliveryTag, false);
            };
            _channel.BasicConsume(queue: queueName,autoAck:false,consumer: consumer);
            return Task.CompletedTask;
        }

        private async Task HandleMessage(CheckoutHeaderDto checkoutHeaderDto)
        {
            OrderHeader orderHeader = _mapper.Map<CheckoutHeaderDto, OrderHeader>(checkoutHeaderDto);
            orderHeader.OrderDetails = new List<OrderDetails>();
            orderHeader.OrderTime = DateTime.UtcNow;
            orderHeader.PaymentStatus = false;

            foreach (var detailList in checkoutHeaderDto.CartDetails)
            {
                OrderDetails orderDetails = _mapper.Map<CartDetailsDto, OrderDetails>(detailList);
                orderHeader.CartTotalItems += detailList.Count;

                orderHeader.OrderDetails.Add(orderDetails);
            }
            await _orderRepository.AddOrder(orderHeader);

            PaymentRequestMessage paymentRequest = new PaymentRequestMessage
            {
                Name = orderHeader.FirstName + " " + orderHeader.LastName,
                CardNumber = orderHeader.CardNumber,
                CVV = orderHeader.CVV,
                ExpiryMonthYear = orderHeader.ExpiryMonthYear,
                MessageCreated = DateTime.UtcNow,
                OrderId = orderHeader.OrderHeaderId,
                OrderTotal = orderHeader.OrderTotal,
                Email = orderHeader.Email,
            };
            try
            {
                //Azure
                //await _messageBus.PublishMessage(paymentRequest, orderPaymentProcessTopic);
                //await args.CompleteMessageAsync(args.Message);

                //RabbitMQ
                _rabbitMqOrderMessageSender.SendMessage(paymentRequest, orderPaymentProcessTopic);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
