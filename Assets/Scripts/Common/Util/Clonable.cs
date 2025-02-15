using System;
using System.Collections.Generic;

namespace Ulko
{
    public interface IClonable
    {
        void Clone(object source);
    }

    public static class Clonable
    {
        public static T Clone<T>(this T source) where T : IClonable
        {
            T obj = Activator.CreateInstance<T>();
            obj.Clone(source);

            return obj;
        }

        public static List<string> Clone(this List<string> source)
        {
            var list = new List<string>();
            foreach (var element in source)
            {
                list.Add(element);
            }

            return list;
        }

        public static List<T> Clone<T>(this List<T> source) where T : IClonable
        {
            var list = new List<T>();
            foreach (var element in source)
            {
                list.Add(element.Clone());
            }

            return list;
        }

        public static Dictionary<K, float> Clone<K>(this Dictionary<K, float> source)
        {
            var dict = new Dictionary<K, float>();
            foreach (var kvp in source)
            {
                dict.Add(kvp.Key, kvp.Value);
            }

            return dict;
        }

        public static Dictionary<K, V> Clone<K, V>(this Dictionary<K, V> source) where V : IClonable
        {
            var dict = new Dictionary<K, V>();
            foreach (var kvp in source)
            {
                dict.Add(kvp.Key, kvp.Value.Clone());
            }

            return dict;
        }
    }
}
