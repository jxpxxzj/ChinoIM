using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization;

namespace ChinoIM.Common.Network
{
    public class Connection<T> : Connection where T: ISerializable
    {
        public IFormatter Formatter { get; set; }

        public Connection(TcpClient tcpClient, IFormatter formatter) : base(tcpClient)
        {
            init(formatter);
        }

        public Connection(TcpClient tcpClient, int timeoutSeconds, IFormatter formatter) : base(tcpClient, timeoutSeconds)
        {
            init(formatter);
        }

        public Connection(TcpClient tcpClient, int timeoutSeconds, Guid sessionId, IFormatter formatter) : base(tcpClient, timeoutSeconds, sessionId)
        {
            init(formatter);
        }

        protected void init(IFormatter formatter)
        {
            Formatter = formatter;
        }

        public new event EventHandler<T> Received;

        protected override void OnReceived(string request)
        {
            using (var memStream = new MemoryStream())
            {
                var writer = new StreamWriter(memStream);
                writer.Write(request);

                var obj = (T)Formatter.Deserialize(memStream);
                Received?.Invoke(this, obj);
            }           
        }

        public void SendRequest(T obj)
        {
            if (obj != null)
            {
                using (var memStream = new MemoryStream())
                {
                    Formatter.Serialize(memStream, obj);
                    var reader = new StreamReader(memStream);
                    var str = reader.ReadToEnd();
                    SendRequest(str);
                }
            }
        }
    }
}
