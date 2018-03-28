using ChinoIM.Common.Enums;
using ChinoIM.Common.Helpers;
using ChinoIM.Common.Models;
using ChinoIM.Common.Requests;
using ChinoIM.Server.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChinoIM.Server
{
    public class Client
    {
        public static int TimeoutSeconds = 90;
        public User User { get; set; }

        public Guid ClientID { get; protected set; }

        public TcpClient TcpClient { get; set; }

        private bool isAuth = false;
        private bool isKilled = false;
        private bool pinging = false;
        private long lastPingTime = 0;
        private long lastReceiveTime = 0;

        private ILogger logger = LogManager.CreateLogger<Client>();

        private ConcurrentQueue<Request> sendQueue = new ConcurrentQueue<Request>();

        public Client(TcpClient tcpClient)
        {
            TcpClient = tcpClient;
            ClientID = Guid.NewGuid();
            lastReceiveTime = ChinoServer.CurrentTime;
        }

        public async Task Check(ChinoWorker worker)
        {
            if (isKilled)
            {
                return;
            }

            long current = ChinoServer.CurrentTime;
            long timeout = current - lastReceiveTime;
            if (timeout > TimeoutSeconds)
            {
                Kill("Timed out");
                return;
            }
            if (!TcpClient.Connected)
            {
                Kill("Disconnect");
            }

            // processOfflineMessage();
            await receive();
            ping();
            await send();
        }

        private async Task receive()
        {
            if (isKilled)
            {
                return;
            }

            var stream = TcpClient.GetStream();
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
            if (isKilled)
            {
                return;
            }

            if (sendQueue.TryDequeue(out var request))
            {
                var token = request.GetToken();
                request.Token = token;
                request.SendTime = ChinoServer.CurrentTime;
                var json = JsonSerializer.Serialize(request).Trim();
                logger.LogInformation("Send: {0}", json);

                if (!string.IsNullOrEmpty(json))
                {
                    var stream = TcpClient.GetStream();
                    var writer = new StreamWriter(stream);
                    await writer.WriteAsync(json + "\n");
                    await writer.FlushAsync();
                }
            }
        }

        private void authenticate(string uid, string password)
        {
            var result = true;

            if (result)
            {
                isAuth = true;
                User = new User()
                {
                    UID = long.Parse(uid),
                };
                pong();
            }

            var dict = new Dictionary<string, object>()
            {
                { "result", isAuth ? uid : 0.ToString() }
            };
            sendRequest(RequestType.User_LoginResult, dict);
        }

        private void ping()
        {
            long currentTime = ChinoServer.CurrentTime;
            if (!pinging && (currentTime - lastPingTime > 30))
            {
                sendPing();
            }
        }

        private void pong()
        {
            pinging = false;
        }

        private void sendPing()
        {
            pinging = true;
            lastPingTime = ChinoServer.CurrentTime;
            sendRequest(RequestType.Ping, null);
        }

        public void Kill(String reason)
        {
            isKilled = true;
            logger.LogInformation("{0} killed for {1}",ToString(), reason);
            ClientManager.UnregisterClient(this);
        }

        private void handleIncoming(Request request)
        {
            lastReceiveTime = ChinoServer.CurrentTime;
            if (!isAuth && (request.Type == RequestType.User_Login || request.Type == RequestType.User_Register))
            {
                switch (request.Type)
                {
                    //case User_Register:
                    //    long uid = UserService.Register(request.payload.get("username").toString(), request.payload.get("password").toString());
                    //    Authenticate(String.valueOf(uid), request.payload.get("password").toString());
                    //    break;
                    case RequestType.User_Login:
                        authenticate(request["UID"].ToString(), request["Password"].ToString());
                        break;
                }
            }
            if (isAuth)
            {
                switch (request.Type)
                {
                    case RequestType.Message:
                        long targetId = long.Parse(request["Target"].ToString());
                        var targetType = Enum.Parse<EndpointType>(request["TargetType"].ToString());
                        MessageService.SendMessage(User.UID, targetId, targetType, request["Content"].ToString(), bool.Parse(request["UseEscape"].ToString()));
                        break;
                    case RequestType.User_Logout:
                        Kill("logout");
                        break;
                    case RequestType.Pong:
                        pong();
                        break;
                }
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
            if ((!isAuth || isKilled) && type != RequestType.Ping && type != RequestType.User_LoginResult)
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

        public override string ToString()
        {
            if (User != null)
            {
                return string.Format("Client[{0}] {1}(2) ", ClientID, User.Username, User.UID);
            }
            return string.Format("Client[{0}]", ClientID);
        }
    }
}
