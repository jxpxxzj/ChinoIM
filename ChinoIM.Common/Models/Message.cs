using ChinoIM.Common.Enums;

namespace ChinoIM.Common.Models
{
    public class Message
    {
        public long ID { get; set; }
        public long Sender { get; set; }
        public long Container { get; set; }
        public EndpointType ContainerType { get; set; }
        public long Target { get; set; }
        public EndpointType TargetType { get; set; }
        
        public bool UseEscape { get; set; }
        public string Content { get; set; }
    }
}
