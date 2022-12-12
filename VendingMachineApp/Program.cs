using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using EventFlow;
using EventFlow.Aggregates.ExecutionResults;
using EventFlow.Configuration;
using EventFlow.Extensions;
using EventFlow.Queries;
using VendingMachineApp.Domain;
using VendingMachineApp.Domain.Events;
using VendingMachineApp.Domain.Commands;
using VendingMachineApp.Domain.Entities;
using VendingMachineApp.ReadModel.QueryHandlers;
using static VendingMachineApp.Domain.Commands.InitCommand;
using VendingMachineApp.ReadModel.Models;
using VendingMachineApp.ReadModel.Queries;

namespace VendingMachineApp
{
    internal class Program
    {
        private static ICommandBus _commandBus;
        private static IQueryProcessor _queryProcessor;
        private static VendingMachineId _vendingMachineId;
        private static Regex sellProductRegex = new Regex(@"(sell)\s([0-9]*)");
        private static Regex insertCoinRegex = new Regex(@"(insert)\s(Cent10|Cent20|Cent30|Cent50|OneEuro)");

        static async Task Main(string[] args)
        {
            try
            {
                using var rootResolver = EventFlowOptions.New
                           .AddEvents(typeof(CoinInsertedEvent), typeof(PurchaseCanceledEvent), typeof(ProductSoldEvent),
                               typeof(InitializedEvent))
                           .AddCommands(typeof(InsertCoinCommand), typeof(CancelPurchaseCommand),
                               typeof(SellProductCommand), typeof(InitCommand))
                           .AddCommandHandlers(typeof(InsertCoinCommand.InsertCoinCommandHandler),
                               typeof(CancelPurchaseCommand.CancelPurchaseCommandHandler),
                               typeof(SellProductCommand.SellProductCommandHandler), typeof(InitCommandHandler))
                           .AddQueryHandlers(typeof(VendingMachineByIdQueryHandler))
                           .UseInMemoryReadStoreFor<VendingMachineReadModel>()
                           .UseNullLog()
                           .CreateResolver();

                _commandBus = rootResolver.Resolve<ICommandBus>();
                _queryProcessor = rootResolver.Resolve<IQueryProcessor>();

                Console.WriteLine("Welcome to the Vending Machine");

                await InitVendingMachineAsync();

                Console.WriteLine("Type one of the following commands please:");
                Console.WriteLine("- products => Shows production information ");
                Console.WriteLine("- insert <coin> => Insert coin. Ex: insert OneEuro. Allowed values in <coin>: Cent10, Cent20, Cent50, OneEuro");
                Console.WriteLine("- sell <productId>  => Sell product with Id. Ex: sell 1");
                Console.WriteLine("- cancel => Cancel purchase. Return inserted coins");
                Console.WriteLine();

                while (true)
                {
                    string userInput = Console.ReadLine();
                    if (userInput.Equals("products"))
                    {
                        await ShowProductsAsync();
                    }
                    else if (insertCoinRegex.IsMatch(userInput))
                    {
                        await InsertCoinAsync(userInput);
                    }
                    else if (sellProductRegex.IsMatch(userInput))
                    {
                        await SellProductAsync(userInput);
                    }
                    else if (userInput.Equals("cancel"))
                    {
                        await CancelPurchaseAsync();
                    }
                    else
                    {
                        Console.WriteLine($"Invalid command: {userInput}");
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error. Message : {ex.Message}");
            }
        }

        private static async Task InitVendingMachineAsync()
        {
            _vendingMachineId = VendingMachineId.New;

            var initResult = await _commandBus.PublishAsync(
                    new InitCommand(_vendingMachineId),
                    CancellationToken.None)
                .ConfigureAwait(false);

            if (!initResult.IsSuccess)
            {
                throw new Exception((string.Join(",", ((FailedExecutionResult)initResult).Errors)));
            }

            Console.WriteLine("Vending Machine On! Good morning!");
        }

        private static async Task CancelPurchaseAsync()
        {
            var cancelResult = await _commandBus.PublishAsync(
                    new CancelPurchaseCommand(_vendingMachineId),
                    CancellationToken.None)
                .ConfigureAwait(false);

            if (!cancelResult.IsSuccess)
            {
                Console.WriteLine((string.Join(",", ((FailedExecutionResult)cancelResult).Errors)));
                return;
            }

            Console.WriteLine("Purchase canceled");

            var vendingMachineReadModel = await _queryProcessor.ProcessAsync(
                    new VendingMachineByIdQuery(_vendingMachineId.Value),
                    CancellationToken.None)
                .ConfigureAwait(false);

            Console.WriteLine($"Here are your inserted coins.");
            foreach (var changeCoins in vendingMachineReadModel.ChangeCoins.Where(x => x.Quantity > 0).ToList())
            {
                Console.WriteLine($"{changeCoins.Quantity} coin/s of {changeCoins.Coin}");
            }
            Console.WriteLine($"Have a nice day!");
        }

        private static async Task SellProductAsync(string userInput)
        {
            var match = sellProductRegex.Match(userInput);
            var productId = int.Parse(match.Groups[2].Value);

            var sellResult = await _commandBus.PublishAsync(
                    new SellProductCommand(_vendingMachineId, productId),
                    CancellationToken.None)
                .ConfigureAwait(false);

            if (!sellResult.IsSuccess)
            {
                Console.WriteLine((string.Join(",", ((FailedExecutionResult)sellResult).Errors)));
                return;
            }

            Console.WriteLine("Product sold");

            var vendingMachineReadModel = await _queryProcessor.ProcessAsync(
                    new VendingMachineByIdQuery(_vendingMachineId.Value),
                    CancellationToken.None)
                .ConfigureAwait(false);

            Console.WriteLine($"Thank you. Here is your change:");
            foreach (var changeCoins in vendingMachineReadModel.ChangeCoins.Where(x => x.Quantity > 0).ToList())
            {
                Console.WriteLine($"{changeCoins.Quantity} coin/s of {changeCoins.Coin}");
            }
            Console.WriteLine($"Have a nice day!");
        }

        private static async Task InsertCoinAsync(string userInput)
        {
            var match = insertCoinRegex.Match(userInput);
            var coinName = match.Groups[2].Value;
            var coin = GetCoin(coinName);

            var insertCoinResult = await _commandBus.PublishAsync(
                    new InsertCoinCommand(_vendingMachineId, coin),
                    CancellationToken.None)
                .ConfigureAwait(false);

            if (!insertCoinResult.IsSuccess)
            {
                throw new Exception((string.Join(",", ((FailedExecutionResult)insertCoinResult).Errors)));
            }

            Console.WriteLine($"Inserted a {coin} coin in the vending machine");
        }

        private static Coin GetCoin(string coinName)
        {
            Coin coin;
            switch (coinName)
            {
                case "Cent10":
                    coin = Coin.Cent10;
                    break;
                case "Cent20":
                    coin = Coin.Cent20;
                    break;
                case "Cent50":
                    coin = Coin.Cent50;
                    break;
                case "OneEuro":
                    coin = Coin.OneEuro;
                    break;
                default:
                    throw new ArgumentException("CoinName not valid");
            }

            return coin;
        }

        private static async Task ShowProductsAsync()
        {
            var vendingMachineReadModel = await _queryProcessor.ProcessAsync(
                    new VendingMachineByIdQuery(_vendingMachineId.Value),
                    CancellationToken.None)
                .ConfigureAwait(false);

            Console.WriteLine("These are our products: ");

            foreach (var product in vendingMachineReadModel.Products)
            {
                Console.WriteLine($"Id: {product.Id}     Name: {product.Name}   Price: {product.Price} eur      Quantity: {product.Quantity} portions");
            }
        }
    }
}
