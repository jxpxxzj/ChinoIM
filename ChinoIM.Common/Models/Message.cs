using ChinoIM.Common.Enums;

namespace ChinoIM.Common.Models
{
    public class Message
    {
        public long ID { get; set; }
        public long Sender { get; set; }
        public long Container { get; set; }
        public MessageEndPoint ContainerType { get; set; }
        public long Target { get; set; }
        public MessageEndPoint TargetType { get; set; }
        
        public bool UseEscape { get; set; }
        public string Content { get; set; }
    }
}
