using System;

namespace DataContextInteraction.Banking
{
    using ObjectComposition;

    public class TransferMoney
    {
        public SourceAccount Source { get; set; }
        public DestinationAccount Destination { get; set; }
        public decimal Amount { get; set; }

        public void Execute()
        {
            if (Source.Balance < Amount)
            {
                throw new ApplicationException("Insufficient funds.");
            }

            Source.DecreseBalanceBy(Amount);
            Destination.IncreaseBalanceBy(Amount);
        }

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

        public class DestinationAccount : Role
        {
            public DestinationAccount(Entity entity) : base(entity) { }

            public void IncreaseBalanceBy(decimal amount)
            {
                Entity.Balance += amount;
            }
        }
    }
}
