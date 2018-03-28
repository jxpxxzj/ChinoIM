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
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChinoIM.Server
{
    public class Client
    {
        public static int TimeoutSeconds = 90;
        public User User { get; set; }

        public Guid SessionID { get; protected set; }

        public TcpClient TcpClient { get; set; }
        public IPEndPoint EndPoint { get; protected set; }

        public bool isAuth = false;
        public bool isKilled { get; set; } = false;
        private bool pinging = false;
        private long lastPingTime = 0;
        private long lastReceiveTime = 0;

        private ILogger logger;

        private ConcurrentQueue<Request> sendQueue = new ConcurrentQueue<Request>();

        public Client(TcpClient tcpClient)
        {
            TcpClient = tcpClient;
            EndPoint = (IPEndPoint)TcpClient.Client.RemoteEndPoint;
            SessionID = Guid.NewGuid();
            lastReceiveTime = ChinoServer.CurrentTime;
            logger = LogManager.CreateLogger<Client>(ToString(string.Empty));
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
                try
                {
                    while (stream.DataAvailable)
                    {
                        toRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                        builder.AppendFormat("{0}", Encoding.UTF8.GetString(buffer, 0, toRead));
                    }
                }
                catch
                {
                    Kill("Disconnect");
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
                    if (stream.CanWrite)
                    {
                        try
                        {
                            await writer.WriteAsync(json + "\n");
                            await writer.FlushAsync();
                        }
                        catch
                        {
                            Kill("Disconnect");
                        }
                    }
                }
            }
        }

        private void authenticate(string uid, string password)
        {
            var result = true;

            if (result)
            {
                isAuth = true;
                logger.LogInformation("Auth with {0}", uid);
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
            return ToString("Client");
        }

        public string ToString(string prefix)
        {
            var baseStr = string.Format("{0}[{1} @ {2}:{3}]", prefix, SessionID, EndPoint.Address, EndPoint.Port);
            if (User != null)
            {
                return string.Format("{0} {1}(2)", baseStr, User.Username, User.UID);
            }
            return baseStr;
        }
    }
}
