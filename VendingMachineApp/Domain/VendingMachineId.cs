using EventFlow.Core;

namespace VendingMachineApp.Domain
{
    public class VendingMachineId : Identity<VendingMachineId>
    {
        public VendingMachineId(string value) : base(value)
        {
        }
    }

}
