using ChinoIM.Common.Helpers;
using ChinoIM.Common.Serialization;
using Microsoft.Extensions.Logging;
using System;

namespace ChinoIM.Common.Network
{
    public abstract class IncomingHandler<T, U> where T : ISerializable where U : Client<T, U>
    {
        public U Client { get; protected set; }
        public Predicate<T> Condition { get; protected set; }

        protected ILogger logger;

        public IncomingHandler(U client) : this(client, null) { }

        public IncomingHandler(U client, Predicate<T> condition)
        {
            Client = client;
            Condition = condition;
            logger = LogManager.CreateLogger(GetType());
        }

        public bool Test(T obj)
        {
            if (Condition == null)
            {
                logger.LogError("Condition is null, returing false.");
            }
            return Condition.Invoke(obj);
        }

        public abstract void HandleIncoming(T request);
    }
}
