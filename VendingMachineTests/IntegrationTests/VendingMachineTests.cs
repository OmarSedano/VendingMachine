using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventFlow;
using EventFlow.Aggregates.ExecutionResults;
using EventFlow.Extensions;
using EventFlow.Queries;
using FluentAssertions;
using NUnit.Framework;
using VendingMachineApp.Domain;
using VendingMachineApp.Domain.Commands;
using VendingMachineApp.Domain.Entities;
using VendingMachineApp.Domain.Events;
using VendingMachineApp.ReadModel.Models;
using VendingMachineApp.ReadModel.Queries;
using VendingMachineApp.ReadModel.QueryHandlers;
using static VendingMachineApp.Domain.Commands.InitCommand;

namespace VendingMachineTests.IntegrationTests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task SellProduct_WhenInsertedMoneyIsLessThanProductPrice_ThenReturnExecutionFailed()
        {
            using (var resolver = EventFlowOptions.New
                       .AddEvents(typeof(CoinInsertedEvent), typeof(PurchaseCanceledEvent), typeof(ProductSoldEvent),
                           typeof(InitializedEvent))
                       .AddCommands(typeof(InsertCoinCommand), typeof(CancelPurchaseCommand),
                           typeof(SellProductCommand), typeof(InitCommand))
                       .AddCommandHandlers(typeof(InsertCoinCommand.InsertCoinCommandHandler),
                           typeof(CancelPurchaseCommand.CancelPurchaseCommandHandler),
                           typeof(SellProductCommand.SellProductCommandHandler), typeof(InitCommandHandler))
                       .AddQueryHandlers(typeof(VendingMachineByIdQueryHandler))
                       .UseInMemoryReadStoreFor<VendingMachineReadModel>()
                       .CreateResolver())
            {
                var vendingMachineId = VendingMachineId.New;

                var commandBus = resolver.Resolve<ICommandBus>();

                //Launch InitCommand

                var initResult = await commandBus.PublishAsync(
                        new InitCommand(vendingMachineId),
                        CancellationToken.None)
                    .ConfigureAwait(false);

                initResult.IsSuccess.Should().BeTrue();

                //Launch InsertCoinCommand with one coin of 50Cent

                var insertCoin1Result = await commandBus.PublishAsync(
                        new InsertCoinCommand(vendingMachineId, Coin.Cent50),
                        CancellationToken.None)
                    .ConfigureAwait(false);

                insertCoin1Result.IsSuccess.Should().BeTrue();

                //Launch InsertCoinCommand with one coin of 1Euro

                var insertCoin2Result = await commandBus.PublishAsync(
                        new InsertCoinCommand(vendingMachineId, Coin.OneEuro),
                        CancellationToken.None)
                    .ConfigureAwait(false);

                insertCoin2Result.IsSuccess.Should().BeTrue();


                //Launch SellProductCommand. Buy product Espresso with Id 2. Value: 1.80 euros . Quantity: 10

                var productId = 2;
                var sellProductResult = await commandBus.PublishAsync(
                        new SellProductCommand(vendingMachineId, productId),
                        CancellationToken.None)
                    .ConfigureAwait(false);

                sellProductResult.IsSuccess.Should().BeFalse();
                (sellProductResult as FailedExecutionResult).Errors.First().Should().Be("Insufficient amount of money");
            }
        }

        [Test]
        public async Task SellProduct_WhenCalled_ThenChangeAndProductShouldBeUpdated()
        {
            using (var resolver = EventFlowOptions.New
                       .AddEvents(typeof(CoinInsertedEvent), typeof(PurchaseCanceledEvent), typeof(ProductSoldEvent),
                           typeof(InitializedEvent))
                       .AddCommands(typeof(InsertCoinCommand), typeof(CancelPurchaseCommand),
                           typeof(SellProductCommand), typeof(InitCommand))
                       .AddCommandHandlers(typeof(InsertCoinCommand.InsertCoinCommandHandler),
                           typeof(CancelPurchaseCommand.CancelPurchaseCommandHandler),
                           typeof(SellProductCommand.SellProductCommandHandler), typeof(InitCommandHandler))
                       .AddQueryHandlers(typeof(VendingMachineByIdQueryHandler))
                       .UseInMemoryReadStoreFor<VendingMachineReadModel>()
                       .CreateResolver())
            {
                var vendingMachineId = VendingMachineId.New;

                var commandBus = resolver.Resolve<ICommandBus>();

                //Launch InitCommand

                var initResult = await commandBus.PublishAsync(
                        new InitCommand(vendingMachineId),
                        CancellationToken.None)
                    .ConfigureAwait(false);

                initResult.IsSuccess.Should().BeTrue();

                //Launch InsertCoinCommand with one coin of 50Cent

                var insertCoin1Result = await commandBus.PublishAsync(
                        new InsertCoinCommand(vendingMachineId, Coin.Cent50),
                        CancellationToken.None)
                    .ConfigureAwait(false);

                insertCoin1Result.IsSuccess.Should().BeTrue();

                //Launch InsertCoinCommand with one coin of 1Euro

                var insertCoin2Result = await commandBus.PublishAsync(
                        new InsertCoinCommand(vendingMachineId, Coin.OneEuro),
                        CancellationToken.None)
                    .ConfigureAwait(false);

                insertCoin2Result.IsSuccess.Should().BeTrue();


                //Launch SellProductCommand. Buy product Tea with Id 1. Value: 1.30 euros . Quantity: 10

                var productId = 1;
                var sellProductResult = await commandBus.PublishAsync(
                        new SellProductCommand(vendingMachineId, productId),
                        CancellationToken.None)
                    .ConfigureAwait(false);

                sellProductResult.IsSuccess.Should().BeTrue();


                //Launch VendingMachineByIdQuery. Check tea quantity decreases one. Check Change is ok. Check Wallet Coins

                var queryProcessor = resolver.Resolve<IQueryProcessor>();
                var vendingMachineReadModel = await queryProcessor.ProcessAsync(
                        new VendingMachineByIdQuery(vendingMachineId.Value),
                        CancellationToken.None)
                    .ConfigureAwait(false);

                vendingMachineReadModel.Should().NotBeNull();

                //Verify Tea product has 9 products
                vendingMachineReadModel.Products.FirstOrDefault(x => x.Id == productId).Quantity.Should().Be(9);

                //Verify ChangeCoins are 2 coins of 20CENT

                vendingMachineReadModel.ChangeCoins.FirstOrDefault(x => x.Coin == Coin.Cent20.ToString()).Quantity
                    .Should().Be(1);
                vendingMachineReadModel.ChangeCoins.FirstOrDefault(x => x.Coin == Coin.Cent50.ToString()).Quantity
                    .Should().Be(0);
                vendingMachineReadModel.ChangeCoins.FirstOrDefault(x => x.Coin == Coin.Cent10.ToString()).Quantity
                    .Should().Be(0);
                vendingMachineReadModel.ChangeCoins.FirstOrDefault(x => x.Coin == Coin.OneEuro.ToString()).Quantity
                    .Should().Be(0);

                //Verify WalletCoins 

                vendingMachineReadModel.WalletCoins.FirstOrDefault(x => x.Coin == Coin.Cent20.ToString()).Quantity
                    .Should().Be(99);
                vendingMachineReadModel.WalletCoins.FirstOrDefault(x => x.Coin == Coin.Cent50.ToString()).Quantity
                    .Should().Be(101);
                vendingMachineReadModel.WalletCoins.FirstOrDefault(x => x.Coin == Coin.Cent10.ToString()).Quantity
                    .Should().Be(100);
                vendingMachineReadModel.WalletCoins.FirstOrDefault(x => x.Coin == Coin.OneEuro.ToString()).Quantity
                    .Should().Be(101);


                //Verify InsertedCoins 

                vendingMachineReadModel.InsertedCoins.FirstOrDefault(x => x.Coin == Coin.Cent20.ToString()).Quantity
                    .Should().Be(0);
                vendingMachineReadModel.InsertedCoins.FirstOrDefault(x => x.Coin == Coin.Cent50.ToString()).Quantity
                    .Should().Be(0);
                vendingMachineReadModel.InsertedCoins.FirstOrDefault(x => x.Coin == Coin.Cent10.ToString()).Quantity
                    .Should().Be(0);
                vendingMachineReadModel.InsertedCoins.FirstOrDefault(x => x.Coin == Coin.OneEuro.ToString()).Quantity
                    .Should().Be(0);
            }
        }

        [Test]
        public async Task CancelPurchase_WhenCalled_ThenReturnInsertCoinsAsChange()
        {
            using (var resolver = EventFlowOptions.New
                       .AddEvents(typeof(CoinInsertedEvent), typeof(PurchaseCanceledEvent), typeof(ProductSoldEvent),
                           typeof(InitializedEvent))
                       .AddCommands(typeof(InsertCoinCommand), typeof(CancelPurchaseCommand),
                           typeof(SellProductCommand), typeof(InitCommand))
                       .AddCommandHandlers(typeof(InsertCoinCommand.InsertCoinCommandHandler),
                           typeof(CancelPurchaseCommand.CancelPurchaseCommandHandler),
                           typeof(SellProductCommand.SellProductCommandHandler), typeof(InitCommandHandler))
                       .AddQueryHandlers(typeof(VendingMachineByIdQueryHandler))
                       .UseInMemoryReadStoreFor<VendingMachineReadModel>()
                       .CreateResolver())
            {
                var vendingMachineId = VendingMachineId.New;

                var commandBus = resolver.Resolve<ICommandBus>();

                //Launch InitCommand

                var initResult = await commandBus.PublishAsync(
                        new InitCommand(vendingMachineId),
                        CancellationToken.None)
                    .ConfigureAwait(false);

                initResult.IsSuccess.Should().BeTrue();

                //Launch InsertCoinCommand with one coin of 50Cent

                var insertCoin1Result = await commandBus.PublishAsync(
                        new InsertCoinCommand(vendingMachineId, Coin.Cent50),
                        CancellationToken.None)
                    .ConfigureAwait(false);

                insertCoin1Result.IsSuccess.Should().BeTrue();

                //Launch InsertCoinCommand with one coin of 1Euro

                var insertCoin2Result = await commandBus.PublishAsync(
                        new InsertCoinCommand(vendingMachineId, Coin.OneEuro),
                        CancellationToken.None)
                    .ConfigureAwait(false);

                insertCoin2Result.IsSuccess.Should().BeTrue();


                //Launch CancelPurchase.

                var cancelPurchaseResult = await commandBus.PublishAsync(
                        new CancelPurchaseCommand(vendingMachineId),
                        CancellationToken.None)
                    .ConfigureAwait(false);

                cancelPurchaseResult.IsSuccess.Should().BeTrue();

                //Launch VendingMachineByIdQuery. Check inserted coins are 0.

                var queryProcessor = resolver.Resolve<IQueryProcessor>();
                var vendingMachineReadModel = await queryProcessor.ProcessAsync(
                        new VendingMachineByIdQuery(vendingMachineId.Value),
                        CancellationToken.None)
                    .ConfigureAwait(false);

                vendingMachineReadModel.Should().NotBeNull();

                //Verify ChangeCoins 

                vendingMachineReadModel.ChangeCoins.FirstOrDefault(x => x.Coin == Coin.Cent20.ToString()).Quantity
                    .Should().Be(0);
                vendingMachineReadModel.ChangeCoins.FirstOrDefault(x => x.Coin == Coin.Cent50.ToString()).Quantity
                    .Should().Be(1);
                vendingMachineReadModel.ChangeCoins.FirstOrDefault(x => x.Coin == Coin.Cent10.ToString()).Quantity
                    .Should().Be(0);
                vendingMachineReadModel.ChangeCoins.FirstOrDefault(x => x.Coin == Coin.OneEuro.ToString()).Quantity
                    .Should().Be(1);

                //Verify InsertedCoins 

                vendingMachineReadModel.InsertedCoins.FirstOrDefault(x => x.Coin == Coin.Cent20.ToString()).Quantity
                    .Should().Be(0);
                vendingMachineReadModel.InsertedCoins.FirstOrDefault(x => x.Coin == Coin.Cent50.ToString()).Quantity
                    .Should().Be(0);
                vendingMachineReadModel.InsertedCoins.FirstOrDefault(x => x.Coin == Coin.Cent10.ToString()).Quantity
                    .Should().Be(0);
                vendingMachineReadModel.InsertedCoins.FirstOrDefault(x => x.Coin == Coin.OneEuro.ToString()).Quantity
                    .Should().Be(0);
            }
        }
    }
}