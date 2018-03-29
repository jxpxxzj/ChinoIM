using ChinoIM.Common.Helpers;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace ChinoIM.Server
{
    public class ChinoWorker
    {
        public int ID { get; protected set; }
        private ILogger logger;

        private static int TotalWorkers;
        private bool decommissioned = false;

        public ChinoWorker(int id)
        {
            ID = id;
            logger = LogManager.CreateLogger<ChinoWorker>(id.ToString());
            Interlocked.Increment(ref TotalWorkers);
        }

        ~ChinoWorker()
        {
            Interlocked.Decrement(ref TotalWorkers);
        }

        public async Task DoWork()
        {
            if (decommissioned)
            {
                return;
            }
            var client = ClientManager.GetClientForProcessing();
            if (client != null)
            {
                try
                {
                    await client.Check(this);
                }
                catch
                {
                    throw;
                }
                
                ClientManager.AddClientForProcessing(client);
                return;
            }
        }

        internal void Recover()
        {
            decommissioned = false;
        }

        internal void Decommission()
        {
            decommissioned = true;
        }
    }
}
