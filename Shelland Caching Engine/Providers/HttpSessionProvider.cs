using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

using Shelland.CachingEngine.Logic;

namespace Shelland.CachingEngine.Providers
{
    public class HttpSessionProvider : BaseProvider
    {
        private static Func<HttpSessionStateBase> _sessionBaseFactory;
        private static readonly Mutex _mutex = new Mutex();

        static HttpSessionProvider()
        {
            SetSessionFactory(() => new HttpSessionStateWrapper(HttpContext.Current.Session));
        }

        public override T Fetch<T>(string key, string group = null, Func<string, string, T> factory = null)
        {
            var resolved = ResolveKey(key, group);
            var session = GetSession();
            var item = session[resolved] as CachingItem<T>;

            if (item != null)
            {
                item.LastAccessed = DateTime.UtcNow;
                session[resolved] = item;

                return item.Value;
            }

            if (factory != null)
            {
                T value = factory(key, group);
                session[resolved] = new CachingItem<T>(key, value, group);

                return value;
            }

            return default(T);
        }

        public override IEnumerable<T> FetchAll<T>(string group = null, Func<string, T, int, string> keyFactory = null,
            Func<string, IEnumerable<T>> itemFactory = null)
        {
            var session = GetSession();

            var items = session
                .OfType<string>()
                .Select(k => session[k])
                .OfType<CachingItem<T>>()
                .Where(t => t.Group == group)
                .ToList();

            if (items.Any())
            {
                DateTime lastAccessed = DateTime.UtcNow;
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
                    var key = keyFactory(group, value, index);
                    var resolved = ResolveKey(key, group);

                    session[resolved] = new CachingItem<T>(key, value, group);
                    index++;
                }

                return values;
            }

            return Enumerable.Empty<T>();
        }

        private static HttpSessionStateBase GetSession()
        {
            _mutex.WaitOne();

            var session = _sessionBaseFactory();
            if (session == null)
                throw new InvalidOperationException("Session factory returned null unexpectedly.");

            _mutex.ReleaseMutex();

            return session;
        }

        public override void Remove(string key, string group = null)
        {
            var resolved = ResolveKey(key, group);
            var session = GetSession();

            session.Remove(resolved);
        }

        public override void RemoveAll<T>(string group = null)
        {
            var session = GetSession();

            var items = session
                .OfType<string>()
                .Select(k => session[k])
                .OfType<CachingItem<T>>()
                .Where(i => i.Group == group)
                .ToList();

            foreach (var item in items)
            {
                string resolved = ResolveKey(item.Key, group);
                session.Remove(resolved);
            }
        }

        public static void SetSessionFactory(Func<HttpSessionStateBase> sessionBaseFactory)
        {
            _mutex.WaitOne();

            _sessionBaseFactory = sessionBaseFactory;

            _mutex.ReleaseMutex();
        }

        public override void Store<T>(string key, T value, string group = null)
        {
            var resolved = ResolveKey(key, group);
            var session = GetSession();

            session[resolved] = new CachingItem<T>(key, value, group);
        }

        public override void StoreAll<T>(Func<string, T, int, string> keyFactory, IEnumerable<T> values,
            string group = null)
        {
            var session = GetSession();

            var index = 0;
            foreach (var value in values)
            {
                var key = keyFactory(group, value, index);
                var resolved = ResolveKey(key, group);

                session[resolved] = new CachingItem<T>(key, value, group);

                index++;
            }
        }
    }
}