using ChinoIM.Common.Enums;
using ChinoIM.Common.Helpers;
using ChinoIM.Common.Serialization;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace ChinoIM.Common.Network
{
    public class Request : ISerializable
    {
        [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
        public RequestType Type { get; set; }
        public string Token { get; protected set; }
        public long SendTime { get; protected set; }
        public IDictionary<string, object> Payload { get; set; } = new Dictionary<string, object>();

        public Request()
        {

        }
        public Request(RequestType type)
        {
            Type = type;
        }
        public Request(RequestType type, IDictionary<string, object> payload)
        {
            Type = type;
            Payload = payload;
        }

        private string getPayloadJson()
        {
            return JsonSerializer.Serialize(Payload);
        }

        public string GetToken()
        {
            return CryptoHelper.BCryptHash(getPayloadJson() + SendTime);
        }

        public bool Verify()
        {
            return CryptoHelper.BCryptVerify(getPayloadJson() + SendTime, Token);
        }

        public void AddStamp()
        {
            Token = GetToken();
            SendTime = TimeService.CurrentTime;
        }

        public void ReadFromStream(SerializationReader reader)
        {
            Type = (RequestType)reader.ReadInt32();
            Payload = reader.ReadDictionary<string, object>();
        }

        public void WriteToStream(SerializationWriter writer)
        {
            writer.Write((int)Type);
            writer.Write(Payload);
        }

        public object this[string key]
        {
            get
            {
                return Payload[key];
            }
            set
            {
                Payload[key] = value;
            }
        }

    }
}
