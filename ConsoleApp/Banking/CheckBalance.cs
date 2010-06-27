using System;

namespace DataContextInteraction.Banking
{
    using ObjectComposition;

    public class CheckBalance
    {
        public Account TheAccount { get; set; }

        public void Execute()
        {
            Console.WriteLine("{0}: {1:c}", TheAccount.Id, TheAccount.Balance);
        }

        public class Account : Role
        {
            public Account(Entity entity) : base(entity) { }

            public decimal Balance
            {
                get { return Entity.Balance; }
            }
        }
    }
}
