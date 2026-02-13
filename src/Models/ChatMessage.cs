namespace MAUILLMChatRabbitMQ.Models;

public class ChatMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string SenderId { get; set; } = string.Empty;
    public string Sender { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public MessageType MessageType { get; set; } = MessageType.System;
}

public enum MessageType
{
    Sent,
    Received,
    System,
    Error
}
