using System;
using System.Threading.Tasks;
using AutoMapper;

namespace Automapper
{
    public abstract class AsyncTypeConverter<TSource, TDestination> : ITypeConverter<TSource, TDestination>
    {
        public TDestination Convert(ResolutionContext context)
        {
            var contextItems = context.Options.Items;
            if (!contextItems.ContainsKey("AsyncContext"))
                throw new InvalidOperationException("You must use mapper.AsyncMap when using async value resolvers");

            var asyncContext = (AsyncExtensions.AsyncContext)contextItems["AsyncContext"];

            asyncContext.StartAsyncOperation();
            var sourceValue = context.SourceValue;

            GetValue(context, (TSource)sourceValue)
                .ContinueWith(r =>
                {
                    try
                    {
                        var destination = r.Result;
                        var desintation = context.PropertyMap.DestinationProperty;
                        var parent = context.Parent.DestinationValue;
                        desintation.SetValue(parent, destination);
                        asyncContext.OperationFinished();
                    }
                    catch(Exception ex)
                    {
                        asyncContext.Error(ex);
                    }
                });

            return default(TDestination);
        }

        protected abstract Task<TDestination> GetValue(ResolutionContext context, TSource fromValue);
    }
}