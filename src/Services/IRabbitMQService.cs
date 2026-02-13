using MAUILLMChatRabbitMQ.Models;

namespace MAUILLMChatRabbitMQ.Services;

public interface IRabbitMQService : IDisposable
{
    Task ConnectAsync();
    Task DisconnectAsync();
    Task PublishMessageAsync(ChatMessage message);
    event EventHandler<ChatMessage>? MessageReceived;
}
