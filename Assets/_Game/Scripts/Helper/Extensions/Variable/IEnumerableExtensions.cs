using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace Handler.Extensions
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<T> FindMin<T, TKey>(this IEnumerable<T> enumerable, Func<T, TKey> selector)
        {
            var min = enumerable.Min(selector);
            return enumerable.Where(c => selector(c).Equals(min));
        }

        public static IEnumerable<T> FindMax<T, TKey>(this IEnumerable<T> enumerable, Func<T, TKey> selector)
        {
            var max = enumerable.Max(selector);
            return enumerable.Where(c => selector(c).Equals(max));
        }

        public static T GetRandomElement<T>(this IEnumerable<T> enumerable)
        {
            if (!enumerable.Any())
                throw new ArgumentException("The collection cannot be empty.");

            var rng = new Random();
            var index = rng.Next(0, enumerable.Count());

            return enumerable.ElementAt(index);
        }

        public static T RemoveRandom<T>(this IList<T> list)
        {
            if (list.Count == 0) throw new IndexOutOfRangeException("Cannot remove a random item from an empty list");
            var index = UnityEngine.Random.Range(0, list.Count);
            var item = list[index];
            list.RemoveAt(index);
            return item;
        }

        public static IEnumerable<TSource> Shuffle<TSource>(this IEnumerable<TSource> self)
        {
            if (self == null)
                throw new ArgumentNullException(nameof(self), "The collection cannot be null.");

            var list = self.ToList();
            var n = list.Count;
            for (var i = n - 1; i > 0; i--)
            {
                var j = UnityEngine.Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }

            return list;
        }

        public static IEnumerable<TComponent> BetterForEach<TComponent>(this IEnumerable<TComponent> enumerable,
            Action<TComponent> action) 
            where TComponent : Component
        {
            if (enumerable == null)
                throw new ArgumentNullException(nameof(enumerable), "The collection cannot be null.");

            if (action == null)
                throw new ArgumentNullException(nameof(action), "The action cannot be null.");

            foreach (var item in enumerable)
            {
                action(item);
                yield return item;
            }
        }

        public static IEnumerable<TComponent> BetterReverseForEach<TComponent>(this IEnumerable<TComponent> enumerable,
            Action<TComponent> action) 
            where TComponent : Component
        {
            if (enumerable == null)
                throw new ArgumentNullException(nameof(enumerable), "The collection cannot be null.");

            if (action == null)
                throw new ArgumentNullException(nameof(action), "The action cannot be null.");

            var list = new List<TComponent>(enumerable);
            for (var i = list.Count - 1; i >= 0; i--)
            {
                action(list[i]);
                yield return list[i];
            }
        }

        public static IEnumerable<T> GetElementWithModulus<T>(this IEnumerable<T> enumerable, int index)
        {
            if (enumerable == null)
                throw new ArgumentNullException(nameof(enumerable), "The collection cannot be null.");

            var list = enumerable.ToList();
            if (list.Count == 0)
                throw new ArgumentException("The collection cannot be empty.", nameof(enumerable));

            return list.Select((item, i) => list[(i + index) % list.Count]);
        }

        public static IEnumerable<TSource> Replace<TSource>(this IEnumerable<TSource> self, 
            IEnumerable<TSource> src,
            Func<TSource, TSource, bool> func)
        {
            if (self == null)
                throw new ArgumentNullException(nameof(self), "The collection cannot be null.");

            if (src == null)
                throw new ArgumentNullException(nameof(src), "The source collection cannot be null.");

            if (func == null)
                throw new ArgumentNullException(nameof(func), "The comparison function cannot be null.");

            var selfList = self.ToList();
            var srcList = src.ToList();

            var adds = srcList.Where(x => !selfList.Any(y => func(x, y)));
            var removes = selfList.Where(x => !srcList.Any(y => func(x, y)));

            selfList.AddRange(adds);
            selfList = selfList.Except(removes).ToList();

            return selfList;
        }

        public static IEnumerable<T> GetUniqueRandomItems<T>(this IEnumerable<T> enumerable, int count)
        {
            if (!enumerable.Any())
                throw new AggregateException("Cannot get unique random items from empty list.");

            var collection = enumerable.ToList();
            var rng = new Random(DateTime.Now.Millisecond);

            if (count > collection.Count)
                count = collection.Count;

            for (var i = 0; i < count; i++)
            {
                var index = rng.Next(collection.Count);
                var item = collection[index];
                yield return item;
                collection.RemoveAt(index);
            }
        }

        public static IEnumerable<T> GetWeightedRandomItems<T>(this IEnumerable<T> enumerable, int count,
            Func<T, float> weightSelector)
        {
            if (!enumerable.Any())
                throw new AggregateException("Cannot get weighted random items from empty list.");

            var collection = enumerable.ToList();
            var rng = new Random(DateTime.Now.Millisecond);

            if (count > collection.Count)
                count = collection.Count;

            for (var i = 0; i < count; i++)
            {
                var totalWeight = collection.Sum(weightSelector);
                var randomWeight = rng.NextDouble() * totalWeight;
                var currentWeight = 0f;
                var item = collection.FirstOrDefault(c =>
                {
                    currentWeight += weightSelector(c);
                    return currentWeight >= randomWeight;
                });
                yield return item;
                collection.Remove(item);
            }
        }
    }
}