using System;
using System.Linq;

namespace DataContextInteraction
{
    using System.Collections.Generic;

    /// <summary>
    /// The only class of the "composition framework".
    /// </summary>
    public abstract class RoleFor<T>
    {
        public T This { protected get; set; }
    }

    /// <summary>
    /// Dumb data: value object.
    /// </summary>
    public class AccountOperation
    {
        public AccountOperation(decimal amount, string description)
        {
            Amount = amount;
            Description = description;
        }

        public decimal Amount { get; private set; }

        public string Description { get; private set; }
    }

    /// <summary>
    /// Dumb data: entity.
    /// </summary>
    public class Account
    {
        private readonly List<AccountOperation> operations = new List<AccountOperation>();

        public Account(string number)
        {
            Number = number;
        }

        public string Number { get; private set; }

        public IEnumerable<AccountOperation> Operations
        {
            get { return operations.AsEnumerable(); }
        }

        public void AddOperation(AccountOperation operation)
        {
            operations.Add(operation);
            Console.WriteLine("{0}: {1:c} {2}",
                Number,
                operation.Amount,
                operation.Description);
        }
    }

    /// <summary>
    /// "Intermediate" role for code reuse.
    /// </summary>
    public class AccountWithBalance : RoleFor<Account>
    {
        public decimal Balance
        {
            get { return This.Operations.Sum(x => x.Amount); }
        }
    }

    /// <summary>
    /// Role of the MoneyTransfer context.
    /// </summary>
    public class SourceAccount : AccountWithBalance
    {
        public void TransferTo(decimal amount, DestinationAccount destination)
        {
            if (Balance < amount)
            {
                throw new ApplicationException("Insufficient funds.");
            }

            This.AddOperation(new AccountOperation(
                -amount,
                string.Format("Transfer to {0}", destination.Number)));

            destination.AcceptTransferFrom(amount, This.Number);
        }
    }

    /// <summary>
    /// Role of the MoneyTransferContext
    /// </summary>
    public class DestinationAccount : RoleFor<Account>
    {
        public string Number
        {
            get { return This.Number; }
        }

        public void AcceptTransferFrom(decimal amount, string sourceAccountNumber)
        {
            This.AddOperation(new AccountOperation(
                amount,
                string.Format("Transfer from {0}", sourceAccountNumber)));
        }
    }

    /// <summary>
    /// Context.
    /// </summary>
    public class MoneyTransfer
    {
        public decimal Amount;
        public DestinationAccount Destination;
        public SourceAccount Source;

        public void Execute()
        {
            Source.TransferTo(Amount, Destination);
        }
    }

    /// <summary>
    /// Role of the BalanceInquiry context.
    /// </summary>
    public class BalanceInquiryAccount : AccountWithBalance
    {
        public void ReportBalance()
        {
            Console.WriteLine("Balance of {0}: {1:c}", This.Number, Balance);
        }
    }

    /// <summary>
    /// Context.
    /// </summary>
    public class BalanceInquiry
    {
        public BalanceInquiryAccount Account;

        public void Execute()
        {
            Account.ReportBalance();
        }
    }

    public static class StaticRolesExample
    {
        public static void Run()
        {
            var source = new Account("1001");
            source.AddOperation(new AccountOperation(2000, "Deposit"));
            var destination = new Account("1002");

            new BalanceInquiry
            {
                Account = new BalanceInquiryAccount
                {
                    This = source
                }
            }.Execute();

            new BalanceInquiry
            {
                Account = new BalanceInquiryAccount
                {
                    This = destination
                }
            }.Execute();

            new MoneyTransfer
            {
                Source = new SourceAccount
                {
                    This = source
                },
                Destination = new DestinationAccount
                {
                    This = destination
                },
                Amount = 1550
            }.Execute();

            new BalanceInquiry
            {
                Account = new BalanceInquiryAccount
                {
                    This = source
                }
            }.Execute();

            new BalanceInquiry
            {
                Account = new BalanceInquiryAccount
                {
                    This = destination
                }
            }.Execute();
        }
    }
}