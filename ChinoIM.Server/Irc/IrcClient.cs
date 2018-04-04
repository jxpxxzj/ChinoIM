using ChinoIM.Common.Helpers;
using ChinoIM.Common.Network;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;

namespace ChinoIM.Server.Irc
{
    public class IrcClient : Client<IrcCommand, IrcClient>
    {
        private ILogger logger;

        protected string username;
        public string UsernameUnderscored;
        public bool IsAdmin;
        public virtual string Username
        {
            get => username;
            set
            {
                username = value;
                UsernameUnderscored = Username.Replace(' ', '_');
            }
        }

        public string IrcFullName { get { return UsernameUnderscored + "!chino"; } }
        public string IrcPrefix = "+";

        public IrcClient(TcpClient tcpClient)
        {
            SetConnection(tcpClient, new IrcSerializer());
            logger = LogManager.CreateLogger<IrcClient>(Connection.ToString(string.Empty));
            Connection.TimeoutLimit = 120;
        }

        protected override void Connection_Received(object sender, IrcCommand e)
        {
            if (!Connection.IsConnected)
            {
                return;
            }
            var str = e.Command;
            var split = str.Split('\n');
            foreach (var line in split)
            {
                if (line.Length > 0)
                {
                    var command = new IrcCommand(line);
                    command.ParseType();
                    HandleIncoming(command);
                }
            }      
        }

        public override void HandleIncoming(IrcCommand request)
        {
            logger.LogInformation(request.Type.ToString());
        }

        private void SendIrcRaw(IrcCommands command, string message, params object[] args)
        {
            if (args.Length == 0)
                SendIrcRaw(string.Format(":{0} {1:000} {2} {3}\r\n", ChinoServer.Hostname, (int)command, UsernameUnderscored, message));
            else
                SendIrcRaw(string.Format(":{0} {1:000} {2} {3}\r\n", ChinoServer.Hostname, (int)command, UsernameUnderscored, string.Format(message, args)));
        }

        private void SendIrcRaw(string content)
        {
            SendRequest(new IrcCommand(content));
        }

    }
}
