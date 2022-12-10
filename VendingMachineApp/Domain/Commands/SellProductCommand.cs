using EventFlow.Commands;
using System.Threading;
using System.Threading.Tasks;
using EventFlow.Aggregates.ExecutionResults;

namespace VendingMachineApp.Domain.Commands
{
    public class SellProductCommand : Command<VendingMachineAggregate, VendingMachineId>
    {
        public SellProductCommand(VendingMachineId aggregateId, int productId) : base(aggregateId)
        {
            ProductId = productId;
        }

        public int ProductId { get; }

        public class SellProductCommandHandler : CommandHandler<VendingMachineAggregate, VendingMachineId, IExecutionResult, SellProductCommand>
        {
            public override Task<IExecutionResult> ExecuteCommandAsync(VendingMachineAggregate aggregate, SellProductCommand command, CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                return Task.FromResult(aggregate.SellProduct(command.ProductId));
            }
        }
    }
}
