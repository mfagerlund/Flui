using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Flui
{
    internal static class Extensions
    {
        public static TValue SafeGetValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key) => !dictionary.TryGetValue(key, out TValue value) ? default(TValue) : value;
        
        public static TValue GetOrCreate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<TValue> constructor)
        {
            if (!dictionary.TryGetValue(key, out TValue value))
            {
                value = constructor();
                dictionary.Add(key, value);
            }

            return value;
        }
    
        public static void ForEach<T>(this IEnumerable<T> list, Action<T> action)
        {
            foreach (T item in list)
            {
                action(item);
            }
        }
        
        public static string SJoin<T>(this IEnumerable<T> list, string separator = ", ") => string.Join(separator, list.Select(x => x == null ? "" : x.ToString()));
        public static string SJoin<T>(this IEnumerable<T> list, Func<T, object> convert, string separator = ", ") => string.Join(separator, list.Select(convert));

        public static string ToHexString(this Color color) => "#" + ((int)(color.r*255)).ToString("X2") + ((int)(color.g*255)).ToString("X2") + ((int)(color.b*255)).ToString("X2");
    }
}
