using MicroRabbit.Transfer.Data.Context;
using MicroRabbit.Transfer.Domain.Events;
using MicroRabbit.Transfer.Domain.Interfaces;
using MicroRabbit.Transfer.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MicroRabbit.Transfer.Data.Repository
{
    public class TransferRepository : ITransferRepository
    {
        private readonly TransferDbContext _transferDBContext;
        public TransferRepository(TransferDbContext transferDbContext)
        {
            _transferDBContext = transferDbContext;
        }

        public IEnumerable<TransferLog> GetTransferLogs()
        {
            return _transferDBContext.TransferLogs;
        }

        public void AddTransferLog(TransferLog transferLog)
        {
            _transferDBContext.TransferLogs.Add(transferLog);
            _transferDBContext.SaveChanges();
        }
    }
}
