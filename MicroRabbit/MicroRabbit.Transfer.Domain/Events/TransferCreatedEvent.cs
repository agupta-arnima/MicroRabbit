using MicroRabbit.Domain.Core.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace MicroRabbit.Transfer.Domain.Events
{
    //Command was CreateTransferCommand but event is TransferCreatedEvent
    //Transfer Completed event
    public class TransferCreatedEvent : Event
    {
        public TransferCreatedEvent()
        {
        }
        public int From { get; set; }
        public int To { get; set; }
        public decimal Amount { get; set; }
        public TransferCreatedEvent(int from, int to, decimal amount)
        {
            From = from;
            To = to;
            Amount = amount;
        }
    }
}
