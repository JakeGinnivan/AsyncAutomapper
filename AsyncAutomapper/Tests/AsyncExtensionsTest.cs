using System;
using System.Threading.Tasks;
using Automapper.Tests.TestClasses;
using Automapper.Tests.TestClasses.Domain;
using Automapper.Tests.TestClasses.Server;
using AutoMapper;
using AutoMapper.Mappers;
using Shouldly;
using Xunit;

namespace Automapper.Tests
{
    public class AsyncExtensionsTest
    {
        [Fact]
        public void Verify()
        {
            var configurationStore = new ConfigurationStore(new TypeMapFactory(), MapperRegistry.Mappers);
            var mapper = new MappingEngine(configurationStore);

            var server = new ServerApi();

            configurationStore
                .CreateMap<TradeDto, Trade>()
                .ForMember(d => d.CurrencyPair, m => m.MapFrom(s => s.CcyPair));

            configurationStore.CreateAsyncMap<string, CurrencyDto, Currency>(s => server.GetCurrency(s));
            configurationStore.CreateAsyncMap<string, CurrencyPairDto, CurrencyPair>(s => server.GetCurrencyPair(s));
            configurationStore.CreateAsyncMap<string, CityDto, City>(s => server.GetCity(s));

            configurationStore.AssertConfigurationIsValid();

            var trade = new TradeDto { CcyPair = "EURUSD", City = "London", Notional = 1000000 };
            var mapped = mapper.AsyncMap<Trade>(trade).Result;
            mapped.CurrencyPair.Symbol.ShouldBe("EURUSD");
            mapped.CurrencyPair.Base.IsoCode.ShouldBe("EUR");
            mapped.CurrencyPair.Counter.IsoCode.ShouldBe("USD");
            mapped.City.Name.ShouldBe("London");
            mapped.Notional.ShouldBe(1000000);
        }

        [Fact]
        public void PropogatesServerErrorsProperly()
        {
            var configurationStore = new ConfigurationStore(new TypeMapFactory(), MapperRegistry.Mappers);
            var mapper = new MappingEngine(configurationStore);

            configurationStore.CreateAsyncMap<string, CityDto, City>(s => Task.Run(new Func<CityDto>(() => { throw new Exception("Boom");})));

            Should.Throw<Exception>(mapper.AsyncMap<City>("London")).Message.ShouldBe("Boom");
        }

        [Fact]
        public void PropogatesAutomapperErrorsProperly()
        {
            var configurationStore = new ConfigurationStore(new TypeMapFactory(), MapperRegistry.Mappers);
            var mapper = new MappingEngine(configurationStore);

            configurationStore.CreateMap<TradeDto, Trade>()
                .ForMember(d => d.CurrencyPair, m => m.MapFrom(s => s.CcyPair));
            var trade = new TradeDto { CcyPair = "EURUSD" };
            Should.Throw<AutoMapperMappingException>(() => { mapper.AsyncMap<Trade>(trade); });
        }
    }
}