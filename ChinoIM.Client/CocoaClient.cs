using ChinoIM.Common.Enums;
using ChinoIM.Common.Helpers;
using ChinoIM.Common.Requests;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChinoIM.Client
{
    public class CocoaClient
    {
        private static int timeoutSeconds = 90;
        public static long CurrentTime
        {
            get
            {
                var unixTime = new DateTime(1970, 1, 1);
                var ts = DateTime.Now - unixTime;
                return (int)ts.TotalSeconds;
            }
        }

        private ILogger logger = LogManager.CreateLogger<CocoaClient>();

        public static IPAddress ServerAddressV6 = IPAddress.IPv6Loopback;
        public static IPAddress ServerAddressV4 = IPAddress.Loopback;
        public static int Port = 6163;

        private TcpClient tcpClient;

        private bool isConnected;
        private bool isAuth;
        private long lastReceiveTime;
        private ConcurrentQueue<Request> sendQueue = new ConcurrentQueue<Request>();

        public event EventHandler Connected;
        public event EventHandler<Request> Receive;
        public event EventHandler Disconnected;
        protected virtual void OnConnected()
        {
            Connected?.Invoke(this, EventArgs.Empty);
        }
        protected virtual void OnReceive(Request e)
        {
            Receive?.Invoke(this, e);
        }
        protected virtual void OnDisconnected()
        {
            Disconnected?.Invoke(this, EventArgs.Empty);
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


        public async Task Connect(IPAddress serverV4, IPAddress serverV6, int port)
        {
            IPAddress server = null;
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

            if (server != null)
            {
                logger.LogInformation("Connected to {0}:{1}", server.ToString(), port);
                isConnected = true;
                lastReceiveTime = CurrentTime;
            }
        }

        public void Disconnect(string reason)
        {
            logger.LogWarning("Disconnected for " + reason);
            disconnect();
        }

        private async void mainLoop()
        {
            while (true)
            {
                long current = CurrentTime;
                long timeout = current - lastReceiveTime;
                if (timeout > timeoutSeconds)
                {
                    Disconnect("Timed out");
                    return;
                }
                if (!tcpClient.Connected)
                {
                    Disconnect("Disconnect");
                }
                await receive();
                await send();
                Thread.Sleep(200);
            }
        }

        public void Disconnect()
        {
            logger.LogInformation("Disconnected");
            disconnect();
        }

        private void disconnect()
        {
            sendRequest(RequestType.User_Logout, null);
            isConnected = false;
            tcpClient.Close();
        }

        private async Task receive()
        {
            if (!isConnected)
            {
                return;
            }

            var stream = tcpClient.GetStream();
            var str = "";
            if (stream.CanRead)
            {
                byte[] buffer = new byte[4096];
                var builder = new StringBuilder();
                int toRead = 0;
                while (stream.DataAvailable)
                {
                    toRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    builder.AppendFormat("{0}", Encoding.UTF8.GetString(buffer, 0, toRead));
                }

                str = builder.ToString().Trim();
                if (string.IsNullOrEmpty(str))
                {
                    return;
                }
                logger.LogInformation("Receive: {0}", str);
                var data = JsonSerializer.Deserialize<Request>(str);
                if (data != null)
                {
                    handleIncoming(data);
                }
            }
        }

        private async Task send()
        {
            if (!isConnected)
            {
                return;
            }

            if (sendQueue.TryDequeue(out var request))
            {
                request.SendTime = CurrentTime;
                var token = request.GetToken();
                request.Token = token;
                var json = JsonSerializer.Serialize(request).Trim();

                if (!string.IsNullOrEmpty(json))
                {
                    var stream = tcpClient.GetStream();
                    var writer = new StreamWriter(stream);
                    await writer.WriteAsync(json + "\n");
                    await writer.FlushAsync();
                }
            }
        }

        private void pong()
        {
            sendRequest(RequestType.Pong, null);
        }

        private void handleIncoming(Request request)
        {
            lastReceiveTime = CurrentTime;
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

        private void sendRequest(RequestType type, IDictionary<string, object> payload)
        {
            if ((!isAuth || !isConnected) && type != RequestType.User_Login && type != RequestType.User_Register)
            {
                return;
            }

            var request = new Request
            {
                Payload = payload,
                Type = type
            };
            sendQueue.Enqueue(request);
        }
    }
}
