using ChinoIM.Common.Helpers;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ChinoIM.Server
{
    public class TcpClientListener : IDisposable
    {
        private ILogger logger;
        private TcpListener tcpListener;
        public IPAddress IPAddress { get; protected set; }
        public int Port { get; protected set; }
        public bool IsRunning { get; protected set; } = false;

        public string Info { get; set; }

        public event EventHandler<TcpClient> TcpClientAccepted;

        protected virtual void OnTcpClientAccepted(TcpClient client)
        {
            TcpClientAccepted?.Invoke(this, client);
        }
        public TcpClientListener(IPAddress address, int port, string info = "")
        {
            IPAddress = address;
            Port = port;
            Info = info;
            tcpListener = new TcpListener(address, port);
            logger = LogManager.CreateLogger<TcpClientListener>(ToString());
        }


        public void Start()
        {
            logger.LogInformation("Listening on {0}:{1} for {2} connections...", IPAddress.ToString(), Port, Info);
            tcpListener.Start();
            IsRunning = true;
            Task.Run(() => listeningConnection());
        }

        public void Stop()
        {
            IsRunning = false;
            tcpListener.Stop();
        }

        private async Task listeningConnection()
        {
            while (true)
            {
                if (!IsRunning)
                {
                    return;
                }
                TcpClient tcpClient = null;
                try
                {
                    tcpClient = await tcpListener.AcceptTcpClientAsync();
                }
                catch (Exception e)
                {
                    logger.LogError("Listener error: {0}", e.ToString());
                    break;
                }

                if (tcpClient != null)
                {
                    logger.LogInformation("TcpClient accepted from " + Info);
                    OnTcpClientAccepted(tcpClient);
                }
            }
        }

        public override string ToString()
        {
            var addressFormat = string.Format(" @ {0}:{1}", IPAddress.ToString(), Port.ToString());

            var str = string.IsNullOrEmpty(Info) ? addressFormat : string.Format("{0}({1})", addressFormat, Info);

            return str;
        }

        #region IDisposable Support
        private bool disposedValue = false; 

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Stop();
                }
                disposedValue = true;
            }
        }

        ~TcpClientListener() {
            Dispose(false);
        }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
