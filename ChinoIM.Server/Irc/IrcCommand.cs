using ChinoIM.Common.Serialization;

namespace ChinoIM.Server.Irc
{
    public class IrcCommand : ISerializable
    {
        public string Command { get; set; } = string.Empty;

        public IrcCommand() : this(string.Empty) { }

        public IrcCommand(string command)
        {
            Command = command;
        }

        public void ReadFromStream(SerializationReader reader)
        {
            Command = reader.ReadUTF8();
        }

        public void WriteToStream(SerializationWriter writer)
        {
            writer.WriteUTF8(Command);
        }
    }
}
