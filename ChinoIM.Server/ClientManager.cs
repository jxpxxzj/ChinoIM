using ChinoIM.Common.Helpers;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ChinoIM.Server
{
    public class ClientManager
    {
        private static ConcurrentQueue<Client> clientsForProcessing = new ConcurrentQueue<Client>();
        private static List<Client> clients = new List<Client>();
        private static object lockClientList = new object();

        private static ILogger logger = LogManager.CreateLogger<ClientManager>();

        public static List<Client> GetClients()
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

        public static Client FindClient(long uid)
        {
            var clients = GetClients();
            return clients.Find(t => t.User.UID == uid);
        }

        public static Client GetClientForProcessing()
        {
            if (clientsForProcessing.TryDequeue(out var client))
            {
                return client;
            }
            return null;
        }

        public static void AddClientForProcessing(Client c)
        {
            clientsForProcessing.Enqueue(c);
        }

        public static void RegisterClient(Client c)
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
        
        public static void UnregisterClient(Client c)
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
