using System;
using System.Threading;
using System.Threading.Tasks;
using EventFlow.Queries;
using VendingMachineApp.ReadModel.Models;
using VendingMachineApp.ReadModel.Queries;

namespace VendingMachineApp.ReadModel.QueryHandlers
{
    public class VendingMachineByIdQueryHandler : IQueryHandler<VendingMachineByIdQuery, VendingMachineReadModel>
    {
        private readonly IQueryProcessor _queryProcessor;

        public VendingMachineByIdQueryHandler(IQueryProcessor queryProcessor)
        {
            _queryProcessor = queryProcessor;
        }

        public async Task<VendingMachineReadModel> ExecuteQueryAsync(VendingMachineByIdQuery query, CancellationToken cancellationToken)
        {
            return await _queryProcessor.ProcessAsync(new ReadModelByIdQuery<VendingMachineReadModel>(query.VendingMachineId), CancellationToken.None);
        }
    }
}
