namespace VendingMachineApp.Domain.Entities
{
    public class CoinsInfo
    {
        public CoinsInfo(Coin coin, int quantity)
        {
            Coin = coin;
            Quantity = quantity;
        }

        public Coin Coin { get; set; }
        public int Quantity { get; set; }
    }
}
