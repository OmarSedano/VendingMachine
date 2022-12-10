using EventFlow.Commands;
using System.Threading;
using System.Threading.Tasks;
using EventFlow.Aggregates.ExecutionResults;
using VendingMachineApp.Domain.Entities;

namespace VendingMachineApp.Domain.Commands
{
    public class InsertCoinCommand : Command<VendingMachineAggregate, VendingMachineId>
    {
        public InsertCoinCommand(VendingMachineId aggregateId, Coin coin) : base(aggregateId)
        {
            Coin = coin;
        }

        public Coin Coin { get; }

        public class InsertCoinCommandHandler : CommandHandler<VendingMachineAggregate, VendingMachineId, IExecutionResult, InsertCoinCommand>
        {
            public override Task<IExecutionResult> ExecuteCommandAsync(VendingMachineAggregate aggregate, InsertCoinCommand command, CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                return Task.FromResult(aggregate.InsertCoin(command.Coin));
            }
        }
    }
}
