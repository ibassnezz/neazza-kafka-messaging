using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Niazza.KafkaMessaging
{
    public class SerializingSettings: JsonSerializerSettings
    {
        public SerializingSettings()
        {
            NullValueHandling = NullValueHandling.Ignore;
            Culture = CultureInfo.InvariantCulture;
            ContractResolver = new CamelCasePropertyNamesContractResolver();
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        }
    }
}