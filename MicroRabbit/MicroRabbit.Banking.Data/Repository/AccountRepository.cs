using MicroRabbit.Banking.Data.Context;
using MicroRabbit.Banking.Domain.Interfaces;
using MicroRabbit.Banking.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MicroRabbit.Banking.Data.Repository
{
    public class AccountRepository : IAccountRepository
    {
        private readonly BankingDbContext _bankingDbContext;
        public AccountRepository(BankingDbContext bankingDbContext)
        {
            _bankingDbContext = bankingDbContext;
        }
        public IEnumerable<Account> GetAccounts()
        {
            return _bankingDbContext.Accounts;
        }
    }
}
