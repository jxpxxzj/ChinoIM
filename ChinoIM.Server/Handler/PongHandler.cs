using ChinoIM.Common.Enums;
using ChinoIM.Common.Helpers;
using ChinoIM.Common.Network;

namespace ChinoIM.Server.Handler
{
    public class PongHandler : IncomingHandler<Request, ChinoClient>
    {
        public PongHandler(ChinoClient client) : base(client)
        {
            Condition = t => t.Type == RequestType.Ping;
        }
        private long lastPingTime;
        private bool pinging;

        public void Ping()
        {
            long currentTime = TimeService.CurrentTime;
            if (!pinging && (currentTime - lastPingTime > 30))
            {
                sendPing();
            }
        }

        public void Pong()
        {
            pinging = false;
        }

        private void sendPing()
        {
            pinging = true;
            lastPingTime = TimeService.CurrentTime;
            Client.SendRequest(new Request(RequestType.Ping));
        }

        public override void HandleIncoming(Request request)
        {
            Pong();
        }
    }
}
