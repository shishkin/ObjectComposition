namespace DataContextInteraction
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;

    /// <summary>
    /// Generic property bag implementation of an entity.
    /// </summary>
    public class Entity : DynamicObject
    {
        private readonly Dictionary<string, object> data = new Dictionary<string, object>();

        public Entity(string id)
        {
            Id = id;
        }

        public string Id { get; private set; }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return data.TryGetValue(binder.Name, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            data[binder.Name] = value;
            return true;
        }
    }

    /// <summary>
    /// Base class for roles.
    /// </summary>
    public abstract class Role
    {
        public Role(Entity entity)
        {
            Entity = entity;
        }

        protected dynamic Entity { get; private set; }

        public string Id
        {
            get { return Entity.Id; }
        }
    }

    /// <summary>
    /// Context.
    /// </summary>
    public class TransferMoney
    {
        public SourceAccount Source { get; set; }
        public DestinationAccount Destination { get; set; }
        public decimal Amount { get; set; }

        /// <summary>
        /// Use-case algorithm.
        /// </summary>
        public void Execute()
        {
            if (Source.Balance < Amount)
            {
                throw new ApplicationException("Insufficient funds.");
            }

            Source.DecreseBalanceBy(Amount);
            Destination.IncreaseBalanceBy(Amount);
        }

        /// <summary>
        /// Role.
        /// </summary>
        public class SourceAccount : Role
        {
            public SourceAccount(Entity entity) : base(entity) { }

            public decimal Balance
            {
                get { return Entity.Balance; }
            }

            public void DecreseBalanceBy(decimal amount)
            {
                Entity.Balance -= amount;
            }
        }

        /// <summary>
        /// Role.
        /// </summary>
        public class DestinationAccount : Role
        {
            public DestinationAccount(Entity entity) : base(entity) { }

            public void IncreaseBalanceBy(decimal amount)
            {
                Entity.Balance += amount;
            }
        }
    }

    /// <summary>
    /// Context.
    /// </summary>
    public class CheckBalance
    {
        public Account TheAccount { get; set; }

        /// <summary>
        /// Use-case algorithm.
        /// </summary>
        public void Execute()
        {
            Console.WriteLine("{0}: {1:c}", TheAccount.Id, TheAccount.Balance);
        }

        /// <summary>
        /// Role.
        /// </summary>
        public class Account : Role
        {
            public Account(Entity entity) : base(entity) { }

            public decimal Balance
            {
                get { return Entity.Balance; }
            }
        }
    }

    public static class DynamicDataExample
    {
        public static void Run()
        {
            // Create entities and pre-initialize data
            dynamic sourceAccount = new Entity("account/1");
            sourceAccount.Balance = 1000m;
            dynamic destinationAccount = new Entity("account/2");
            destinationAccount.Balance = 1000m;

            // Create CheckBalance contexts
            // TODO: Potential usage scenario for a DI container
            var sourceBalance = new CheckBalance
            {
                TheAccount = new CheckBalance.Account(sourceAccount)
            };
            var destinationBalance = new CheckBalance
            {
                TheAccount = new CheckBalance.Account(destinationAccount)
            };

            Console.WriteLine("Balances before transfer:");
            sourceBalance.Execute();
            destinationBalance.Execute();

            // Make transfer via the TransferMoney context
            new TransferMoney
            {
                Source = new TransferMoney.SourceAccount(sourceAccount),
                Destination = new TransferMoney.DestinationAccount(destinationAccount),
                Amount = 350
            }
                .Execute();

            Console.WriteLine("Balances after transfer:");
            sourceBalance.Execute();
            destinationBalance.Execute();
        }
    }
}