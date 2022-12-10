
using VendingMachineApp.Domain.Entities;

namespace VendingMachineApp.ReadModel.Models.Mappers
{
    public static class ReadModelMapper
    {
        public static ProductModel Map(Product product)
        {
            return new ProductModel()
            {
                Id = product.Id,
                Name = product.Name,
                Quantity = product.Quantity,
                Price = product.Price
            };
        }

        public static CoinsInfoModel Map(CoinsInfo coinsInfo)
        {
            return new CoinsInfoModel()
            {
                Coin = coinsInfo.Coin.ToString(),
                Quantity = coinsInfo.Quantity
            };
        }
    }
}
