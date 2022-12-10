using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using EventFlow.Aggregates;
using EventFlow.Aggregates.ExecutionResults;
using VendingMachineApp.Domain.Entities;
using VendingMachineApp.Domain.Events;

namespace VendingMachineApp.Domain
{
    //TODO: Check entities and valueobjects
    //TODO: Maybe change from list products, and coins to Dictionarys
    //TODO: In Method CancelPurchase. maybe to check if there are any coins inserted. but not necessary
    //TODO: ProductId chaneg to Guid
    //TODO: Instead of query use subscribers
    //TODO: WHat happens if change is not enough. Not available coins
    //TODO: Do more testing
    public class VendingMachineAggregate : AggregateRoot<VendingMachineAggregate, VendingMachineId>,
        IEmit<CoinInsertedEvent>,
        IEmit<PurchaseCanceledEvent>,
        IEmit<ProductSoldEvent>,
        IEmit<InitializedEvent>
    {

        public List<Product> Products = new List<Product>();

        public List<CoinsInfo> WalletCoins = new List<CoinsInfo>();

        public List<CoinsInfo> InsertedCoins = new List<CoinsInfo>();


        public VendingMachineAggregate(VendingMachineId id) : base(id)
        {
        }

        #region Aggregate Methods

        public IExecutionResult Init()
        {
            var products = new List<Product>()
            {
                new Product(1, "Tea", 1.30M, 10),
                new Product(2, "Espresso", 1.80M, 20),
                new Product(3, "Juice", 1.80M, 20),
                new Product(4, "Chicken soup", 1.80M, 15)
            };

            var walletCoins = new List<CoinsInfo>()
            {
                new CoinsInfo(Coin.Cent10, 100),
                new CoinsInfo(Coin.Cent20, 100),
                new CoinsInfo(Coin.Cent50, 100),
                new CoinsInfo(Coin.OneEuro, 100)
            };

            var insertedCoins = GetEmptyInsertedCoins();

            Emit(new InitializedEvent(products, walletCoins, insertedCoins));
            return ExecutionResult.Success();
        }


        public IExecutionResult InsertCoin(Coin coin)
        {
            var updatedInsertedCoinss = InsertedCoins;
            var coinInfo = updatedInsertedCoinss.SingleOrDefault(x => x.Coin.value == coin.value);
            coinInfo.Quantity += 1;

            Emit(new CoinInsertedEvent(coin, updatedInsertedCoinss));
            return ExecutionResult.Success();
        }

        public IExecutionResult CancelPurchase()
        {
            var updatedInsertedCoins = GetEmptyInsertedCoins();
            Emit(new PurchaseCanceledEvent(updatedInsertedCoins));
            return ExecutionResult.Success();
        }

        public IExecutionResult SellProduct(int productId)
        {
            var product = Products.FirstOrDefault(x => x.Id == productId);
            if (product == null)
            {
                return ExecutionResult.Failed($"No product found with Id:{productId}");
            }

            if (product.Quantity == 0)
            {
                return ExecutionResult.Failed("Product sold out");
            }

            var insertedMoney = InsertedCoins.Sum(x => x.Coin.value * x.Quantity);
            if (insertedMoney < product.Price)
            {
                return ExecutionResult.Failed("Insufficient amount of money");
            }

            var coinsResult = GetCoinsResult(insertedMoney, product.Price);

            var updatedProducts = Products;
            updatedProducts.FirstOrDefault(x => x.Id == productId).Quantity -= 1;

            Emit(new ProductSoldEvent(productId, coinsResult.Change, coinsResult.UpdatedWallet, coinsResult.UpdatedInsertedCoins, updatedProducts));
            return ExecutionResult.Success();
        }

        #endregion


        #region private Methods

        private List<CoinsInfo> GetEmptyInsertedCoins()
        {
            return new List<CoinsInfo>()
            {
                new CoinsInfo(Coin.Cent10, 0),
                new CoinsInfo(Coin.Cent20, 0),
                new CoinsInfo(Coin.Cent50, 0),
                new CoinsInfo(Coin.OneEuro, 0)
            };
        }

        private SellProductCoinsResult GetCoinsResult(decimal insertedMoney, decimal productPrice)
        {
            var changeCoins = new List<CoinsInfo>()
            {
                new CoinsInfo(Coin.Cent10, 0),
                new CoinsInfo(Coin.Cent20, 0),
                new CoinsInfo(Coin.Cent50, 0),
                new CoinsInfo(Coin.OneEuro, 0)
            };

            var change = insertedMoney - productPrice;
            var vendingCoins = GetVendingCoins().OrderByDescending(x => x.Coin.value).ToList();
            if (change > 0)
            {
                var remainingChange = change;
                foreach (var vendingCoinInfo in vendingCoins)
                {

                    for (int i = 0; i < vendingCoinInfo.Quantity; i++)
                    {
                        if (remainingChange < vendingCoinInfo.Coin.value)
                        {
                            break;
                        }
                        remainingChange -= vendingCoinInfo.Coin.value;
                        vendingCoinInfo.Quantity -= 1;

                        var changeCoinInfo = changeCoins.FirstOrDefault(x => x.Coin.value == vendingCoinInfo.Coin.value);
                        changeCoinInfo.Quantity += 1;
                    }

                    if (remainingChange == 0)
                    {
                        break;
                    }
                }
            }

            var insertedCoins = GetEmptyInsertedCoins();

            return new SellProductCoinsResult(vendingCoins, changeCoins, insertedCoins);
        }

        private List<CoinsInfo> GetVendingCoins()
        {
            var coinsInfo = WalletCoins;
            foreach (var insertedCoin in InsertedCoins)
            {
                var coinInfo = coinsInfo.FirstOrDefault(x => x.Coin.value == insertedCoin.Coin.value);
                coinInfo.Quantity += insertedCoin.Quantity;
            }

            return coinsInfo;
        }

        # endregion 


        #region Apply Events

        public void Apply(InitializedEvent aggregateEvent)
        {
            Products = aggregateEvent.Products;
            InsertedCoins = aggregateEvent.InsertedCoins;
            WalletCoins = aggregateEvent.WalletCoins;
        }

        public void Apply(CoinInsertedEvent aggregateEvent)
        {
            InsertedCoins = aggregateEvent.UpdatedInsertedCoins;
        }

        public void Apply(PurchaseCanceledEvent aggregateEvent)
        {
            InsertedCoins = aggregateEvent.UpdatedInsertedCoins;
        }

        public void Apply(ProductSoldEvent aggregateEvent)
        {
            InsertedCoins = aggregateEvent.UpdatedInsertedCoins;
            WalletCoins = aggregateEvent.UpdatedWallet;
            Products = aggregateEvent.UpdatedProducts;
        }

        #endregion
    }

    internal class SellProductCoinsResult
    {
        public SellProductCoinsResult(List<CoinsInfo> updatedWallet, List<CoinsInfo> change, List<CoinsInfo> updatedInsertedCoins)
        {
            UpdatedWallet = updatedWallet;
            Change = change;
            UpdatedInsertedCoins = updatedInsertedCoins;
        }

        public List<CoinsInfo> UpdatedWallet { get; }
        public List<CoinsInfo> Change { get; }
        public List<CoinsInfo> UpdatedInsertedCoins { get; }
    }
}
