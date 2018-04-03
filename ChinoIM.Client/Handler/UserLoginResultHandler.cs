using ChinoIM.Common.Enums;
using ChinoIM.Common.Network;
using Microsoft.Extensions.Logging;

namespace ChinoIM.Client.Handler
{
    public class UserLoginResultHandler : IncomingHandler<Request, CocoaClient>
    {
        public UserLoginResultHandler(CocoaClient client) : base(client)
        {
            Condition = t => t.Type == RequestType.User_LoginResult;
        }

        public override void HandleIncoming(Request request)
        {
            if (long.Parse(request["result"].ToString()) > 0)
            {
                logger.LogInformation("Login success");
                Client.isAuth = true;
            }
        }
    }
}
