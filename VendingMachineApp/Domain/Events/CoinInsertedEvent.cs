using System.Collections.Generic;
using EventFlow.Aggregates;
using VendingMachineApp.Domain.Entities;

namespace VendingMachineApp.Domain.Events
{
    public class CoinInsertedEvent : AggregateEvent<VendingMachineAggregate, VendingMachineId>
    {
        public CoinInsertedEvent(Coin coin, List<CoinsInfo> updatedInsertedCoins)
        {
            Coin = coin;
            UpdatedInsertedCoins = updatedInsertedCoins;
        }

        public Coin Coin { get; }
        public List<CoinsInfo> UpdatedInsertedCoins { get; }
    }
}
