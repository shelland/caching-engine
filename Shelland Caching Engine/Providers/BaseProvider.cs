using System;
using System.Collections.Generic;
using Shelland.CachingEngine.Logic;

namespace Shelland.CachingEngine.Providers
{
    public abstract class BaseProvider : ICache
    {
        #region Methods

        public abstract T Fetch<T>(string key, string group = null, Func<string, string, T> factory = null);

        public abstract IEnumerable<T> FetchAll<T>(string group = null, Func<string, T, int, string> keyFactory = null,
                                Func<string, IEnumerable<T>> itemFactory = null);

        public abstract void Remove(string key, string group = null);

        public abstract void RemoveAll<T>(string group = null);

        protected static string ResolveKey(string key, string group)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException();
            }

            if (string.IsNullOrWhiteSpace(group))
            {
                return key;
            }

            return group + "_" + key;
        }

        public abstract void Store<T>(string key, T value, string group = null);

        public abstract void StoreAll<T>(Func<string, T, int, string> keyFactory, IEnumerable<T> values, string group = null);
        
        #endregion
    }
}