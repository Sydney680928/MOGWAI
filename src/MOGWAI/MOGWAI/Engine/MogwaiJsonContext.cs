using System.Text.Json.Serialization;

namespace MOGWAI.Engine
{
    [JsonSerializable(typeof(ServerMessage))]
    [JsonSerializable(typeof(List<string>))]
    internal partial class MogwaiJsonContext : JsonSerializerContext { }
}