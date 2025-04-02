using Newtonsoft.Json.Linq;

namespace Ulko.Data
{
    public interface ICard
    {
        int Id { get; }
        object Data { get; }
    }

    public class Card<T> : ICard, IJsonObject, IClonable where T : IJsonObject, IClonable
    {
        public int Id { get; private set; } //unique card id
        public T Data { get; private set; }
        object ICard.Data => Data;

        private static int currentId;

        public Card() { }

        public Card(T data)
        {
            Id = currentId++;
            Data = data;
        }

        public Card(int id, T data)
        {
            Id = id;
            Data = data;
        }

        public void Clone(object source)
        {
            Clone(source as Card<T>);
        }

        public void Clone(Card<T> source)
        {
            Id = source.Id;
            Data = source.Data.Clone();
        }

        public void FromJson(JToken token)
        {
            Id = token["id"].ToObject<int>();
            Data = token["data"].Parse<T>();
        }

        public JToken ToJson()
        {
            return new JObject
            {
                {"id", Id },
                {"data", Data.ToJson() }
            };
        }
    }
}
