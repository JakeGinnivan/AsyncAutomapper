namespace Automapper.Tests.TestClasses.Domain
{
    public class CurrencyPair
    {
        public string Symbol { get; set; }
        public Currency Base { get; set; }
        public Currency Counter { get; set; }
    }
}