using MicroRabbit.Domain.Core.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace MicroRabbit.Domain.Core.Commands
{
    public abstract class Command : Message
    {
        public DateTime Timestamp { get; protected set; } //Basic property is this command has to send some time.
        protected Command()
        {
            Timestamp = DateTime.Now;
        }
    } 
}
