using ChinoIM.Common.Network;

namespace ChinoIM.Server.Irc.Handler
{
    public class QuitHandler : IncomingHandler<IrcCommand, IrcClient>
    {
        public QuitHandler(IrcClient client) : base(client)
        {
            Condition = t => t.Type == IrcMessageType.QUIT;
        }

        public override void HandleIncoming(IrcCommand request)
        {
            Client.Connection.Disconnect("quit");
        }
    }
}
