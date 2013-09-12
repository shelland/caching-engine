using System;
using System.Collections.Generic;

namespace Shelland.CachingEngine.Logic
{
    public interface ICache
    {
        #region Methods

        /// <summary>
        /// Получает экземпляр типа <see cref="T"/> из кеша.
        /// </summary>
        /// <typeparam name="T">Тип.</typeparam>
        /// <param name="key">Ключ.</param>
        /// <param name="group">Группа.</param>
        /// <param name="factory">Фабрика.</param>
        /// <returns>Экземпляр <see cref="T"/>.</returns>
        T Fetch<T>(string key, string group = null, Func<string, string, T> factory = null);

        /// <summary>
        /// Получает все экземпляры типа <see cref="T"/> из кеша.
        /// </summary>
        /// <typeparam name="T">Тип.</typeparam>
        /// <param name="group">Группа.</param>
        /// <param name="keyFactory">Фабрика.</param>
        /// <param name="itemFactory">Фабрика.</param>
        /// <returns>Все объекты типа <see cref="T"/>.</returns>
        IEnumerable<T> FetchAll<T>(string group = null, Func<string, T, int, string> keyFactory = null,
                                Func<string, IEnumerable<T>> itemFactory = null);

        /// <summary>
        /// Удаление объекта по определенному ключу.
        /// </summary>
        /// <param name="key">Ключ.</param>
        /// <param name="group">Группа.</param>
        void Remove(string key, string group = null);

        /// <summary>
        /// Удаление всех объектов из кеша.
        /// </summary>
        /// <typeparam name="T">The item type.</typeparam>
        /// <param name="group">The grouping.</param>
        void RemoveAll<T>(string group = null);

        /// <summary>
        /// Сохранение объекта в кеш.
        /// </summary>
        /// <typeparam name="T">Тип.</typeparam>
        /// <param name="key">Ключ.</param>
        /// <param name="value">Значение.</param>
        /// <param name="group">Группа.</param>
        void Store<T>(string key, T value, string group = null);

        /// <summary>
        /// Сохранение всех значений в кеш
        /// </summary>
        /// <typeparam name="T">Тип.</typeparam>
        /// <param name="keyFactory">Фабрика.</param>
        /// <param name="values">Набор значений для сохранения.</param>
        /// <param name="group">Группа.</param>
        void StoreAll<T>(Func<string, T, int, string> keyFactory, IEnumerable<T> values, string group = null);
        
        #endregion 
    }
}