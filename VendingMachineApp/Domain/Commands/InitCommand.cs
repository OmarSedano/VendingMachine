using EventFlow.Commands;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using EventFlow.Aggregates.ExecutionResults;

namespace VendingMachineApp.Domain.Commands
{
    public class InitCommand : Command<VendingMachineAggregate, VendingMachineId>
    {
        public InitCommand(VendingMachineId aggregateId) : base(aggregateId)
        {
        }

        public class InitCommandHandler : CommandHandler<VendingMachineAggregate, VendingMachineId, IExecutionResult, InitCommand>
        {
            public override Task<IExecutionResult> ExecuteCommandAsync(VendingMachineAggregate aggregate, InitCommand command, CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                return Task.FromResult(aggregate.Init());
            }
        }
    }
}
