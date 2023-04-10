using AutoMapper;
using Azure.Messaging.ServiceBus;
using Mango.MessageBus;
using Mango.Services.OrderAPI.Messages;
using Mango.Services.OrderAPI.Models;
using Mango.Services.OrderAPI.Repository;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Text;

namespace Mango.Services.OrderAPI.Messaging
{
    public class AzureServiceBusConsumer: IAzureServiceBusConsumer
    {
        private readonly string serviceBusConnectionString;
        private readonly string subscriptionNameCheckout;
        private readonly string checkoutMessageTopic;

        private readonly string subscriptionNameOrder;
        private readonly string orderPaymentProcessTopic;

        private readonly OrderRepository _orderRepository;
        private IMapper _mapper;
        private readonly IConfiguration _configuration;

        private ServiceBusProcessor checkoutProcessor;
        private readonly IMessageBus _messageBus;
        public AzureServiceBusConsumer(OrderRepository orderRepository, IMapper mapper, IConfiguration configuration, IMessageBus messageBus)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
            _configuration = configuration;

            serviceBusConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString");
            subscriptionNameCheckout = _configuration.GetValue<string>("subscriptionNameCheckout");
            checkoutMessageTopic = _configuration.GetValue<string>("CheckoutMessageTopic");

            subscriptionNameOrder = _configuration.GetValue<string>("OrderPaymentProcessSubscription");
            orderPaymentProcessTopic = _configuration.GetValue<string>("OrderPaymentProcessTopic");
             

            var client = new ServiceBusClient(serviceBusConnectionString);
            checkoutProcessor = client.CreateProcessor(checkoutMessageTopic, subscriptionNameCheckout);

            _messageBus = messageBus;
        }
        public async Task Start()
        {
            checkoutProcessor.ProcessMessageAsync += OnCheckOutMessageReceived;
            checkoutProcessor.ProcessErrorAsync+= ErrorHandler;
            await checkoutProcessor.StartProcessingAsync();
        }
        public async Task Stop()
        { 
            await checkoutProcessor.StopProcessingAsync();
            await checkoutProcessor.DisposeAsync();
        }
        public  Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }
        private  async Task OnCheckOutMessageReceived(ProcessMessageEventArgs args)
        {
            var message = args.Message;
            var body = Encoding.UTF8.GetString(message.Body);
            CheckoutHeaderDto checkoutHeaderDto = JsonConvert.DeserializeObject<CheckoutHeaderDto>(body);

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
            };
            try
            {
                await _messageBus.PublishMessage(paymentRequest, orderPaymentProcessTopic);
                await args.CompleteMessageAsync(args.Message);
            }
            catch(Exception ex)
            {
                throw;
            }
        }
    }
}
