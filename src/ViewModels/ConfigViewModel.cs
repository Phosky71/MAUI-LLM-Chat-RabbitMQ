using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MAUILLMChatRabbitMQ.Models;
using MAUILLMChatRabbitMQ.Services;

namespace MAUILLMChatRabbitMQ.ViewModels;

public class ConfigViewModel : INotifyPropertyChanged
{
    private readonly LLMService _llmService;
    private readonly IRabbitMQService _rabbitMQService;

    private readonly LLMConfig _llmConfig;
    private readonly RabbitMQConfig _rabbitConfig;

    // ===== PROPIEDADES LLM =====
    public string AppName
    {
        get => _llmConfig.AppName;
        set { _llmConfig.AppName = value; OnPropertyChanged(); }
    }

    public string ModelName
    {
        get => _llmConfig.ModelName;
        set { _llmConfig.ModelName = value; OnPropertyChanged(); }
    }

    public string BaseURL
    {
        get => _llmConfig.BaseURL;
        set { _llmConfig.BaseURL = value; OnPropertyChanged(); }
    }

    public double Temperature
    {
        get => _llmConfig.Temperature;
        set { _llmConfig.Temperature = value; OnPropertyChanged(); }
    }

    public int MaxTokens
    {
        get => _llmConfig.MaxTokens;
        set { _llmConfig.MaxTokens = value; OnPropertyChanged(); }
    }

    public string SystemPrompt
    {
        get => _llmConfig.SystemPrompt;
        set { _llmConfig.SystemPrompt = value; OnPropertyChanged(); }
    }

    // ===== PROPIEDADES RABBITMQ =====
    public string RabbitHostName
    {
        get => _rabbitConfig.HostName;
        set { _rabbitConfig.HostName = value; OnPropertyChanged(); }
    }

    public int RabbitPort
    {
        get => _rabbitConfig.Port;
        set { _rabbitConfig.Port = value; OnPropertyChanged(); }
    }

    public string RabbitUserName
    {
        get => _rabbitConfig.UserName;
        set { _rabbitConfig.UserName = value; OnPropertyChanged(); }
    }

    public string RabbitPassword
    {
        get => _rabbitConfig.Password;
        set { _rabbitConfig.Password = value; OnPropertyChanged(); }
    }

    public string ExchangeName
    {
        get => _rabbitConfig.ExchangeName;
        set { _rabbitConfig.ExchangeName = value; OnPropertyChanged(); }
    }

    public string PublishQueueName
    {
        get => _rabbitConfig.PublishQueueName;
        set { _rabbitConfig.PublishQueueName = value; OnPropertyChanged(); }
    }

    public string SubscribeQueueName
    {
        get => _rabbitConfig.SubscribeQueueName;
        set { _rabbitConfig.SubscribeQueueName = value; OnPropertyChanged(); }
    }

    public string AppId
    {
        get => _rabbitConfig.AppId;
        set { _rabbitConfig.AppId = value; OnPropertyChanged(); }
    }

    public ICommand SaveConfigCommand { get; }

    // ===== CONSTRUCTOR =====
    public ConfigViewModel(
        LLMService llmService,
        IRabbitMQService rabbitMQService,
        LLMConfig llmConfig,
        RabbitMQConfig rabbitConfig)
    {
        _llmService = llmService;
        _rabbitMQService = rabbitMQService;
        _llmConfig = llmConfig;
        _rabbitConfig = rabbitConfig;
        _rabbitConfig.AppId = Guid.NewGuid().ToString();

        LoadSavedConfig();

        SaveConfigCommand = new Command(async () => await SaveConfigAsync());
    }

    // ===== MÉTODOS =====

    private void LoadSavedConfig()
    {
        _rabbitConfig.HostName = Preferences.Get(nameof(RabbitHostName), "localhost");
        _rabbitConfig.Port = Preferences.Get(nameof(RabbitPort), 5672);
        _rabbitConfig.UserName = Preferences.Get(nameof(RabbitUserName), "guest");
        _rabbitConfig.Password = Preferences.Get(nameof(RabbitPassword), "guest");
        _rabbitConfig.ExchangeName = Preferences.Get(nameof(ExchangeName), "llm_chat_exchange");
        _rabbitConfig.PublishQueueName = Preferences.Get(nameof(PublishQueueName), "app1_out");
        _rabbitConfig.SubscribeQueueName = Preferences.Get(nameof(SubscribeQueueName), "app2_out");



        _llmConfig.AppName = Preferences.Get(nameof(AppName), "Defensor de Gatos");
        _llmConfig.ModelName = Preferences.Get(nameof(ModelName), "llama-3.2-3b-instruct");
        _llmConfig.BaseURL = Preferences.Get(nameof(BaseURL), "http://localhost:1234/v1");
        _llmConfig.SystemPrompt = Preferences.Get(nameof(SystemPrompt), "Eres un asistente útil.");
        _llmConfig.Temperature = Preferences.Get(nameof(Temperature), 0.7);
        _llmConfig.MaxTokens = Preferences.Get(nameof(MaxTokens), 500);

        RefreshAllProperties();
    }

    private async Task SaveConfigAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(BaseURL)) { await ShowAlertAsync("Error", "URL vacía"); return; }
            if (string.IsNullOrWhiteSpace(RabbitHostName)) { await ShowAlertAsync("Error", "Host RabbitMQ vacío"); return; }
            if (PublishQueueName == SubscribeQueueName) { await ShowAlertAsync("Error", "Las colas deben ser diferentes"); return; }

            Preferences.Set(nameof(RabbitHostName), RabbitHostName);
            Preferences.Set(nameof(RabbitPort), RabbitPort);
            Preferences.Set(nameof(RabbitUserName), RabbitUserName);
            Preferences.Set(nameof(RabbitPassword), RabbitPassword);
            Preferences.Set(nameof(ExchangeName), ExchangeName);
            Preferences.Set(nameof(PublishQueueName), PublishQueueName);
            Preferences.Set(nameof(SubscribeQueueName), SubscribeQueueName);



            Preferences.Set(nameof(AppName), AppName);
            Preferences.Set(nameof(ModelName), ModelName);
            Preferences.Set(nameof(BaseURL), BaseURL);
            Preferences.Set(nameof(SystemPrompt), SystemPrompt);
            Preferences.Set(nameof(Temperature), Temperature);
            Preferences.Set(nameof(MaxTokens), MaxTokens);

            _llmService.UpdateConfig(_llmConfig);

            await ShowAlertAsync(
                "Configuración Guardada",
                "Datos guardados correctamente.\n\n⚠️ Si cambiaste la configuración de RabbitMQ, desconecta y vuelve a conectar para aplicar cambios.",
                "OK"
            );
        }
        catch (Exception ex)
        {
            await ShowAlertAsync("Error", $"Error al guardar: {ex.Message}");
        }
    }

    private void RefreshAllProperties()
    {
        OnPropertyChanged(string.Empty);
    }

    private async Task ShowAlertAsync(string title, string message, string button = "OK")
    {
        if (Application.Current?.MainPage != null)
            await Application.Current.MainPage.DisplayAlert(title, message, button);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
