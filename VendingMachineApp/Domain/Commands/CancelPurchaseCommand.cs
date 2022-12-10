using EventFlow.Commands;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using EventFlow.Aggregates.ExecutionResults;

namespace VendingMachineApp.Domain.Commands
{
    public class CancelPurchaseCommand : Command<VendingMachineAggregate, VendingMachineId>
    {
        public CancelPurchaseCommand(VendingMachineId aggregateId) : base(aggregateId)
        {
        }

        public class CancelPurchaseCommandHandler : CommandHandler<VendingMachineAggregate, VendingMachineId, IExecutionResult, CancelPurchaseCommand>
        {
            public override Task<IExecutionResult> ExecuteCommandAsync(VendingMachineAggregate aggregate, CancelPurchaseCommand command, CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                return Task.FromResult(aggregate.CancelPurchase());
            }
        }
    }
}
