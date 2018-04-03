using ChinoIM.Common.Enums;
using ChinoIM.Common.Helpers;
using ChinoIM.Common.Network;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ChinoIM.Client
{
    public class CocoaClient : Client<Request>
    {
        private ILogger logger = LogManager.CreateLogger<CocoaClient>();

        public static IPAddress ServerAddressV6 = IPAddress.IPv6Loopback;
        public static IPAddress ServerAddressV4 = IPAddress.Loopback;
        public static int Port = 6163;

        private bool isAuth;

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
            Connection.Disconnect();
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
                SetConnection(tcpClient, new JsonSerializer<Request>());
            }
        }

        private async void mainLoop()
        {
            while (true)
            {
                await Connection.Update();
                Thread.Sleep(200);
            }
        }

        private void pong()
        {
            sendRequest(RequestType.Pong);
        }

        public override void HandleIncoming(Request request)
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

        public override void SendRequest(Request request)
        {
            if (request != null)
            {
                sendRequest(request.Type, request.Payload);
            }
        }

        private void sendRequest(RequestType type, IDictionary<string, object> payload = null)
        {
            if ((!isAuth || !Connection.IsConnected) && type != RequestType.User_Login && type != RequestType.User_Register)
            {
                return;
            }

            var request = new Request
            {
                Payload = payload,
                Type = type
            };

            request.AddStamp();

            base.SendRequest(request);
        }
    }
}
