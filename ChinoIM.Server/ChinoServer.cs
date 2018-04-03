using ChinoIM.Common.Helpers;
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

        private TcpListener listenerV4;
        private TcpListener listenerV6;
        private List<ChinoWorker> workers = new List<ChinoWorker>();
        private ILogger logger = LogManager.CreateLogger<ChinoServer>();

        // private static FileSystemWatcher fsw;
        private Stopwatch timer = new Stopwatch();
        // internal static ConfigManager Config;

        public ChinoServer() : this(IPAddressV4, IPAddressV6, Port)
        {

        }

        public ChinoServer(IPAddress ipAddressV4, IPAddress ipAddressV6, int port)
        {
            timer.Start();
            for (var i = 0; i < WorkerCount; i++)
            {
                workers.Add(new ChinoWorker(i));
                logger.LogInformation("Add worker {0}", i);
            }

            if (NetworkUtil.IsSupportIPv4)
            {
                listenerV4 = new TcpListener(ipAddressV4, port);
                listenerV4.Start();
            }
            
            if (NetworkUtil.IsSupportIPv6)
            {
                listenerV6 = new TcpListener(ipAddressV6, port);
                listenerV6.Start();
            }
        }

        public void Start()
        {
            if (listenerV4 != null)
            {
                acceptConnectionV4();
            }
            if (listenerV6 != null)
            {
                acceptConnectionV6();
            }
            mainLoop();
        }

        private async Task listeningConnection(TcpListener listener, string appendix = "")
        {
            while(true)
            {
                TcpClient tcpClient = null;
                try
                {
                    tcpClient = await listener.AcceptTcpClientAsync();
                }
                catch
                {

                }
                if (tcpClient != null)
                {
                    logger.LogInformation("TcpClient accepted " + appendix);
                    var client = new ChinoClient(tcpClient);
                    ClientManager.RegisterClient(client);
                }
            }
        }

        private void acceptConnectionV4()
        {
            logger.LogInformation("Listening on {0}:{1} for IPv4 connections...", IPAddressV4.ToString(), Port);
            Task.Run(() => listeningConnection(listenerV4, "from IPv4"));
        }

        private void acceptConnectionV6()
        {
            logger.LogInformation("Listening on {0}:{1} for IPv6 connections...", IPAddressV6.ToString(), Port);
            Task.Run(() => listeningConnection(listenerV6, "from IPv6"));
        }

        private long lastCntTime = 0;
        private long lastGCTime = 0;
        private void mainLoop()
        {
            logger.LogInformation("Main loop is running...");
            while(true)
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
