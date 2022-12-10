using EventFlow.Aggregates;
using System.Collections.Generic;
using VendingMachineApp.Domain.Entities;

namespace VendingMachineApp.Domain.Events
{
    public class InitializedEvent : AggregateEvent<VendingMachineAggregate, VendingMachineId>
    {
        public InitializedEvent(List<Product> products, List<CoinsInfo> walletCoins, List<CoinsInfo> insertedCoins)
        {
            Products = products;
            WalletCoins = walletCoins;
            InsertedCoins = insertedCoins;
        }

        public List<Product> Products { get; }
        public List<CoinsInfo> WalletCoins { get; }
        public List<CoinsInfo> InsertedCoins { get; }
    }
}
