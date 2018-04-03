using ChinoIM.Common.Helpers;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ChinoIM.Server
{
    public class ClientManager
    {
        private static ConcurrentQueue<ChinoClient> clientsForProcessing = new ConcurrentQueue<ChinoClient>();
        private static List<ChinoClient> clients = new List<ChinoClient>();
        private static object lockClientList = new object();

        private static ILogger logger = LogManager.CreateLogger<ClientManager>();

        public static List<ChinoClient> GetClients()
        {
            lock(lockClientList)
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
            return clients.Find(t => t.User.UID == uid);
        }

        public static ChinoClient GetClientForProcessing()
        {
            if (clientsForProcessing.TryDequeue(out var client))
            {
                return client;
            }
            return null;
        }

        public static void AddClientForProcessing(ChinoClient c)
        {
            clientsForProcessing.Enqueue(c);
        }

        public static void RegisterClient(ChinoClient c)
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
        
        public static void UnregisterClient(ChinoClient c)
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
