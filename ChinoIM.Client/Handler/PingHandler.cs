using ChinoIM.Common.Enums;
using ChinoIM.Common.Network;

namespace ChinoIM.Client.Handler
{
    class PingHandler : IncomingHandler<Request, CocoaClient>
    {
        public PingHandler(CocoaClient client) : base(client)
        {
            Condition = t => t.Type == RequestType.Ping;
        }

        public override void HandleIncoming(Request request)
        {
            sendPong();
        }

        private void sendPong()
        {
            Client.SendRequest(new Request(RequestType.Pong));
        }
    }
}
