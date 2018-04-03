using ChinoIM.Common.Serialization;
using System;

namespace ChinoIM.Common.Network
{
    public abstract class IncomingHandler<T> where T : ISerializable
    {
        public Client<T> Client { get; protected set; }
        public Predicate<T> Condition { get; protected set; }

        public IncomingHandler(Client<T> client, Predicate<T> condition)
        {
            Client = client;
            Condition = condition; 
        }

        public bool Test(T obj)
        {
            return Condition.Invoke(obj);
        }

        public abstract bool HandleIncoming(T request);
    }
}
