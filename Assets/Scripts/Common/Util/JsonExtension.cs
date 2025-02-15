using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Ulko
{
    public interface IJsonObject
    {
        void FromJson(JToken token);
        JToken ToJson();
    }

    public static class JsonExtension
    {
        public static JToken ToJson<T>(this T value) where T : Enum
        {
            return value.ToString();
        }

        public static JToken ToJson(this List<string> list)
        {
            var array = new JArray();
            foreach (var element in list)
                array.Add(element);

            return array;
        }

        public static JToken ToJson<T>(this List<T> list) where T : IJsonObject
        {
            var array = new JArray();
            foreach (var element in list)
                array.Add(element.ToJson());

            return array;
        }

        public static JToken ToJson(this Dictionary<string, float> dict)
        {
            var jObject = new JObject();
            foreach (var kvp in dict)
            {
                jObject.Add(kvp.Key, kvp.Value);
            }
            return jObject;
        }

        public static JToken ToJson(this Dictionary<string, string> dict)
        {
            var jObject = new JObject();
            foreach (var kvp in dict)
            {
                jObject.Add(kvp.Key, kvp.Value);
            }
            return jObject;
        }

        public static JToken ToJson<T>(this Dictionary<string, T> dict) where T : IJsonObject
        {
            var jObject = new JObject();
            foreach (var kvp in dict)
            {
                jObject.Add(kvp.Key, kvp.Value.ToJson());
            }
            return jObject;
        }

        public static T ParseEnum<T>(this JToken json) where T : unmanaged, Enum
        {
            if (json != null && Enum.TryParse(json.ToString(), true, out T value))
            {
                return value;
            }

            return default;
        }

        public static T Parse<T>(this JToken json) where T : IJsonObject
        {
            T obj = Activator.CreateInstance<T>();
            if (json != null) obj.FromJson(json);

            return obj;
        }

        public static List<string> ParseStringList(this JToken json)
        {
            var list = new List<string>();
            if (json != null && json is JArray array)
            {
                foreach (var element in array)
                    list.Add(element.ToString());
            }

            return list;
        }

        public static List<T> ParseList<T>(this JToken json) where T : IJsonObject
        {
            var list = new List<T>();
            if (json != null && json is JArray array)
            {
                foreach (var element in array)
                    list.Add(element.Parse<T>());
            }

            return list;
        }

        public static Dictionary<string, string> ParseStringDict(this JToken json)
        {
            var dict = new Dictionary<string, string>();
            if (json != null && json is JObject jObject)
            {
                foreach (var element in jObject)
                {
                    dict.Add(element.Key, element.Value.ToString());
                }
            }

            return dict;
        }

        public static Dictionary<string, T> ParseSimpleDict<T>(this JToken json) where T : unmanaged
        {
            var dict = new Dictionary<string, T>();
            if (json != null && json is JObject jObject)
            {
                foreach (var obj in jObject)
                {
                    dict.Add(obj.Key, obj.Value.ToObject<T>());
                }
            }

            return dict;
        }

        public static Dictionary<string, T> ParseDict<T>(this JToken json) where T : IJsonObject
        {
            var dict = new Dictionary<string, T>();
            if (json != null && json is JObject jObject)
            {
                foreach (var obj in jObject)
                {
                    dict.Add(obj.Key, obj.Value.Parse<T>());
                }
            }

            return dict;
        }
    }
}
