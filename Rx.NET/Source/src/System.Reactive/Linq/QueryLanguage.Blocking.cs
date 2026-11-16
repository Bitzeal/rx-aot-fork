// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT License.
// See the LICENSE file in the project root for more information. 

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace System.Reactive.Linq
{

    using ObservableImpl;

    internal partial class QueryLanguage
    {
        #region - Chunkify -

        public IEnumerable<IList<TSource>> Chunkify<TSource>(IObservable<TSource> source)
        {
            return source.Collect<TSource, IList<TSource>>(() => [], (lst, x) => { lst.Add(x); return lst; }, _ => []);
        }

        #endregion

        #region + Collect +

        public IEnumerable<TResult> Collect<TSource, TResult>(IObservable<TSource> source, Func<TResult> newCollector, Func<TResult, TSource, TResult> merge)
        {
            return Collect_(source, newCollector, merge, _ => newCollector());
        }

        public IEnumerable<TResult> Collect<TSource, TResult>(IObservable<TSource> source, Func<TResult> getInitialCollector, Func<TResult, TSource, TResult> merge, Func<TResult, TResult> getNewCollector)
        {
            return Collect_(source, getInitialCollector, merge, getNewCollector);
        }

        private static IEnumerable<TResult> Collect_<TSource, TResult>(IObservable<TSource> source, Func<TResult> getInitialCollector, Func<TResult, TSource, TResult> merge, Func<TResult, TResult> getNewCollector)
        {
            return new Collect<TSource, TResult>(source, getInitialCollector, merge, getNewCollector);
        }

        #endregion

        #region First

        public TSource First<TSource>(IObservable<TSource> source)
        {
            return FirstOrDefaultInternal(source, throwOnEmpty: true)!;
        }

        public TSource First<TSource>(IObservable<TSource> source, Func<TSource, bool> predicate)
        {
            return First(Where(source, predicate));
        }

        #endregion

        #region FirstOrDefault

        [return: MaybeNull]
        public TSource FirstOrDefault<TSource>(IObservable<TSource> source)
        {
            return FirstOrDefaultInternal(source, throwOnEmpty: false);
        }

        [return: MaybeNull]
        public TSource FirstOrDefault<TSource>(IObservable<TSource> source, Func<TSource, bool> predicate)
        {
            return FirstOrDefault(Where(source, predicate));
        }

        [return: MaybeNull]
        private static TSource FirstOrDefaultInternal<TSource>(IObservable<TSource> source, bool throwOnEmpty)
        {
            using var consumer = new FirstBlocking<TSource>();

            using (source.Subscribe(consumer))
            {
                consumer.Wait();
            }

            consumer._error?.Throw();

            if (throwOnEmpty && !consumer._hasValue)
            {
                throw new InvalidOperationException(Strings_Linq.NO_ELEMENTS);
            }

            return consumer._value;
        }

        #endregion

        #region + ForEach +

        public void ForEach<TSource>(IObservable<TSource> source, Action<TSource> onNext)
        {
            using var sink = new ForEach<TSource>.Observer(onNext);

            using (source.SubscribeSafe(sink))
            {
                sink.Wait();
            }

            sink.Error?.Throw();
        }

        public void ForEach<TSource>(IObservable<TSource> source, Action<TSource, int> onNext)
        {
            using var sink = new ForEach<TSource>.ObserverIndexed(onNext);

            using (source.SubscribeSafe(sink))
            {
                sink.Wait();
            }

            sink.Error?.Throw();
        }

        #endregion

        #region + GetEnumerator +

        public IEnumerator<TSource> GetEnumerator<TSource>(IObservable<TSource> source)
        {
            var e = new GetEnumerator<TSource>();
            return e.Run(source);
        }

        #endregion

        #region Last

        public TSource Last<TSource>(IObservable<TSource> source)
        {
            return LastOrDefaultInternal(source, throwOnEmpty: true)!;
        }

        public TSource Last<TSource>(IObservable<TSource> source, Func<TSource, bool> predicate)
        {
            return Last(Where(source, predicate));
        }

        #endregion

        #region LastOrDefault

        [return: MaybeNull]
        public TSource LastOrDefault<TSource>(IObservable<TSource> source)
        {
            return LastOrDefaultInternal(source, throwOnEmpty: false);
        }

        [return: MaybeNull]
        public TSource LastOrDefault<TSource>(IObservable<TSource> source, Func<TSource, bool> predicate)
        {
            return LastOrDefault(Where(source, predicate));
        }

        [return: MaybeNull]
        private static TSource LastOrDefaultInternal<TSource>(IObservable<TSource> source, bool throwOnEmpty)
        {
            using var consumer = new LastBlocking<TSource>();

            using (source.Subscribe(consumer))
            {
                consumer.Wait();
            }

            consumer._error?.Throw();

            if (throwOnEmpty && !consumer._hasValue)
            {
                throw new InvalidOperationException(Strings_Linq.NO_ELEMENTS);
            }

            return consumer._value;
        }

        #endregion

        #region + Latest +

        public IEnumerable<TSource> Latest<TSource>(IObservable<TSource> source)
        {
            return new Latest<TSource>(source);
        }

        #endregion

        #region + MostRecent +

        public IEnumerable<TSource> MostRecent<TSource>(IObservable<TSource> source, TSource initialValue)
        {
            return new MostRecent<TSource>(source, initialValue);
        }

        #endregion

        #region + Next +

        public IEnumerable<TSource> Next<TSource>(IObservable<TSource> source)
        {
            return new Next<TSource>(source);
        }

        #endregion

        #region Single

        public TSource Single<TSource>(IObservable<TSource> source)
        {
            return SingleOrDefaultInternal(source, throwOnEmpty: true)!;
        }

        public TSource Single<TSource>(IObservable<TSource> source, Func<TSource, bool> predicate)
        {
            return Single(Where(source, predicate));
        }

        #endregion

        #region SingleOrDefault

        [return: MaybeNull]
        public TSource SingleOrDefault<TSource>(IObservable<TSource> source)
        {
            return SingleOrDefaultInternal(source, throwOnEmpty: false);
        }

        [return: MaybeNull]
        public TSource SingleOrDefault<TSource>(IObservable<TSource> source, Func<TSource, bool> predicate)
        {
            return SingleOrDefault(Where(source, predicate));
        }

        [return: MaybeNull]
        private static TSource SingleOrDefaultInternal<TSource>(IObservable<TSource> source, bool throwOnEmpty)
        {
            using var consumer = new SingleBlocking<TSource>();

            using (source.Subscribe(consumer))
            {
                consumer.Wait();
            }

            consumer._error?.Throw();

            if (consumer._hasMoreThanOneElement)
            {
                throw new InvalidOperationException(Strings_Linq.MORE_THAN_ONE_ELEMENT);
            }

            if (throwOnEmpty && !consumer._hasValue)
            {
                throw new InvalidOperationException(Strings_Linq.NO_ELEMENTS);
            }

            return consumer._value;
        }

        #endregion

        #region Wait

        public TSource Wait<TSource>(IObservable<TSource> source)
        {
            return LastOrDefaultInternal(source, true)!;
        }

        #endregion
    }
}
