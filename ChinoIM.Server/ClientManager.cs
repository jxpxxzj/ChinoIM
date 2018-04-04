using ChinoIM.Common.Helpers;
using ChinoIM.Common.Network;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ChinoIM.Server
{
    public class ClientManager
    {
        private static ConcurrentQueue<IUpdateable> clientsForProcessing = new ConcurrentQueue<IUpdateable>();
        private static List<IUpdateable> clients = new List<IUpdateable>();
        private static object lockClientList = new object();

        private static ILogger logger = LogManager.CreateLogger<ClientManager>();

        public static List<IUpdateable> GetClients()
        {
            lock (lockClientList)
            {
                return clients;
            }
        }

        public static int GetClientCount()
        {
            return GetClients().Count;
        }

        public static ChinoClient FindClient(long uid)
        {
            var clients = GetClients();
            foreach (var t in clients)
            {
                if (t is ChinoClient client)
                {
                    if (client.User.UID == uid)
                    {
                        return client;
                    }
                }
            }
            return null;
        }

        public static IUpdateable GetClientForProcessing()
        {
            if (clientsForProcessing.TryDequeue(out var client))
            {
                return client;
            }
            return null;
        }

        public static void AddClientForProcessing(IUpdateable c)
        {
            clientsForProcessing.Enqueue(c);
        }

        public static void RegisterClient(IUpdateable c)
        {
            lock (lockClientList)
            {
                // if (FindClient(c.User.UID) == null)
                // {
                clients.Add(c);
                AddClientForProcessing(c);
                logger.LogInformation("{0} registered", c.ToString());
                // }
                // else
                // {
                // logger.LogWarning("Client existed: {0}({1})", c.User.Username, c.User.UID);

                // }
            }
        }

        public static void UnregisterClient(IUpdateable c)
        {
            lock (lockClientList)
            {
                // c.Connection.Disconnect();
                clients.Remove(c);
                logger.LogInformation("Client unregistered");
            }
        }
    }
}
