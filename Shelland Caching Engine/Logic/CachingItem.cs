using System;
using System.Runtime.Serialization;

namespace Shelland.CachingEngine.Logic
{
    /// <summary>
    /// Объект для сохранение в кеш.
    /// </summary>
    /// <typeparam name="T">Тип сохраняемого объекта</typeparam>
    [Serializable]
    public class CachingItem<T> : ISerializable
    {
        private static readonly Type _type = typeof(T);

        /// <summary>
        /// Конструктор для <see cref="CachingItem{T}"/>
        /// </summary>
        /// <param name="key">Ключ.</param>
        /// <param name="value">Значение.</param>
        /// <param name="group">Группа.</param>
        public CachingItem(string key, T value, string group)
        {
            Value = value;
            Group = group;

            Created = DateTime.UtcNow;
            LastAccessed = Created;
        }

        /// <summary>
        /// Создание экземпляра типа <see cref="CachingItem{T}"/> из сериализованных данных.
        /// </summary>
        /// <param name="info">Информация сериализации.</param>
        /// <param name="context">Контекст.</param>
        protected CachingItem(SerializationInfo info, StreamingContext context)
        {
            Created = info.GetDateTime("created");
            Key = info.GetString("key");
            LastAccessed = info.GetDateTime("lastAccessed");
            Group = info.GetString("group");
            Value = (T)info.GetValue("value", _type);
        }

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime Created { get; private set; }

        /// <summary>
        /// Группа
        /// </summary>
        public string Group { get; private set; }

        /// <summary>
        /// Ключ
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// Дата последнего доступа
        /// </summary>
        public DateTime LastAccessed { get; internal set; }

        /// <summary>
        /// Значение
        /// </summary>
        public T Value { get; private set; }

        /// <summary>
        /// Метаданные объекта сериализации
        /// </summary>
        /// <param name="info">Информация сериализации.</param>
        /// <param name="context">Контекст.</param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("created", Created);
            info.AddValue("group", Group);
            info.AddValue("key", Key);
            info.AddValue("lastAccessed", LastAccessed);
            info.AddValue("value", Value, _type);
        }
    }
}