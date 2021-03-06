using System;
using System.Linq;

namespace DataContextInteraction.CompositionExample
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.ComponentModel.Composition.Primitives;
    using System.Reflection;

    public class Sample
    {
        public static void Run()
        {
            var composer = new Composer();
            
            var account = new Account(300);
            var reporter = composer.CastTo<ReportingAccount>(account); 

            reporter.ReportBalance();
            
            composer.Compose<CashWithdrawal>(account, 120m).Trigger();
            
            reporter.ReportBalance();
            reporter.ReportOperations();
        }
    }

    /// <summary>
    /// Dumb entity data object.
    /// </summary>
    [Export]
    public class Account
    {
        public Collection<AccountOperation> Operations =
            new Collection<AccountOperation>();

        public Account(decimal initialDeposit)
        {
            Operations.Add(new AccountOperation(initialDeposit, "Initial deposit"));
        }
    }

    /// <summary>
    /// Immutable value object.
    /// </summary>
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

    /// <summary>
    /// Role based directly on the entity.
    /// </summary>
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

    /// <summary>
    /// Role based on entity and on another role.
    /// </summary>
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

    /// <summary>
    /// Context.
    /// </summary>
    [Export]
    public class CashWithdrawal
    {
        [Import]
        private WithdrawableAccount account;

        [Import]
        private decimal amount;

        public void Trigger()
        {
            account.Withdraw(amount);
        }
    }

    /// <summary>
    /// Role.
    /// </summary>
    [Export]
    public class ReportingAccount
    {
        [Import]
        private Account account;

        [Import]
        private AccountWithBalance balance;

        public void ReportBalance()
        {
            Console.WriteLine("Balance: {0:c}", balance.Balance);
        }

        public void ReportOperations()
        {
            Console.WriteLine("Account operations:");
            account.Operations.ToList()
                .ForEach(x => Console.WriteLine("\t{0:c} {1}", x.Amount, x.Description));
        }
    }

    /// <summary>
    /// "Fx".
    /// </summary>
    public class Composer
    {
        private static readonly ComposablePartCatalog Catalog =
            new AssemblyCatalog(Assembly.GetExecutingAssembly());

        public T Compose<T>(params object[] dependencies)
        {
            var container = new CompositionContainer(Catalog);
            var batch = new CompositionBatch();
            dependencies
                .ToList().ForEach(x => AddExportedValue(batch, x));
            container.Compose(batch);
            return container.GetExportedValue<T>();
        }

        public T CastTo<T>(object entity, params object[] dependencies)
        {
            return Compose<T>(
                Enumerable.Repeat(entity, 1)
                .Concat(dependencies)
                .ToArray());
        }

        /// <summary>
        /// I wish MEF provided this method out of the box.
        /// </summary>
        private static ComposablePart AddExportedValue(CompositionBatch batch, object exportedValue)
        {
            var contractName = AttributedModelServices.GetContractName(exportedValue.GetType());
            var typeIdentity = AttributedModelServices.GetTypeIdentity(exportedValue.GetType());
            var metadata = new Dictionary<string, object>
            {
                { "ExportTypeIdentity", typeIdentity }
            };
            return batch.AddExport(new Export(contractName, metadata, () => exportedValue));
        }
    }
}