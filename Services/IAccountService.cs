using Backend.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.Services
{
    public interface IAccountService
    {
        BankAccount GetAccountById(string userId, Guid accountId);

        Task<List<BankAccount>> GetAccounts();
    }
}