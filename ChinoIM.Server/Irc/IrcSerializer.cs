using ChinoIM.Common.Serialization;

namespace ChinoIM.Server.Irc
{
    public class IrcSerializer : ISerializer<IrcCommand>
    {
        public IrcCommand Deserialize(string data)
        {
            var str = data.TrimEnd('\n').Replace("\r", "");
            return new IrcCommand(str);
        }

        public string Serialize(IrcCommand obj)
        {
            var message = obj.Command;
            if (!message.EndsWith("\r\n")) message += "\r\n";
            return message;
        }
    }
}
