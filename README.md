# AsyncAutomapper
AsyncAutomapper is a code only package to allow you to use Automapper with asynchronous maps.

The scenario is you may have some reference data which can be cached on the client, but may not be there yet or there may be a cache miss. 

## Example
Because I used beer example in WebApi.Hal, I will do the same here. Say we have an http API but the data can be cached quite well so the structure is flat.
```
public class BeerDto
{
    public string Name { get; set; }
    public string BreweryName { get; set; }
}
public class BreweryDto
{
    public string Name { get; set; }
}

public class Beer
{
    public string Name { get; set; }
    public Brewery Brewery { get; set; }
}
public class Brewery
{
    public string Name { get; set; }
}
```

The mapping and usage now looks like this:
```
var configurationStore = new ConfigurationStore(new TypeMapFactory(), MapperRegistry.Mappers);
var mapper = new MappingEngine(configurationStore);

configurationStore.CreateAsyncMap<string, BeerDto, Beer>(s => api.GetBeerByName(s));
configurationStore.CreateAsyncMap<string, BreweryDto, Brewery>(s => api.GetBreweryByName(s));

var beer = mapper.AsyncMap<Beer>("Punk IPA");
```

`.AsyncMap` will first map the string to the intermediate dto, then map it to the target type. When it tries to map to the 
target type it hits a string -> Brewery mapping, which will then call your api then map the result to the target type.


