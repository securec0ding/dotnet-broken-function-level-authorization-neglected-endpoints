
using System;

namespace Backend.Model
{
    public class BankAccount
    {
        public Guid Id { get; set; }

        public string UnsecureId { get; set; }

        public string UserId { get; set; }

        public string UserName { get; set; }

        public string SSN { get; set; }

        public decimal Balance { get; set; }
    }
}