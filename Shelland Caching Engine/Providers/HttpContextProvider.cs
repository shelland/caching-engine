using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

using Shelland.CachingEngine.Logic;

namespace Shelland.CachingEngine.Providers
{
    public class HttpContextProvider : BaseProvider
    {
        private static Func<HttpContextBase> _contextBaseFactory;
        private static readonly Mutex _mutex = new Mutex();

        static HttpContextProvider()
        {
            SetContextFactory(() => new HttpContextWrapper(HttpContext.Current));
        }

        public override T Fetch<T>(string key, string group = null, Func<string, string, T> factory = null)
        {
            var resolved = ResolveKey(key, group);
            var context = GetContext();
            var item = context.Items[resolved] as CachingItem<T>;

            if (item != null)
            {
                item.LastAccessed = DateTime.UtcNow;

                return item.Value;
            }

            if (factory != null)
            {
                var value = factory(key, group);
                item = new CachingItem<T>(key, value, group);

                context.Items[resolved] = item;

                return value;
            }

            return default(T);
        }

        public override IEnumerable<T> FetchAll<T>(string group = null, Func<string, T, int, string> keyFactory = null,
            Func<string, IEnumerable<T>> itemFactory = null)
        {
            var context = GetContext();

            var items = context.Items
                .Values
                .OfType<CachingItem<T>>()
                .Where(i => i.Group == group)
                .ToList();

            if (items.Any())
            {
                var lastAccessed = DateTime.UtcNow;
                items.ForEach(i => i.LastAccessed = lastAccessed);

                return items.Select(i => i.Value);
            }

            if (itemFactory != null)
            {
                if (keyFactory == null)
                    throw new ArgumentException("A key factory must be provided if an item factory is set.");

                var values = itemFactory(group);
                var index = 0;

                foreach (var value in values)
                {
                    string key = keyFactory(group, value, index);
                    string resolved = ResolveKey(key, group);

                    context.Items[resolved] = new CachingItem<T>(key, value, group);
                    index++;
                }

                return values;
            }

            return Enumerable.Empty<T>();
        }

        private static HttpContextBase GetContext()
        {
            _mutex.WaitOne();

            var context = _contextBaseFactory();
            if (context == null)
                throw new InvalidOperationException("Context factory returned null unexpectedly.");

            _mutex.ReleaseMutex();

            return context;
        }

        public override void Remove(string key, string group = null)
        {
            var resolved = ResolveKey(key, group);
            var context = GetContext();

            context.Items.Remove(resolved);
        }

        public override void RemoveAll<T>(string group = null)
        {
            var context = GetContext();

            var items = context.Items
                .Values
                .OfType<CachingItem<T>>()
                .Where(i => i.Group == group)
                .ToList();

            foreach (var resolved in items.Select(item => ResolveKey(item.Key, @group)))
            {
                context.Items.Remove(resolved);
            }
        }

        public static void SetContextFactory(Func<HttpContextBase> contextBaseFactory)
        {
            _mutex.WaitOne();

            _contextBaseFactory = contextBaseFactory;

            _mutex.ReleaseMutex();
        }

        public override void Store<T>(string key, T value, string group = null)
        {
            var resolved = ResolveKey(key, group);
            var context = GetContext();
            var item = new CachingItem<T>(key, value, group);

            context.Items[resolved] = item;
        }

        public override void StoreAll<T>(Func<string, T, int, string> keyFactory, IEnumerable<T> values,
            string group = null)
        {
            var context = GetContext();
            var index = 0;

            foreach (var value in values)
            {
                var key = keyFactory(group, value, index);
                var resolved = ResolveKey(key, group);

                var item = new CachingItem<T>(key, value, group);

                context.Items[resolved] = item;
                index++;
            }
        }
    }
}