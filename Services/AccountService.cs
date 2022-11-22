using Backend.Data;
using Backend.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Services
{
    public class AccountService : IAccountService
    {
        private ApplicationDbContext _db;

        public AccountService(ApplicationDbContext dbContext)
        {
            _db = dbContext;
        }

        public BankAccount GetAccountById(string userId, Guid accountId)
        {
            var account = _db.Accounts.FirstOrDefault(a => a.UserId == userId && a.Id == accountId);

            if(account != null)
            {
                account.UnsecureId = account.Id.ToString();
            }

            return account;
        }

        public async Task<List<BankAccount>> GetAccounts()
        {
            var accounts = await _db.Accounts.ToListAsync();

            return accounts;
        }
    }
}
