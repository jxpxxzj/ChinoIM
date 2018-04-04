using ChinoIM.Common.Network;

namespace ChinoIM.Server.Irc.Handler
{
    public class NickHandler : IncomingHandler<IrcCommand, IrcClient>
    {
        public NickHandler(IrcClient client) : base(client)
        {
            Condition = t => t.Type == IrcMessageType.NICK;
        }

        public override void HandleIncoming(IrcCommand request)
        {
            Client.Username = request[1];
        }
    }
}
