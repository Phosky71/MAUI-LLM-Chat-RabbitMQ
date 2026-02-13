using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MAUILLMChatRabbitMQ.Models;
using MAUILLMChatRabbitMQ.Services;

namespace MAUILLMChatRabbitMQ.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly IRabbitMQService _rabbitMQService;
    private readonly LLMService _llmService;
    private readonly LLMConfig _llmConfig;
    private readonly RabbitMQConfig _rabbitConfig;

    private bool _isConnected;
    private string _statusMessage = "Desconectado";
    private bool _isProcessing;
    private int _messageCount;

    private int _isBusy = 0;

    public ObservableCollection<ChatMessage> Messages { get; } = new();

    public bool IsConnected
    {
        get => _isConnected;
        set
        {
            _isConnected = value;
            OnPropertyChanged();
            ((Command)StartConversationCommand).ChangeCanExecute();
            ((Command)DisconnectCommand).ChangeCanExecute();
            ((Command)SendManualMessageCommand).ChangeCanExecute();
            ((Command)ClearHistoryCommand).ChangeCanExecute();
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set { _statusMessage = value; OnPropertyChanged(); }
    }

    public bool IsProcessing
    {
        get => _isProcessing;
        set { _isProcessing = value; OnPropertyChanged(); }
    }

    public int MessageCount
    {
        get => _messageCount;
        set { _messageCount = value; OnPropertyChanged(); }
    }

    public string ManualMessageText { get; set; } = string.Empty;

    public ICommand StartConversationCommand { get; }
    public ICommand DisconnectCommand { get; }
    public ICommand SendManualMessageCommand { get; }
    public ICommand ClearMessagesCommand { get; }
    public ICommand ClearHistoryCommand { get; }

    public MainViewModel(IRabbitMQService rabbitMQService, LLMService llmService, LLMConfig llmConfig, RabbitMQConfig rabbitConfig)
    {
        _rabbitMQService = rabbitMQService;
        _llmService = llmService;
        _llmConfig = llmConfig;
        _rabbitConfig = rabbitConfig;

        _rabbitMQService.MessageReceived += OnMessageReceived;

        StartConversationCommand = new Command(async () => await StartConversationAsync(), () => !IsConnected);
        DisconnectCommand = new Command(async () => await DisconnectAsync(), () => IsConnected);
        SendManualMessageCommand = new Command(async () => await SendManualMessageAsync(), () => IsConnected && !string.IsNullOrWhiteSpace(ManualMessageText));
        ClearMessagesCommand = new Command(() => ClearMessages());
        ClearHistoryCommand = new Command(() => ClearConversationHistory(), () => IsConnected);
    }

    private async Task StartConversationAsync()
    {
        try
        {
            Interlocked.Exchange(ref _isBusy, 0);

            IsProcessing = true;
            StatusMessage = "Conectando...";
            await _rabbitMQService.ConnectAsync();
            IsConnected = true;

            AddSystemMessage($"✅ Conectado como: {_llmConfig.AppName}");
            AddSystemMessage($"📡 Exchange: {_rabbitConfig.ExchangeName}");
            AddSystemMessage($"📤 Publicando a: {_rabbitConfig.PublishQueueName}");
            AddSystemMessage($"📥 Escuchando en: {_rabbitConfig.SubscribeQueueName}");

            StatusMessage = "Escuchando actividad...";

            await Task.Delay(3000);

            var userMessagesCount = Messages.Count(m => m.MessageType == MessageType.Received);

            if (userMessagesCount > 0)
            {
                System.Diagnostics.Debug.WriteLine("⚠️ Detectada actividad previa. Omitiendo mensaje inicial.");
                StatusMessage = "Debate activo detectado. Esperando turno...";
                AddSystemMessage("ℹ️ Me he unido a un debate en curso.");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("✅ Silencio detectado. Iniciando debate.");
                StatusMessage = "Generando apertura...";

                var initialPrompt = "Inicia el debate con una afirmación contundente y polémica de 2 párrafos.";
                var response = await _llmService.GetResponseAsync(initialPrompt, useHistory: true);

                if (string.IsNullOrEmpty(response) || response.StartsWith("Error"))
                    response = "El debate está abierto. Mi postura es clara e inamovible.";

                var message = new ChatMessage
                {
                    Sender = _llmConfig.AppName,
                    Content = response,
                    MessageType = MessageType.Sent,
                    Timestamp = DateTime.Now
                };

                Messages.Add(message);
                await _rabbitMQService.PublishMessageAsync(message);

                StatusMessage = "Apertura enviada. Esperando contrincante...";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = "Error al conectar";
            AddErrorMessage(ex.Message);
            IsConnected = false;
        }
        finally { IsProcessing = false; }
    }


    private async Task DisconnectAsync()
    {
        await _rabbitMQService.DisconnectAsync();
        IsConnected = false;
        StatusMessage = "Desconectado";
        AddSystemMessage("🔌 Desconectado");
    }

    private void OnMessageReceived(object? sender, ChatMessage message)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            message.MessageType = MessageType.Received;
            Messages.Add(message);
            MessageCount = Messages.Count;
            StatusMessage = $"Recibido de {message.Sender}";
        });

        Task.Run(async () => await ProcessReceivedMessageAsync(message));
    }

    private async Task ProcessReceivedMessageAsync(ChatMessage receivedMessage)
    {
        // LOGICA DE BLOQUEO ATÓMICO
        if (Interlocked.CompareExchange(ref _isBusy, 1, 0) == 1)
        {
            System.Diagnostics.Debug.WriteLine($"⚠️ [MainViewModel] IGNORANDO mensaje de {receivedMessage.Sender} porque ya estoy ocupado.");
            return; 
        }

        try
        {
            IsProcessing = true;
            StatusMessage = "Pensando respuesta...";

            await Task.Delay(3000);

            var response = await _llmService.GetResponseAsync(receivedMessage.Content, useHistory: true);

            if (string.IsNullOrEmpty(response) || response.StartsWith("Error"))
            {
                response = "Tu argumento es falaz. Explícate mejor.";
            }

            var responseMessage = new ChatMessage
            {
                Sender = _llmConfig.AppName,
                Content = response,
                MessageType = MessageType.Sent,
                Timestamp = DateTime.Now
            };

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Messages.Add(responseMessage);
                MessageCount = Messages.Count;
            });

            await _rabbitMQService.PublishMessageAsync(responseMessage);

            StatusMessage = "Respuesta enviada. Esperando turno...";
        }
        catch (Exception ex)
        {
            await MainThread.InvokeOnMainThreadAsync(() => AddErrorMessage($"Error en bucle: {ex.Message}"));
        }
        finally
        {
            IsProcessing = false;
            Interlocked.Exchange(ref _isBusy, 0);
        }
    }

    private async Task SendManualMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(ManualMessageText)) return;

        var message = new ChatMessage
        {
            Sender = _llmConfig.AppName,
            Content = ManualMessageText,
            MessageType = MessageType.Sent,
            Timestamp = DateTime.Now
        };

        Messages.Add(message);
        await _rabbitMQService.PublishMessageAsync(message);
        ManualMessageText = "";
        OnPropertyChanged(nameof(ManualMessageText));
    }

    private void ClearMessages() { Messages.Clear(); MessageCount = 0; }
    private void ClearConversationHistory() { _llmService.ClearHistory(); AddSystemMessage("Memoria borrada"); }

    private void AddSystemMessage(string content)
    {
        Messages.Add(new ChatMessage { Sender = "Sistema", Content = content, MessageType = MessageType.System });
    }

    private void AddErrorMessage(string content)
    {
        Messages.Add(new ChatMessage { Sender = "Error", Content = content, MessageType = MessageType.Error });
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public async Task CleanupAsync() { if (IsConnected) await DisconnectAsync(); }
}
