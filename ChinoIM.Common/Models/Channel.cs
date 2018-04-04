namespace ChinoIM.Common.Models
{
    public class Channel
    {
        public string Name;
        public string Topic;
        public virtual int UserCount { get; set; }
    }
}
