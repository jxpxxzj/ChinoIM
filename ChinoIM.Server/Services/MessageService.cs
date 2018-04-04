using ChinoIM.Common.Enums;
using ChinoIM.Common.Helpers;
using ChinoIM.Common.Models;
using ChinoIM.Common.Network;

namespace ChinoIM.Server.Services
{
    public class MessageService
    {
        public static void SendMessage(long sender, long target, MessageEndPoint targetType, string content, bool useEscape = false)
        {
            long container = targetType == MessageEndPoint.User ? -1 : target;
            var containerType = targetType;

            var message = new Message()
            {
                Sender = sender,
                Target = target,
                TargetType = MessageEndPoint.User,
                Container = container,
                ContainerType = containerType,
                UseEscape = useEscape,
                Content = content
            };

            if (containerType == MessageEndPoint.User)
            {
                var client = ClientManager.FindClient(target);
                var request = new Request()
                {
                    Type = RequestType.Message,
                    Payload = message.ToDictionary()
                };
                if (client != null)
                {
                    client.SendRequest(request);
                }
            }
            else
            {
                // get all user in channel or group
                // foreach user: send message
            }

        }
    }
}
