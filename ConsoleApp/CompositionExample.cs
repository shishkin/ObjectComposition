using System;
using System.Linq;

namespace DataContextInteraction.CompositionExample
{
    using System.Collections.ObjectModel;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.Reflection;

    [Export]
    public class Account
    {
        public Collection<AccountOperation> Operations =
            new Collection<AccountOperation>();
    }

    public class AccountOperation
    {
        public AccountOperation(decimal amount, string description)
        {
            Amount = amount;
            Description = description;
            Timestamp = DateTime.Now;
        }

        public decimal Amount { get; private set; }

        public string Description { get; private set; }

        public DateTime Timestamp { get; private set; }
    }

    [Export]
    public class AccountWithBalance
    {
        [Import]
        private Account account;

        public decimal Balance
        {
            get { return account.Operations.Sum(x => x.Amount); }
        }
    }

    [Export]
    public class WithdrawableAccount
    {
        [Import]
        private Account account;

        [Import]
        private AccountWithBalance balance;

        public void Withdraw(decimal amount)
        {
            if (balance.Balance < amount)
            {
                throw new ApplicationException("Insufficient funds.");
            }

            account.Operations.Add(new AccountOperation(-amount, "Cash withdrawal"));
        }
    }

    public class CashWithdrawal
    {
        [Import]
        private WithdrawableAccount account;

        public decimal amount;

        public void Trigger()
        {
            account.Withdraw(amount);
        }
    }

    public class Sample
    {
        public static void Run()
        {
            var entity = new Account();
            entity.Operations.Add(new AccountOperation(300, "init"));
            var context = new CashWithdrawal();

            var catalog = new AssemblyCatalog(Assembly.GetExecutingAssembly());
            var container = new CompositionContainer(catalog);
            var batch = new CompositionBatch();
            batch.AddPart(entity);
            batch.AddExportedValue(100m);
            batch.AddPart(context);
            container.Compose(batch);

            context.Trigger();
        }
    }
}