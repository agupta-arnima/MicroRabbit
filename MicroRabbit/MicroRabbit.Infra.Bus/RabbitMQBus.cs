using MediatR;
using MicroRabbit.Domain.Core.Bus;
using MicroRabbit.Domain.Core.Commands;
using MicroRabbit.Domain.Core.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MicroRabbit.Infra.Bus
{
    public sealed class RabbitMQBus : IEventBus
    {
        private readonly IMediator _mediator;
        private readonly Dictionary<string, List<Type>> _handlers;
        private readonly List<Type> _eventTypes;

        public RabbitMQBus(IMediator mediator)
        {
            _mediator = mediator;
            _handlers = new Dictionary<string, List<Type>>();
            _eventTypes = new List<Type>();
        }
        public Task SendCommand<T>(T command) where T : Command
        {
            return _mediator.Send(command);
        }

        public void Publish<T>(T @event) where T : Event
        {
            var factory = new ConnectionFactory()
            {
                HostName = "localhost"
            };
            using (var connection = factory.CreateConnection())  //Open the connection
            using (var channel = connection.CreateModel())       //Open the channel
            {
                var eventName = @event.GetType().Name;
                channel.QueueDeclare(eventName, false, false, false, null); //Queue name and Routing key both are eventname
                var message = System.Text.Json.JsonSerializer.Serialize(@event);
                var body = System.Text.Encoding.UTF8.GetBytes(message);
                channel.BasicPublish("", eventName, null, body);
            }
        }

        public void Subscribe<TE, TEH>()
            where TE : Event
            where TEH : IEventHandler<TE>
        {
            var eventType = typeof(TE);
            var handlerType = typeof(TEH);

            if (!_eventTypes.Contains(eventType))
            {
                _eventTypes.Add(eventType);
            }

            if (!_handlers.ContainsKey(eventType.Name))
            {
                _handlers.Add(eventType.Name, new List<Type>() { handlerType });
            }

            if (_handlers[eventType.Name].Any(s => s.GetType() == handlerType)) //Use Any if we want boolean return type
            {
                throw new ArgumentException($"Handler Type {handlerType.Name} is already registered for '{eventType.Name}'", nameof(handlerType));
            }

            //_handlers[eventType.Name].Add(handlerType);

            StartBasicConsume<TE>();
        }

        private void StartBasicConsume<TE>() where TE : Event
        {
            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                DispatchConsumersAsync = true
            };
            using (var connection = factory.CreateConnection())  //Open the connection
            using (var channel = connection.CreateModel())       //Open the channel
            {
                var eventName = typeof(TE).Name;
                channel.QueueDeclare(eventName, false, false, false, null);
                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.Received += Consumer_Received;

                channel.BasicConsume(eventName, true, consumer);  //Bind consumer with Queue
            }
        }

        private async Task Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            var eventName = e.RoutingKey;
            var message = System.Text.Encoding.UTF8.GetString(e.Body.ToArray());
            try
            {
                //Based on Handlers we will process our event
                await ProcessEvent(eventName, message).ConfigureAwait(false);
            }
            catch (Exception ex)
            {

            }
        }

        private async Task ProcessEvent(string eventName, string message)
        {
            if (_handlers.ContainsKey(eventName))
            {
                var subscriptions = _handlers[eventName];
                foreach (Type subscription in subscriptions)
                {
                    var handler = Activator.CreateInstance(subscription);
                    if (handler == null)
                        continue;
                    var eventType = _eventTypes.SingleOrDefault(t => t.Name == eventName);
                    var @event = System.Text.Json.JsonSerializer.Deserialize(message, eventType);  //Deserialize in eventType
                    var concreteType = typeof(IEventHandler<>).MakeGenericType(eventType);
                    //await (Task)subscription.GetMethod("Handle").Invoke(handler, new object[] { @event });
                    await (Task)concreteType.GetMethod("Handle").Invoke(handler, new object[] { @event });
                }
            }
        }
    }
}
