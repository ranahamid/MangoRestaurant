using Mango.MessageBus;

namespace Mango.Services.ShoppingCartAPI.RabbitMqSender
{
    public interface IRabbitMqCartMessageSender
    {
        void SendMessage(BaseMessage message, string queueName);
    }
}
