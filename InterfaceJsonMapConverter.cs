using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Application
{
    internal class InterfaceJsonMapConverter : JsonConverterFactory
    {
        private IDictionary<Type, Type> _types =
            new[]
            {      //maps
                    (typeof(IAccount), typeof(Account)),
                    (typeof(IAddress), typeof(Address))
            }
            .ToDictionary(kv => kv.Item1, kv => kv.Item2);

        public override bool CanConvert(Type typeToConvert)
        {
            return _types.Keys.Any(t => t == typeToConvert);
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            MethodInfo method = this.GetType().GetMethod(nameof(GetGenericConverter), BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo generic = method.MakeGenericMethod(_types[typeToConvert]);//pass mapped type instead of interface type
            JsonConverter converter = (JsonConverter)generic.Invoke(this, null);
            return converter;
        }
        private GenericConverter<T> GetGenericConverter<T>()
        {
            return new GenericConverter<T>();//return a generic serializer with the updated type
        }
    }

    internal class GenericConverter<T> : JsonConverter<T>
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize<T>(ref reader, options);
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize<T>(writer, value, options);
        }
    }
}
