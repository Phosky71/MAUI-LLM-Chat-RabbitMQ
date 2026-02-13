namespace MAUILLMChatRabbitMQ.Models;

public class RabbitMQConfig
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string ExchangeName { get; set; } = "llm_chat_exchange";
    public string PublishQueueName { get; set; } = "app1_out";
    public string SubscribeQueueName { get; set; } = "app2_out";
    public string AppId { get; set; } = Guid.NewGuid().ToString();
}
