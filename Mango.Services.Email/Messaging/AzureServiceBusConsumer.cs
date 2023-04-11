using AutoMapper;
using Azure.Messaging.ServiceBus; 
using Mango.Services.Email.Messages;
using Mango.Services.Email.Models;
using Mango.Services.Email.Repository;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Text;

namespace Mango.Services.Email.Messaging
{
    public class AzureServiceBusConsumer: IAzureServiceBusConsumer
    {
        private readonly string serviceBusConnectionString;

        private readonly string subscriptionNameEmail;
        private readonly string orderUpdatePaymentProcessTopic;
         

        private readonly EmailRepository _emailRepository;
        private IMapper _mapper;
        private readonly IConfiguration _configuration;
         
        private ServiceBusProcessor orderUpdatePaymentStatusProcessor; 
        public AzureServiceBusConsumer(EmailRepository emailRepository, IMapper mapper, IConfiguration configuration)
        {
            _emailRepository = emailRepository;
            _mapper = mapper;
            _configuration = configuration;

            serviceBusConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString");

            subscriptionNameEmail = _configuration.GetValue<string>("EmailSubscriptionName");
           
            orderUpdatePaymentProcessTopic = _configuration.GetValue<string>("OrderUpdatePaymentProcessTopic");

            var client = new ServiceBusClient(serviceBusConnectionString); 

            orderUpdatePaymentStatusProcessor = client.CreateProcessor(orderUpdatePaymentProcessTopic, subscriptionNameEmail);
             
        }
        public async Task Start()
        {
           

            orderUpdatePaymentStatusProcessor.ProcessMessageAsync += OnOrderPaymentUpdateReceived;
            orderUpdatePaymentStatusProcessor.ProcessErrorAsync += ErrorHandler;
            await orderUpdatePaymentStatusProcessor.StartProcessingAsync();
        }
        public async Task Stop()
        { 
            

            await orderUpdatePaymentStatusProcessor.StopProcessingAsync();
            await orderUpdatePaymentStatusProcessor.DisposeAsync();
        }
        public  Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }
        private async Task OnOrderPaymentUpdateReceived(ProcessMessageEventArgs args)
        {
            var message = args.Message;
            var body = Encoding.UTF8.GetString(message.Body);
            var  paymentResultMessage = JsonConvert.DeserializeObject<UpdatePaymentResultMessage>(body);

            await _emailRepository.SendAndLogEmail(paymentResultMessage); 
            try
            { 
                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
   
    }
}
