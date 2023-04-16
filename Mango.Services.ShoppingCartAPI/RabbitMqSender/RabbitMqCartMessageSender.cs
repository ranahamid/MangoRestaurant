using Mango.MessageBus;

namespace Mango.Services.ShoppingCartAPI.RabbitMqSender
{
    public class RabbitMqCartMessageSender : IRabbitMqCartMessageSender
    {
        private readonly string _hostName;
        private readonly string _password;
        private readonly string _userName;
        public void SendMessage(BaseMessage message, string queueName)
        {
            throw new NotImplementedException();
        }
    }
}
