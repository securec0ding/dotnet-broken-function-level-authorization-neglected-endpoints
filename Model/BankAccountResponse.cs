using System;

namespace Backend.Model
{
    public class BankAccountResponse
    {
        public string Id { get; set; }

        public string User { get; set; }

        public string SSN { get; set; }

        public decimal Balance { get; set; }

        public BankAccountResponse(BankAccount account)
        {
            Id = account.Id.ToString();
            User = account.UserName;
            SSN = account.SSN;
            Balance = account.Balance;
        }
    }
}