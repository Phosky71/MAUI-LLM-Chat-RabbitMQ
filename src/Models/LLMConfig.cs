namespace MAUILLMChatRabbitMQ.Models;

public class LLMConfig
{
    public string ModelName { get; set; } = "llama-3.2-3b-instruct";
    public string BaseURL { get; set; } = "http://localhost:1234/v1";
    public int MaxTokens { get; set; } = 500;
    public double Temperature { get; set; } = 0.7;
    public string SystemPrompt { get; set; } =
        "Eres un defensor apasionado de los GATOS. " +
        "Debes argumentar de forma convincente por qué los gatos son mejores mascotas que los perros. " +
        "Usa lógica, datos y experiencias personales inventadas. " +
        "Sé educado pero firme en tus argumentos. " +
        "Responde de forma concisa (máximo 3 párrafos).";

    public bool StreamResponse { get; set; } = false;

    public string AppName { get; set; } = "Defensor de Gatos";
}
