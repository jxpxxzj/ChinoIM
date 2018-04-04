using ChinoIM.Common.Enums;
using ChinoIM.Common.Helpers;
using ChinoIM.Common.Models;
using ChinoIM.Common.Network;
using ChinoIM.Server.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace ChinoIM.Server
{
    public class ChinoClient : Client<Request, ChinoClient>
    {
        public User User { get; set; }

        public bool isAuth = false;
        private bool pinging = false;
        private long lastPingTime = 0;

        private ILogger logger;


        public ChinoClient(TcpClient tcpClient)
        {
            SetConnection(tcpClient, new JsonSerializer<Request>());
            Connection.MidUpdate += Connection_MidUpdate;
            logger = LogManager.CreateLogger<ChinoClient>(Connection.ToString(string.Empty));
        }

        private void Connection_MidUpdate(object sender, EventArgs e)
        {
            ping();
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
            long currentTime = TimeService.CurrentTime;
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
            lastPingTime = TimeService.CurrentTime;
            sendRequest(RequestType.Ping, null);
        }

        public override void HandleIncoming(Request request)
        {
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
                        Connection.Disconnect("logout");
                        break;
                    case RequestType.Pong:
                        pong();
                        break;
                }
            }
        }

        public override void SendRequest(Request request)
        {
            if (request != null)
            {
                sendRequest(request.Type, request.Payload);
            }
        }

        private void sendRequest(RequestType type, IDictionary<string, object> payload)
        {
            if ((!isAuth || Connection.IsConnected) && type != RequestType.Ping && type != RequestType.User_LoginResult)
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
