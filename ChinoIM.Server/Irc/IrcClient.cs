using ChinoIM.Common.Helpers;
using ChinoIM.Common.Network;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using System.Threading.Tasks;

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
            logger = LogManager.CreateLogger<IrcClient>(ToString());
            Connection.TimeoutLimit = 120;
        }

        public override void HandleIncoming(IrcCommand request)
        {
            logger.LogInformation(request.Command);
        }

        public async Task<bool> Check(ChinoWorker worker)
        {
            return await Connection.Update();
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
