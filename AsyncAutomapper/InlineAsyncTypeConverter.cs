using System;
using System.Threading.Tasks;
using AutoMapper;

namespace Automapper
{
    class InlineAsyncTypeConverter<TSource, TDestination> : AsyncTypeConverter<TSource, TDestination>
    {
        private readonly Func<ResolutionContext, TSource, Task<TDestination>> getValue;

        public InlineAsyncTypeConverter(Func<TSource, Task<TDestination>> getValue) : this((context, val) => getValue(val))
        {
        }

        public InlineAsyncTypeConverter(Func<ResolutionContext, TSource, Task<TDestination>> getValue)
        {
            this.getValue = getValue;
        }

        protected override Task<TDestination> GetValue(ResolutionContext context, TSource fromValue)
        {
            return getValue(context, fromValue);
        }
    }
}