using System;
using System.Threading.Tasks;
using AutoMapper;

namespace Automapper
{
    public static class AsyncExtensions
    {
        public static Task<TTo> AsyncMap<TTo>(this IMappingEngine mapper, object source)
        {
            var asyncMapContext = new AsyncContext();
            try
            {
                var result = mapper.Map<TTo>(source, o => o.Items.Add("AsyncContext", asyncMapContext));

                return asyncMapContext.MappingTask.ContinueWith(t =>
                {
                    t.Wait();
                    return result;
                });
            }
            finally
            {
                asyncMapContext.CompleteIfNoCalls();
            }
        }

        public static void CreateAsyncMap<TSource, TDestination>(this ConfigurationStore configurationStore, Func<TSource, Task<TDestination>> getValueAsync)
        {
            configurationStore.CreateAsyncMap(new InlineAsyncTypeConverter<TSource, TDestination>(getValueAsync));
        }

        /// <summary>
        /// Useful when server returns an intermediate DTO which needs to be mapped to the destination once returned by server
        /// Mappings will be registered for:
        /// source => intermediate
        /// intermediate => destination
        /// 
        /// Only source => intermediate is asynchronous
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TIntermediate">Intermediate type returned from server</typeparam>
        /// <typeparam name="TDestination">The end destination</typeparam>
        public static IMappingExpression<TIntermediate, TDestination> CreateAsyncMap<TSource, TIntermediate, TDestination>(this ConfigurationStore configurationStore, Func<TSource, Task<TIntermediate>> getValueAsync)
        {
            configurationStore.CreateAsyncMap(new InlineAsyncTypeConverter<TSource, TDestination>((c, v) => getValueAsync(v).ContinueWith(t => c.Engine.Map<TDestination>(t.Result,
                m =>
                {
                    m.Items.Add("AsyncContext", c.Options.Items["AsyncContext"]);
                }))));
            return configurationStore.CreateMap<TIntermediate, TDestination>();
        }

        public static void CreateAsyncMap<TSource, TDestination>(this ConfigurationStore configurationStore, AsyncTypeConverter<TSource, TDestination> converter)
        {
            configurationStore
                .CreateMap<TSource, TDestination>()
                .ConvertUsing(converter);
        }

        internal class AsyncContext
        {
            readonly TaskCompletionSource<object> taskSource = new TaskCompletionSource<object>();
            readonly object locker = new object();
            int activeCalls;

            public void StartAsyncOperation()
            {
                lock (locker) { activeCalls++; };
            }

            public void OperationFinished()
            {
                var isComplete = false;
                lock (locker)
                {
                    activeCalls--;

                    if (activeCalls == 0)
                    {
                        isComplete = true;
                    }
                };

                if (isComplete)
                    taskSource.TrySetResult(null);
            }

            public Task MappingTask
            {
                get { return taskSource.Task; }
            }

            public void Error(Exception exception)
            {
                taskSource.TrySetException(exception);
            }

            public void CompleteIfNoCalls()
            {
                lock (locker)
                {
                    if (activeCalls == 0)
                    {
                        taskSource.TrySetResult(null);
                    }
                };
            }
        }
    }
}