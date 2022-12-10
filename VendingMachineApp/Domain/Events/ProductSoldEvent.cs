using System.Collections.Generic;
using EventFlow.Aggregates;
using VendingMachineApp.Domain.Entities;

namespace VendingMachineApp.Domain.Events
{
    public class ProductSoldEvent : AggregateEvent<VendingMachineAggregate, VendingMachineId>
    {
        public ProductSoldEvent(int productId, List<CoinsInfo> change, List<CoinsInfo> updatedWallet, List<CoinsInfo> updatedInsertedCoins, List<Product> updatedProducts)
        {
            ProductId = productId;
            Change = change;
            UpdatedWallet = updatedWallet; 
            UpdatedInsertedCoins = updatedInsertedCoins;
            UpdatedProducts = updatedProducts;
        }

        public int ProductId { get; }

        public List<CoinsInfo> Change { get; }

        public List<CoinsInfo> UpdatedWallet { get; }

        public List<CoinsInfo> UpdatedInsertedCoins { get; }

        public List<Product> UpdatedProducts { get; }
    }
}
