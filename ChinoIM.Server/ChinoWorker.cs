using ChinoIM.Common.Helpers;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace ChinoIM.Server
{
    public class ChinoWorker
    {
        public int ID { get; protected set; }
        private ILogger logger;

        public ChinoWorker(int id)
        {
            ID = id;
            logger = LogManager.CreateLogger<ChinoWorker>(id.ToString());
        }

        public async Task DoWork() {
            var client = ClientManager.GetClientForProcessing();
            if (client != null)
            {
                await client.Check(this);
                ClientManager.AddClientForProcessing(client);
                return;
            }
        }
    }
}
