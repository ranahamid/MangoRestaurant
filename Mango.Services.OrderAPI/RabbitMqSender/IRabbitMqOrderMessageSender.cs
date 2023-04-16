using Mango.MessageBus;

namespace Mango.Services.OrderAPI.RabbitMqSender
{
    public interface IRabbitMqOrderMessageSender
    {
        void SendMessage(BaseMessage message, string queueName);
    }
}
