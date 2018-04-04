using ChinoIM.Common.Helpers;
using ChinoIM.Common.Network;
using ChinoIM.Server.Irc.Handler;
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
        private PingPongHandler pingPongHandler;
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
            Connection.MidUpdate += Connection_MidUpdate;
            Connection.TimeoutLimit = 120;
            registerHandler();
            sendWelcomeText();
        }

        private void sendWelcomeText()
        {
            SendRequest(IrcCommands.RPL_WELCOME, ":- Welcome to ChinoIM.");
        }

        private void Connection_MidUpdate(object sender, System.EventArgs e)
        {
            pingPongHandler.Ping();
        }

        private void registerHandler()
        {
            pingPongHandler = new PingPongHandler(this);
            AddHandler(pingPongHandler);
            AddHandler(new NickHandler(this));
            AddHandler(new QuitHandler(this));
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
            ProcessHandler(request);
        }

        public void SendRequest(IrcCommands command, string message, params object[] args)
        {
            if (args.Length == 0)
                SendRequest(string.Format(":{0} {1:000} {2} {3}\r\n", ChinoServer.Hostname, (int)command, UsernameUnderscored, message));
            else
                SendRequest(string.Format(":{0} {1:000} {2} {3}\r\n", ChinoServer.Hostname, (int)command, UsernameUnderscored, string.Format(message, args)));
        }

        public void SendRequest(string content)
        {
            SendRequest(new IrcCommand(content));
        }

        public void SendRequest(string message, params object[] args)
        {
            SendRequest(string.Format(message, args));
        }

    }
}
