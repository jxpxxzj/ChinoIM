using ChinoIM.Common.Helpers;
using ChinoIM.Server.Irc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ChinoIM.Server
{
    public class ChinoServer
    {
        public static int Port = 6163;
        public static IPAddress IPAddressV4 = IPAddress.Any;
        public static IPAddress IPAddressV6 = IPAddress.IPv6Any;
        public static int WorkerCount = 4;
        public static string Hostname = "ChinoIM";

        private TcpClientListener listenerV4;
        private TcpClientListener listenerV6;

        public static bool AllowIrcConnection = true;
        private TcpClientListener listenerIrcV4;
        private TcpClientListener listenerIrcV6;
        public static int PortIrc = 6667;

        private List<ChinoWorker> workers = new List<ChinoWorker>();
        private ILogger logger = LogManager.CreateLogger<ChinoServer>();

        // private static FileSystemWatcher fsw;
        private Stopwatch timer = new Stopwatch();
        // internal static ConfigManager Config;

        public ChinoServer() : this(IPAddressV4, IPAddressV6, Port, AllowIrcConnection, PortIrc)
        {

        }

        public ChinoServer(IPAddress ipAddressV4, IPAddress ipAddressV6, int port, bool allowIrcConnection, int portIrc)
        {
            timer.Start();
            for (var i = 0; i < WorkerCount; i++)
            {
                workers.Add(new ChinoWorker(i));
                logger.LogInformation("Add worker {0}", i);
            }

            if (NetworkUtil.IsSupportIPv4)
            {
                listenerV4 = new TcpClientListener(ipAddressV4, port, "IPv4");

                if (allowIrcConnection)
                {
                    listenerIrcV4 = new TcpClientListener(ipAddressV4, portIrc, "IPv4 IRC");
                }
            }

            if (NetworkUtil.IsSupportIPv6)
            {
                listenerV6 = new TcpClientListener(ipAddressV6, port, "IPv6");
                if (allowIrcConnection)
                {
                    listenerIrcV6 = new TcpClientListener(ipAddressV6, portIrc, "IPv6 IRC");
                }
            }
        }

        public void Start()
        {
            if (listenerV4 != null)
            {
                listenerV4.TcpClientAccepted += Listener_TcpClientAccepted;
                listenerV4.Start();
            }
            if (listenerV6 != null)
            {
                listenerV6.TcpClientAccepted += Listener_TcpClientAccepted;
                listenerV6.Start();
            }
            if (listenerIrcV4 != null)
            {
                listenerIrcV4.TcpClientAccepted += ListenerIrc_TcpClientAccepted;
                listenerIrcV4.Start();
            }
            if (listenerIrcV6 != null)
            {
                listenerIrcV6.TcpClientAccepted += ListenerIrc_TcpClientAccepted;
                listenerIrcV6.Start();
            }
            mainLoop();
        }

        private void ListenerIrc_TcpClientAccepted(object sender, TcpClient e)
        {
            var client = new IrcClient(e);
            ClientManager.RegisterClient(client);
        }

        private void Listener_TcpClientAccepted(object sender, TcpClient e)
        {
            var client = new ChinoClient(e);
            ClientManager.RegisterClient(client);
        }

        private long lastCntTime = 0;
        private long lastGCTime = 0;
        private void mainLoop()
        {
            logger.LogInformation("Main loop is running...");
            while (true)
            {
                long current = TimeService.CurrentTime;
                foreach (var worker in workers)
                {
                    Task.Run(() => worker.DoWork());
                }

                if (current - lastCntTime > 10)
                {
                    logger.LogInformation("Client count: {0}", ClientManager.GetClientCount());
                    lastCntTime = current;
                }

                if (current - lastGCTime > 60)
                {
                    logger.LogInformation("GC called");
                    GC.Collect();
                    lastGCTime = current;
                }

                Thread.Sleep(200);
            }
        }
    }
}
