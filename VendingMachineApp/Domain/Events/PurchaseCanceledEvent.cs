using System.Collections.Generic;
using EventFlow.Aggregates;
using VendingMachineApp.Domain.Entities;

namespace VendingMachineApp.Domain.Events
{
    public class PurchaseCanceledEvent: AggregateEvent<VendingMachineAggregate, VendingMachineId>
    {
        public PurchaseCanceledEvent(List<CoinsInfo> updatedInsertedCoins)
        {
            UpdatedInsertedCoins = updatedInsertedCoins;
        }

        public List<CoinsInfo> UpdatedInsertedCoins { get; }
    }
}
