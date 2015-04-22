using System.Threading;
using System.Threading.Tasks;

namespace Automapper.Tests.TestClasses.Server
{
    public class ServerApi
    {
        public Task<CurrencyDto> GetCurrency(string code)
        {
            return Task.Run(() =>
            {
                Thread.Sleep(200);
                return new CurrencyDto { IsoCode = code };
            });
        }
        public Task<CurrencyPairDto> GetCurrencyPair(string symbol)
        {
            return Task.Run(() =>
            {
                Thread.Sleep(200);
                return new CurrencyPairDto { Symbol = symbol, Base = symbol.Substring(0, 3), Counter = symbol.Substring(3) };
            });
        }
        public Task<CityDto> GetCity(string key)
        {
            return Task.Run(() =>
            {
                Thread.Sleep(200);
                return new CityDto { Key = key, Name = key };
            });
        }
    }
}