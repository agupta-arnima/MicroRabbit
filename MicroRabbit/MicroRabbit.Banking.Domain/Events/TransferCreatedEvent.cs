using MicroRabbit.Domain.Core.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace MicroRabbit.Banking.Domain.Events
{
    //Command was CreateTransferCommand but event is TransferCreatedEvent
    //Transfer Completed event
    public class TransferCreatedEvent : Event
    {
        public int From { get; protected set; }
        public int To { get; protected set; }
        public decimal Amount { get; protected set; }
        public TransferCreatedEvent(int from, int to, decimal amount)
        {
            From = from;
            To = to;
            Amount = amount;
        }
    }
}
