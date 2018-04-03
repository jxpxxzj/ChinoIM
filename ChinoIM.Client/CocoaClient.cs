using ChinoIM.Common.Enums;
using ChinoIM.Common.Helpers;
using ChinoIM.Common.Network;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ChinoIM.Client
{
    public class CocoaClient
    {
        private ILogger logger = LogManager.CreateLogger<CocoaClient>();

        public static IPAddress ServerAddressV6 = IPAddress.IPv6Loopback;
        public static IPAddress ServerAddressV4 = IPAddress.Loopback;
        public static int Port = 6163;

        private bool isAuth;

        private Connection<Request> connection;

        public event EventHandler Connected;
        public event EventHandler<Request> Receive;
        public event EventHandler<string> Disconnected;
        protected virtual void OnConnected()
        {
            Connected?.Invoke(this, EventArgs.Empty);
        }
        protected virtual void OnReceive(Request e)
        {
            Receive?.Invoke(this, e);
        }
        protected virtual void OnDisconnected(string reason)
        {
            Disconnected?.Invoke(this, reason);
        }

        public CocoaClient(IPAddress serverV4, IPAddress serverV6, int port)
        {
            Task.Run(async () =>
            {
                logger.LogInformation("Connecting...");
                await Connect(serverV4, serverV6, port);
                mainLoop();
            });
        }

        public CocoaClient() : this(ServerAddressV4, ServerAddressV6, Port) { }

        ~CocoaClient()
        {
            connection.Disconnect();
        }


        public async Task Connect(IPAddress serverV4, IPAddress serverV6, int port)
        {
            IPAddress server = null;
            TcpClient tcpClient = null;
            if (NetworkUtil.IsSupportIPv6)
            {
                server = serverV6;
                tcpClient = new TcpClient(AddressFamily.InterNetworkV6);
                await tcpClient.ConnectAsync(server, port);
            }
            else if (NetworkUtil.IsSupportIPv4)
            {
                server = serverV4;
                tcpClient = new TcpClient(AddressFamily.InterNetwork);
                await tcpClient.ConnectAsync(server, port);
            }
            else
            {
                throw new SocketException();
            }

            if (server != null && tcpClient != null)
            {
                logger.LogInformation("Connected to {0}:{1}", server.ToString(), port);

                connection = new Connection<Request>(tcpClient, new JsonSerializer<Request>());
                connection.Received += Connection_Receive;
                connection.Disconnected += Connection_Disconnected;
                OnConnected();
            }
        }

        private void Connection_Disconnected(object sender, string e)
        {
            OnDisconnected(e);
        }

        private void Connection_Receive(object sender, Request e)
        {
            handleIncoming(e);
        }

        private async void mainLoop()
        {
            while (true)
            {
                await connection.Update();
                Thread.Sleep(200);
            }
        }

        private void pong()
        {
            sendRequest(RequestType.Pong);
        }

        private void handleIncoming(Request request)
        {
            if (!isAuth && request.Type != RequestType.User_LoginResult)
            {
                logger.LogWarning("Client not login, ignore request");
                return;
            }
            switch (request.Type)
            {
                case RequestType.Ping:
                    pong();
                    break;
                case RequestType.User_LoginResult:
                    if (long.Parse(request["result"].ToString()) > 0)
                    {
                        logger.LogInformation("Login success");
                        isAuth = true;
                    }
                    break;
            }

            if (request.Type != RequestType.Pong)
            {
                OnReceive(request);
            }
        }

        public void SendRequest(Request request)
        {
            if (request != null)
            {
                sendRequest(request.Type, request.Payload);
            }
        }

        private void sendRequest(RequestType type, IDictionary<string, object> payload = null)
        {
            if ((!isAuth || !connection.IsConnected) && type != RequestType.User_Login && type != RequestType.User_Register)
            {
                return;
            }

            var request = new Request
            {
                Payload = payload,
                Type = type
            };

            request.AddStamp();

            connection.SendRequest(request);
        }
    }
}
