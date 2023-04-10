using AutoMapper;
using Azure.Messaging.ServiceBus;
using Mango.MessageBus;
using Mango.Services.OrderAPI.Messages;
using Mango.Services.OrderAPI.Models;
using Mango.Services.OrderAPI.Repository;
using Mango.Services.PaymentAPI.Messages;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PaymentProcessor;
using System.Text;

namespace Mango.Services.PaymentAPI.Messaging
{
    public class AzureServiceBusConsumer: IAzureServiceBusConsumer
    {
        private readonly string serviceBusConnectionString;
        //private readonly string subscriptionNameCheckout;
        //private readonly string checkoutMessageTopic;

        private readonly string subscriptionNameOrder;
        private readonly string orderPaymentProcessTopic;
         
        private IMapper _mapper;
        private readonly IConfiguration _configuration;

        private ServiceBusProcessor orderPaymentProcessor;
        private readonly IMessageBus _messageBus;

        private readonly IProcessPayment _processPayment;

        public AzureServiceBusConsumer(IProcessPayment processPayment, IMapper mapper, IConfiguration configuration, IMessageBus messageBus)
        {
            _processPayment = processPayment;
            _mapper = mapper;
            _configuration = configuration;

            _messageBus = messageBus;

            serviceBusConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString");
            //subscriptionNameCheckout = _configuration.GetValue<string>("subscriptionNameCheckout");
            //checkoutMessageTopic = _configuration.GetValue<string>("CheckoutMessageTopic");

            subscriptionNameOrder = _configuration.GetValue<string>("OrderPaymentProcessSubscription");
            orderPaymentProcessTopic = _configuration.GetValue<string>("OrderPaymentProcessTopic");
             

            var client = new ServiceBusClient(serviceBusConnectionString);
            orderPaymentProcessor = client.CreateProcessor(orderPaymentProcessTopic, subscriptionNameOrder);

        }
        public async Task Start()
        {
            orderPaymentProcessor.ProcessMessageAsync += ProcessPayments;
            orderPaymentProcessor.ProcessErrorAsync+= ErrorHandler;
            await orderPaymentProcessor.StartProcessingAsync();
        }
        public async Task Stop()
        { 
            await orderPaymentProcessor.StopProcessingAsync();
            await orderPaymentProcessor.DisposeAsync();
        }
        public  Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }
        private  async Task ProcessPayments(ProcessMessageEventArgs args)
        {
            var message = args.Message;
            var body = Encoding.UTF8.GetString(message.Body);
            PaymentRequestMessage paymentRequest = JsonConvert.DeserializeObject<PaymentRequestMessage>(body);
            var result = _processPayment.PaymentProcessor();

            UpdatePaymentResultMessage updateResult = new UpdatePaymentResultMessage
            {
                Status = result,
                OrderId = paymentRequest.OrderId,
            };
            //new topic
            

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
