using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace swingtrader
{
    public class UnixMillisecondTimestampConverter : DateTimeConverterBase
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if(reader.Value == null)
            {
                return null;
            }

            return DateTimeOffset.FromUnixTimeMilliseconds((long)reader.Value).LocalDateTime;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteRawValue((new DateTimeOffset((DateTime)value).ToUnixTimeMilliseconds()).ToString());
        }
    }
}
