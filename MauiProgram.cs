using MAUILLMChatRabbitMQ.Models;
using MAUILLMChatRabbitMQ.Services;
using MAUILLMChatRabbitMQ.ViewModels;
using MAUILLMChatRabbitMQ.Views;
using Microsoft.Extensions.Logging;

namespace MAUILLMChatRabbitMQ;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // ===== REGISTRO DE CONFIGURACIONES (Singleton - compartidas) =====

        builder.Services.AddSingleton<LLMConfig>(sp => new LLMConfig
        {
            AppName = "Defensor de Gatos 🐱",
            ModelName = "llama-3.2-3b-instruct",
            BaseURL = "http://localhost:1234/v1",
            MaxTokens = 500,
            Temperature = 0.7,
            SystemPrompt = "Eres un defensor apasionado de los GATOS. " +
                          "Argumenta de forma convincente por qué los gatos son mejores mascotas que los perros. " +
                          "Usa lógica, datos y experiencias personales inventadas. " +
                          "Sé educado pero persuasivo. Responde de forma concisa (máximo 3 párrafos).",
            StreamResponse = false
        });

        builder.Services.AddSingleton<RabbitMQConfig>(sp => new RabbitMQConfig
        {
            HostName = "localhost",
            Port = 5672,
            UserName = "guest",
            Password = "guest",
            ExchangeName = "llm_chat_exchange",
            PublishQueueName = "app1_out",
            SubscribeQueueName = "app2_out",
            AppId = Guid.NewGuid().ToString()
        });

        // ===== REGISTRO DE SERVICIOS (Singleton) =====

        builder.Services.AddSingleton<IRabbitMQService, RabbitMQService>();
        builder.Services.AddSingleton<LLMService>();

        // ===== REGISTRO DE VIEWMODELS (Transient - nueva instancia cada vez) =====

        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<ConfigViewModel>();

        // ===== REGISTRO DE VISTAS (Transient) =====

        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<ConfigPage>();

        return builder.Build();
    }
}

