using MicroRabbit.Domain.Core.Bus;
using MicroRabbit.Transfer.Domain.Events;
using MicroRabbit.Transfer.RabbitMQBus.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MicroRabbit.Transfer.RabbitMQBus.Receiver
{
    //CustomerFullNameUpdateReceiver
    public class TransferMessageReceiver : BackgroundService
    {
        private IChannel _channel;
        private IConnection _connection;
        //private readonly IEventHandler<TransferCreatedEvent> _transferCreatedEventHandler;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<TransferMessageReceiver> _logger;

        private readonly string _hostname;
        private readonly string _queueName;
        private readonly string _username;
        private readonly string _password;

        public TransferMessageReceiver(IOptions<RabbitMqConfiguration> rabbitMqOptions, IServiceScopeFactory serviceScopeFactory, ILogger<TransferMessageReceiver> logger)
        {
            _hostname = rabbitMqOptions.Value.Hostname;
            _queueName = rabbitMqOptions.Value.QueueName;
            _username = rabbitMqOptions.Value.UserName;
            _password = rabbitMqOptions.Value.Password;
            //_transferCreatedEventHandler = transferCreatedEventHandler;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            InitializeRabbitMqListener().GetAwaiter().GetResult();
        }

        private async Task InitializeRabbitMqListener()
        {
            var factory = new ConnectionFactory
            {
                HostName = _hostname,
                UserName = _username,
                Password = _password
            };

            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();
            await _channel.QueueDeclareAsync(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (ch, ea) =>
            {
                var content = Encoding.UTF8.GetString(ea.Body.ToArray());
                var transferCreatedEvent = System.Text.Json.JsonSerializer.Deserialize<TransferCreatedEvent>(content);
                bool processedSuccessfully = false;
                try
                {
                    processedSuccessfully = HandleMessage(transferCreatedEvent);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Exception occurred while processing message from queue {_queueName}: {ex}");
                }
                if (processedSuccessfully)
                {
                    await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                }
                else
                {
                    await _channel.BasicRejectAsync(deliveryTag: ea.DeliveryTag, requeue: true);
                }
            };

            await _channel.BasicConsumeAsync(queue: _queueName, autoAck: false, consumer: consumer);

            //return Task.CompletedTask;
        }

        private bool HandleMessage(TransferCreatedEvent transferCreatedEvent)
        {
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var _transferCreatedEventHandler = scope.ServiceProvider.GetService<IEventHandler<TransferCreatedEvent>>();
                    _transferCreatedEventHandler.Handle(transferCreatedEvent);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing message: {ex.Message}");
                return false;
            }
        }

        public override void Dispose()
        {
            _channel.CloseAsync();
            _connection.CloseAsync();
            base.Dispose();
        }
    }
}