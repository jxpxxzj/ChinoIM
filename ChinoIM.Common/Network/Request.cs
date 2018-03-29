using ChinoIM.Common.Enums;
using ChinoIM.Common.Helpers;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ChinoIM.Common.Network
{
    public class Request : ISerializable
    {
        [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
        public RequestType Type { get; set; }
        public string Token { get; set; }
        public long SendTime { get; set; }
        public IDictionary<string, object> Payload { get; set; } = new Dictionary<string, object>();

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

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            
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
