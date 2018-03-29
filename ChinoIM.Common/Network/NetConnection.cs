﻿using ChinoIM.Common.Helpers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChinoIM.Common.Network
{
    public class NetConnection
    {

        public NetConnection(TcpClient tcpClient)
        {
            init(tcpClient, TimeoutLimit, Guid.NewGuid());
        }
        public NetConnection(TcpClient tcpClient, int timeoutSeconds)
        {
            init(tcpClient, timeoutSeconds, Guid.NewGuid());
        }
        public NetConnection(TcpClient tcpClient, int timeoutSeconds, Guid sessionId)
        {
            init(tcpClient, timeoutSeconds, sessionId);
        }
        ~NetConnection()
        {
            Disconnect("Deconstruction");
        }

        private void init(TcpClient tcpClient, int timeoutSeconds, Guid sessionId)
        {
            TcpClient = tcpClient;
            TimeoutLimit = timeoutSeconds;
            lastReceiveTime = CurrentTime;
            SessionID = sessionId;
            EndPoint = (IPEndPoint)TcpClient.Client.RemoteEndPoint;
            logger = LogManager.CreateLogger<NetConnection>(ToString());
        }

        private static DateTime unixTime = new DateTime(1970, 1, 1);
        public static long CurrentTime
        {
            get
            {
                var ts = DateTime.Now - unixTime;
                return (long)ts.TotalSeconds;
            }
        }

        private ILogger logger;

        public TcpClient TcpClient { get; protected set; }
        public int TimeoutLimit { get; set; } = 90;
        public Guid SessionID { get; set; }
        public IPEndPoint EndPoint { get; protected set; }

        public bool isConnected
        {
            get => TcpClient.Connected;
        }

        public long LastReceiveTimeAgo { get; protected set; }
        private long lastReceiveTime = 0;
        private ConcurrentQueue<string> sendQueue = new ConcurrentQueue<string>();

        public event EventHandler<string> BeforeSend;
        public event EventHandler<string> AfterSend;
        public event EventHandler<NetworkStream> BeforeReceive;
        public event EventHandler<string> AfterReceive;
        public event EventHandler<string> Received;
        public event EventHandler<string> Disconnected;
        public event EventHandler BeforeUpdate;
        public event EventHandler MidUpdate;
        public event EventHandler AfterUpdate;

        protected virtual void OnBeforeUpdate()
        {
            BeforeUpdate?.Invoke(this, EventArgs.Empty);
        }
        protected virtual void OnAfterUpdate()
        {
            AfterUpdate?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnMidUpdate()
        {
            MidUpdate?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnBeforeSend(string data)
        {
            BeforeSend?.Invoke(this, data);
        }
        protected virtual void OnAfterSend(string data)
        {
            AfterSend?.Invoke(this, data);
        }
        protected virtual void OnBeforeReceive(NetworkStream stream)
        {
            BeforeReceive?.Invoke(this, stream);
        }
        protected virtual void OnAfterReceive(string data)
        {
            AfterReceive?.Invoke(this, data);
        }

        protected virtual void OnReceived(string data)
        {
            Received?.Invoke(this, data);
        }

        protected virtual void OnDisconnected(string reason)
        {
            Disconnected?.Invoke(this, reason);
        }

        public async Task Update()
        {
            if (!isConnected)
            {
                Disconnect("Disconnect");
            }

            checkTimeout();

            OnBeforeUpdate();
            await receive();
            OnMidUpdate();
            await send();
            OnAfterUpdate();
        }

        private void checkTimeout()
        {
            long current = CurrentTime;
            long timeout = current - lastReceiveTime;
            LastReceiveTimeAgo = timeout;
            if (timeout > TimeoutLimit)
            {
                Disconnect("Timed out");
                return;
            }
        }

        private async Task receive()
        {
            if (!isConnected)
            {
                return;
            }

            var stream = TcpClient.GetStream();
            var str = string.Empty;
            if (stream.CanRead)
            {
                byte[] buffer = new byte[4096];
                var builder = new StringBuilder();
                int toRead = 0;
                try
                {
                    bool invoked = false;
                    while (stream.DataAvailable)
                    {
                        if (!invoked)
                        {
                            OnBeforeReceive(stream);
                            invoked = true;
                        }
                        toRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                        builder.Append(Encoding.UTF8.GetString(buffer, 0, toRead));
                    }
                    
                }
                catch
                {
                    Disconnect("Disconnect");
                }

                str = builder.ToString().Trim();
                if (!string.IsNullOrEmpty(str))
                {
                    OnAfterReceive(str);
                    lastReceiveTime = CurrentTime;
                    logger.LogInformation("Receive: {0}", str);
                    OnReceived(str);
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
                logger.LogInformation("Send: {0}", request);

                if (!string.IsNullOrEmpty(request))
                {
                    var stream = TcpClient.GetStream();
                    var writer = new StreamWriter(stream);
                    if (stream.CanWrite)
                    {
                        OnBeforeSend(request);
                        try
                        {
                            await writer.WriteAsync(request + "\n");
                            await writer.FlushAsync();
                            OnAfterSend(request);
                        }
                        catch
                        {
                            Disconnect("Disconnect");
                        }
                    }
                }
            }
        }
        public void SendRequest(string request)
        {
            if (!isConnected)
            {
                return;
            }
            if (!string.IsNullOrEmpty(request))
            {
                sendQueue.Enqueue(request);
            }
        }

        public void Disconnect(string reason = null)
        {
            TcpClient.Close();
            if (string.IsNullOrEmpty(reason))
            {
                logger.LogInformation("Disconnected");
            }
            else
            {
                logger.LogWarning("Disconnected for " + reason);
            }
            OnDisconnected(reason);
        }

        public override string ToString()
        {
            return ToString(string.Empty);
        }

        public string ToString(string prefix)
        {
            var baseStr = string.Format("{0}[{1} @ {2}:{3}]", prefix, SessionID, EndPoint.Address, EndPoint.Port);
            return baseStr;
        }
    }
}