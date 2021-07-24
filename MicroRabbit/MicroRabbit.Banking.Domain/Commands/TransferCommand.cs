using MicroRabbit.Domain.Core.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace MicroRabbit.Banking.Domain.Commands
{
    //Similarly we can have CancelTransferCommand, RetryTransferCommand
    public abstract class TransferCommand : Command
    {
        public int From { get; protected set; }
        public int To { get; protected set; }
        public decimal Amount { get; protected set; }
    }
}
