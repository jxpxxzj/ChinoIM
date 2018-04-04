using ChinoIM.Common.Enums;
using ChinoIM.Common.Network;

namespace ChinoIM.Server.Handler
{
    public class UserLogoutHandler : IncomingHandler<Request, ChinoClient>
    {
        public UserLogoutHandler(ChinoClient client) : base(client)
        {
            Condition = t => t.Type == RequestType.User_Logout;
        }

        public override void HandleIncoming(Request request)
        {
            Client.Connection.Disconnect("logout");
        }
    }
}
