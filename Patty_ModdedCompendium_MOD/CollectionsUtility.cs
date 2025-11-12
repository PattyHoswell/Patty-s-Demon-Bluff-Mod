using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Patty_ModdedCompendium_MOD
{
    internal static class CollectionsUtility
    {
        public static T[] Insert<T>(this Il2CppArrayBase<T> array, int index, T value)
        {
            var list = new List<T>(array);
            if (index >= array.Length)
            {
                list.Add(value);
            }
            else
            {
                list.Insert(index, value);
            }
            return list.ToArray();
        }
        public static T[] Insert<T>(this T[] array, int index, T value)
        {
            var list = new List<T>(array);
            if (index >= array.Length)
            {
                list.Add(value);
            }
            else
            {
                list.Insert(index, value);
            }
            return list.ToArray();
        }

        public static string GetName(this Enum @enum)
        {
            return Enum.GetName(@enum.GetType(), @enum);
        }

        public static void AddToList<K, V>(this Dictionary<K, List<V>> dict, K key, V value)
        {
            if (!dict.ContainsKey(key))
            {
                dict[key] = new List<V>();
            }
            dict[key].Add(value);
        }
        public static bool HasValue<K, V>(this Dictionary<K, List<V>> dict, V value)
        {
            return dict.Values.Any(x => x.Contains(value));
        }
        public static void Populate<K, V>(this Dictionary<K, List<V>> dict) where K : Enum
        {
            foreach (var @enum in Enum.GetValues(typeof(K)).Cast<K>())
            {
                dict[@enum] = new List<V>();
            }
        }

        public static int FindIndex<T>(this IEnumerable<T> enumerable, T value)
        {
            return enumerable.FindIndex(x => EqualityComparer<T>.Default.Equals(x, value));
        }

        public static int FindLastIndex<T>(this IEnumerable<T> enumerable, T value)
        {
            return enumerable.FindLastIndex(x => EqualityComparer<T>.Default.Equals(x, value));
        }

        public static int FindIndex<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate)
        {
            var idx = 0;
            using (var enumerator = enumerable.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (predicate.Invoke(enumerator.Current))
                    {
                        return idx;
                    }
                    idx++;
                }
            }
            return -1;
        }

        public static int FindLastIndex<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate)
        {
            var lastIndex = -1;
            var currentIndex = 0;
            using (var enumerator = enumerable.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (predicate.Invoke(enumerator.Current))
                    {
                        lastIndex = currentIndex;
                    }
                    currentIndex++;
                }
            }
            return lastIndex;
        }
    }
}
