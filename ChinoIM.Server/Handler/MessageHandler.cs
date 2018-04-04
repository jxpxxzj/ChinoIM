using ChinoIM.Common.Enums;
using ChinoIM.Common.Network;
using ChinoIM.Server.Services;
using System;

namespace ChinoIM.Server.Handler
{
    public class MessageHandler : IncomingHandler<Request, ChinoClient>
    {
        public MessageHandler(ChinoClient client) : base(client)
        {
            Condition = t => t.Type == RequestType.Message;
        }

        public override void HandleIncoming(Request request)
        {
            long targetId = long.Parse(request["Target"].ToString());
            var targetType = Enum.Parse<EndpointType>(request["TargetType"].ToString());
            MessageService.SendMessage(Client.User.UID, targetId, targetType, request["Content"].ToString(), bool.Parse(request["UseEscape"].ToString()));
        }
    }
}
