using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace Common
{
    public enum MessageType
    {
        Message,
        ConnectionEvent,
        Invocation
    }

    public class Message<TPayload>
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public MessageType MessageType { get; set; }
        public TPayload Payload { get; set; }
    }
}