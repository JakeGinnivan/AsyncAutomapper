namespace Automapper.Tests.TestClasses.Domain
{
    public class Trade
    {
        public CurrencyPair CurrencyPair { get; set; }
        public City City { get; set; }
        public decimal Notional { get; set; }
    }
}