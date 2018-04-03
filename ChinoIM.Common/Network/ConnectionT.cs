using ChinoIM.Common.Serialization;
using System;
using System.Net.Sockets;

namespace ChinoIM.Common.Network
{
    public class Connection<T> : Connection where T : ISerializable
    {
        public ISerializer<T> Formatter { get; set; }

        public Connection(TcpClient tcpClient, ISerializer<T> formatter) : base(tcpClient)
        {
            init(formatter);
        }

        public Connection(TcpClient tcpClient, int timeoutSeconds, ISerializer<T> formatter) : base(tcpClient, timeoutSeconds)
        {
            init(formatter);
        }

        public Connection(TcpClient tcpClient, int timeoutSeconds, Guid sessionId, ISerializer<T> formatter) : base(tcpClient, timeoutSeconds, sessionId)
        {
            init(formatter);
        }

        protected void init(ISerializer<T> formatter)
        {
            Formatter = formatter;
        }

        public new event EventHandler<T> Received;

        protected override void OnReceived(string request)
        {
            var obj = Formatter.Deserialize(request);
            Received?.Invoke(this, obj);
        }

        public void SendRequest(T obj)
        {
            if (obj != null)
            {
                var str = Formatter.Serialize(obj);
                SendRequest(str);
            }
        }
    }
}
