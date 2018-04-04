using ChinoIM.Common.Helpers;
using ChinoIM.Common.Network;

namespace ChinoIM.Server.Irc.Handler
{
    public class PingPongHandler : IncomingHandler<IrcCommand, IrcClient>
    {
        public PingPongHandler(IrcClient client) : base(client)
        {
            Condition = t => t.Type == IrcMessageType.PONG || t.Type == IrcMessageType.PING;
        }
        private long lastPingTime;
        private bool pinging;

        public void Ping()
        {
            long currentTime = TimeService.CurrentTime;
            if (!pinging && (currentTime - lastPingTime > 60))
            {
                SendPing();
            }
        }
        public void SendPing()
        {
            pinging = true;
            lastPingTime = TimeService.CurrentTime;
            Client.SendRequest("PING {0}", ChinoServer.Hostname);
        }
        public void Pong()
        {
            pinging = false;
        }

        public override void HandleIncoming(IrcCommand request)
        {
            if (request.Type == IrcMessageType.PONG)
            {
                Pong();
                return;
            }

            if (request.Type == IrcMessageType.PING)
            {
                Client.SendRequest(":{0} {1}", ChinoServer.Hostname, request.Command.Replace("PING", "PONG"));
            }
            
        }
    }
}
