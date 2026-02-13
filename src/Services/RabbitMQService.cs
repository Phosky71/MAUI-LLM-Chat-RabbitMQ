using MAUILLMChatRabbitMQ.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace MAUILLMChatRabbitMQ.Services;

public class RabbitMQService : IRabbitMQService, IDisposable
{
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly RabbitMQConfig _config;
    private AsyncEventingBasicConsumer? _consumer;
    private bool _isDisposed;

    public event EventHandler<ChatMessage>? MessageReceived;

    public RabbitMQService(RabbitMQConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public async Task ConnectAsync()
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _config.HostName,
                Port = _config.Port,
                UserName = _config.UserName,
                Password = _config.Password
            };

            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            // 1. EXCHANGE DIRECTO
            await _channel.ExchangeDeclareAsync(
                exchange: _config.ExchangeName,
                type: "direct",
                durable: false,
                autoDelete: false,
                arguments: null
            );

            // 2. MI COLA (Donde ESCUCHO)
            await _channel.QueueDeclareAsync(
                queue: _config.SubscribeQueueName,
                durable: false,
                exclusive: false,
                autoDelete: true,
                arguments: null
            );

            //Purgar colaa
            //await _channel.QueuePurgeAsync(_config.SubscribeQueueName);
            //System.Diagnostics.Debug.WriteLine($"[RabbitMQ] Cola '{_config.SubscribeQueueName}' purgada.");

            // BINDING: Ligar MI cola al exchange con MI nombre de cola como routing key
            await _channel.QueueBindAsync(
                queue: _config.SubscribeQueueName,
                exchange: _config.ExchangeName,
                routingKey: _config.SubscribeQueueName,
                arguments: null
            );

            // 3. LA OTRA COLA (Donde PUBLICO)
            await _channel.QueueDeclareAsync(
                queue: _config.PublishQueueName,
                durable: false,
                exclusive: false,
                autoDelete: true,
                arguments: null
            );

            // BINDING: Ligar la OTRA cola al exchange con SU nombre de cola como routing key
            await _channel.QueueBindAsync(
                queue: _config.PublishQueueName,
                exchange: _config.ExchangeName,
                routingKey: _config.PublishQueueName,
                arguments: null
            );

            _consumer = new AsyncEventingBasicConsumer(_channel);
            _consumer.ReceivedAsync += OnMessageReceivedInternalAsync;

            await _channel.BasicConsumeAsync(
                queue: _config.SubscribeQueueName,
                autoAck: true,
                consumer: _consumer
            );

            System.Diagnostics.Debug.WriteLine($"[RabbitMQ] CONECTADO. Escuchando en '{_config.SubscribeQueueName}' | Publicando a '{_config.PublishQueueName}'");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error al conectar con RabbitMQ: {ex.Message}", ex);
        }
    }

    private async Task OnMessageReceivedInternalAsync(object sender, BasicDeliverEventArgs ea)
    {
        try
        {
            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);
            var message = JsonSerializer.Deserialize<ChatMessage>(json);

            if (message is not null)
            {
                // LOG CRÍTICO PARA DEBUG
                System.Diagnostics.Debug.WriteLine($"[RabbitMQ] Recibido mensaje de {message.SenderId} en cola {_config.SubscribeQueueName}");

                // Si el mensaje viene de mí mismo, lo ignoro.
                if (message.SenderId == _config.AppId)
                {
                    System.Diagnostics.Debug.WriteLine($"[RabbitMQ] IGNORADO: Mensaje propio (Loopback).");
                    return;
                }

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    MessageReceived?.Invoke(this, message);
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error procesando mensaje: {ex.Message}");
        }
    }

    public async Task PublishMessageAsync(ChatMessage message)
    {
        try
        {
            if (_channel is null || _channel.IsClosed) throw new InvalidOperationException("No conectado");

            // ASIGNAR ID: Crucial para evitar bucles
            message.SenderId = _config.AppId;

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = new BasicProperties
            {
                ContentType = "application/json",
                DeliveryMode = DeliveryModes.Transient
            };

            // PUBLICAR: Usar routingKey = PublishQueueName
            await _channel.BasicPublishAsync(
                exchange: _config.ExchangeName,
                routingKey: _config.PublishQueueName, // <--- ESTO ES LO IMPORTANTE
                mandatory: false,
                basicProperties: properties,
                body: body
            );

            System.Diagnostics.Debug.WriteLine($"[RabbitMQ] ENVIADO a routingKey '{_config.PublishQueueName}'");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error al publicar: {ex.Message}", ex);
        }
    }

    public async Task DisconnectAsync()
    {
        try
        {
            if (_channel is not null) await _channel.CloseAsync();
            if (_connection is not null) await _connection.CloseAsync();
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Error desconectando: {ex.Message}"); }
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        DisconnectAsync().Wait();
        _channel?.Dispose();
        _connection?.Dispose();
        _isDisposed = true;
        GC.SuppressFinalize(this);
    }
}
