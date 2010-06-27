using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataContextInteraction
{
    using ObjectComposition;
    using Banking;

    class Program
    {
        static void Main(string[] args)
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
