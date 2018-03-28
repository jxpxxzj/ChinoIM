namespace ChinoIM.Common.Models
{
    public class User
    {
        public long UID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public long CreateTime { get; set; }
    }
}
