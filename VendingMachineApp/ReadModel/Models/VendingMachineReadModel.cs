using System.Collections.Generic;
using System.Linq;
using EventFlow.Aggregates;
using EventFlow.ReadStores;
using VendingMachineApp.Domain;
using VendingMachineApp.Domain.Events;
using VendingMachineApp.ReadModel.Models.Mappers;

namespace VendingMachineApp.ReadModel.Models
{
    public class VendingMachineReadModel :
        IReadModel,
        IAmReadModelFor<VendingMachineAggregate, VendingMachineId, InitializedEvent>,
        IAmReadModelFor<VendingMachineAggregate, VendingMachineId, CoinInsertedEvent>,
        IAmReadModelFor<VendingMachineAggregate, VendingMachineId, PurchaseCanceledEvent>,
        IAmReadModelFor<VendingMachineAggregate, VendingMachineId, ProductSoldEvent>
    {
        public string AggregateId { get; set; }

        public List<ProductModel> Products { get; set; }

        public List<CoinsInfoModel> WalletCoins { get; set; }

        public List<CoinsInfoModel> InsertedCoins { get; set; }

        public List<CoinsInfoModel> ChangeCoins { get; set; }


        public void Apply(IReadModelContext context, IDomainEvent<VendingMachineAggregate, VendingMachineId, CoinInsertedEvent> domainEvent)
        {
            ChangeCoins = new List<CoinsInfoModel>();
            InsertedCoins = domainEvent.AggregateEvent.UpdatedInsertedCoins.Select(ReadModelMapper.Map).ToList();
        }

        public void Apply(IReadModelContext context, IDomainEvent<VendingMachineAggregate, VendingMachineId, PurchaseCanceledEvent> domainEvent)
        {
            ChangeCoins = InsertedCoins;
            InsertedCoins = domainEvent.AggregateEvent.UpdatedInsertedCoins.Select(ReadModelMapper.Map).ToList();

        }

        public void Apply(IReadModelContext context, IDomainEvent<VendingMachineAggregate, VendingMachineId, ProductSoldEvent> domainEvent)
        {
            WalletCoins = domainEvent.AggregateEvent.UpdatedWallet.Select(ReadModelMapper.Map).ToList();
            ChangeCoins = domainEvent.AggregateEvent.Change.Select(ReadModelMapper.Map).ToList();
            InsertedCoins = domainEvent.AggregateEvent.UpdatedInsertedCoins.Select(ReadModelMapper.Map).ToList();
            Products = domainEvent.AggregateEvent.UpdatedProducts.Select(ReadModelMapper.Map).ToList();
        }

        public void Apply(IReadModelContext context, IDomainEvent<VendingMachineAggregate, VendingMachineId, InitializedEvent> domainEvent)
        {
            ChangeCoins = new List<CoinsInfoModel>();
            AggregateId = domainEvent.AggregateIdentity.Value;
            Products = domainEvent.AggregateEvent.Products.Select(ReadModelMapper.Map).ToList();
            InsertedCoins = domainEvent.AggregateEvent.InsertedCoins.Select(ReadModelMapper.Map).ToList();
            WalletCoins = domainEvent.AggregateEvent.WalletCoins.Select(ReadModelMapper.Map).ToList();
        }
    }
}
