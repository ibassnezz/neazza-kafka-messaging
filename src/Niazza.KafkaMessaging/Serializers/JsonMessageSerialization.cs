using System;
using Newtonsoft.Json;

namespace Niazza.KafkaMessaging.Serializers
{
    public class JsonMessageSerialization : IMessageSerialization
    {
        public JsonMessageSerialization(Action<JsonSerializerSettings> action = null)
        {
            _settings = new SerializingSettings();
            action?.Invoke(_settings);
        }

        private readonly JsonSerializerSettings _settings;

        public object Deserialize(string message, Type type)
        {
            return JsonConvert.DeserializeObject(message, type, _settings);
        }

        public string Serialize(object objectToSerialize)
        {
            return JsonConvert.SerializeObject(objectToSerialize, _settings);
        }
    }
}
