using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ClassLibrary1
{
    public class MyModel
    {
        public string Value
        {
            get; set;
        }
    }

    public partial class Serializer
    {
        public static string Serialize(MyModel model)
        {
            return JsonSerializer.Serialize(
                model,
                SerializerContext.Default.MyModel);
        }

        public static string SerializeWithGenerics<T>(T model)
        {
            return JsonSerializer.Serialize(
                model,
                typeof(T),
                SerializerContext.Default);
        }
    }

    [JsonSerializable(typeof(MyModel))]
    internal partial class SerializerContext : JsonSerializerContext
    {
    }
}