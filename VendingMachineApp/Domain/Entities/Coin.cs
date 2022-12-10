namespace VendingMachineApp.Domain.Entities
{
    public class Coin
    {
        public static readonly Coin Cent10 = new Coin("Cent10", 0.10M);
        public static readonly Coin Cent20 = new Coin("Cent20", 0.20M);
        public static readonly Coin Cent50 = new Coin("Cent50", 0.50M);
        public static readonly Coin OneEuro = new Coin("OneEuro", 1.0M);

        public string name { get; set; }
        public decimal value { get; set; }

        public Coin()
        {

        }

        public Coin(string name, decimal value)
        {
            this.name = name;
            this.value = value;
        }

        public override string ToString()
        {
            return this.name;
        }
    };
}
