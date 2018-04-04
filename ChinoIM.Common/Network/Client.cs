using ChinoIM.Common.Serialization;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ChinoIM.Common.Network
{
    public abstract class Client<T, U> : IUpdateable where T : ISerializable where U: Client<T,U>
    {
        public Connection<T> Connection { get; protected set; }
        protected List<IncomingHandler<T, U>> IncomingHandlers = new List<IncomingHandler<T, U>>();

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

        public Client()
        {
            
        }

        public virtual void SetConnection(TcpClient tcpClient, ISerializer<T> serializer)
        {
            var connection = new Connection<T>(tcpClient, serializer);
            SetConnection(connection);
        }

        public virtual void SetConnection(Connection<T> connection)
        {
            Connection = connection;
            Connection.Received += Connection_Received;
            Connection.Disconnected += Connection_Disconnected;
            OnConnected();
        }

        protected virtual void Connection_Disconnected(object sender, string e)
        {
            OnDisconnected(e);
        }

        protected virtual void Connection_Received(object sender, T e)
        {
            if (!Connection.IsConnected)
            {
                return;
            }
            HandleIncoming(e);
        }
        ~Client()
        {
            Connection.Disconnect();
            Connection.Dispose();
        }

        public abstract void HandleIncoming(T request);
        public virtual void SendRequest(T request)
        {
            Connection.SendRequest(request);
        }

        public virtual void ProcessHandler(T request)
        {
            IncomingHandlers.FindAll(t => t.Test(request)).ForEach(t => t.HandleIncoming(request));
        }

        public virtual void AddHandler(IncomingHandler<T, U> handler)
        {
            IncomingHandlers.Add(handler);
        }
        public virtual void RemoveHandler(Predicate<IncomingHandler<T, U>> predicate)
        {
            IncomingHandlers.RemoveAll(predicate);
        }

        public virtual async Task<bool> Update()
        {
            return await Connection.Update();
        }
    }
}
