using System;
using EventFlow.Queries;
using EventFlow.ReadStores.InMemory.Queries;
using VendingMachineApp.ReadModel.Models;

namespace VendingMachineApp.ReadModel.Queries
{
    public class VendingMachineByIdQuery : IQuery<VendingMachineReadModel>
    {
        public string VendingMachineId { get; }

        public VendingMachineByIdQuery(string vendingMachineId)
        {
            if (string.IsNullOrEmpty(vendingMachineId))
            {
                throw new ArgumentNullException("VendingMachineId is null");
            }

            VendingMachineId = vendingMachineId;
        }
    }
}
