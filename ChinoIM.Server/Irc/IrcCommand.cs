using ChinoIM.Common.Helpers;
using ChinoIM.Common.Serialization;
using Microsoft.Extensions.Logging;
using System;

namespace ChinoIM.Server.Irc
{
    public class IrcCommand : ISerializable
    {
        private ILogger logger;

        public string Command { get; set; } = string.Empty;
        public IrcMessageType Type { get; set; } = IrcMessageType.UNKNOWN;

        public IrcCommand() : this(string.Empty, IrcMessageType.UNKNOWN) { }

        public IrcCommand(string command) : this (command, IrcMessageType.UNKNOWN) { }

        public IrcCommand(string command, IrcMessageType type)
        {
            Command = command;
            Type = type;
            logger = LogManager.CreateLogger<IrcCommand>();
        }

        public IrcMessageType ParseType()
        {
            if (string.IsNullOrEmpty(Command)) return IrcMessageType.UNKNOWN;
            var split = Command.Split(' ');
            var name = split[0];
            var type = IrcMessageType.UNKNOWN;
            try
            {
                type = Enum.Parse<IrcMessageType>(name, true);
            }
            catch 
            {
                logger.LogError("Unknown message type: {0}", name);
            }
            Type = type;
            return type;
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
