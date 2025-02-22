using MicroRabbit.Domain.Core.Commands;
using MicroRabbit.Domain.Core.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MicroRabbit.Domain.Core.Bus
{
    public interface IEventBus
    {
        Task SendCommand<T>(T command) where T : Command;
        Task Publish<T>(T @event) where T : Event;
        //event is reserved keyword
        //Publish and Subscribe events
        Task Subscribe<TE, TEH>()
            where TE : Event
            where TEH : IEventHandler<TE>;    
    }
}
