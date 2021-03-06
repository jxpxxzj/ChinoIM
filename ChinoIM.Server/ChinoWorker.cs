﻿using ChinoIM.Common.Helpers;
using Microsoft.Extensions.Logging;
using System;
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
                bool success = true;
                string msg = string.Empty;
                try
                {
                    long before = TimeService.CurrentTime;
                    success = await client.Update();
                    long after = TimeService.CurrentTime;
                    if (after - before > 100 && client is ChinoClient c)
                    {
                        logger.LogWarning("Slow client: {0}", c.ToString());
                    }
                }
                catch (Exception e)
                {
                    success = false;
                    msg = string.Format("{0}, removing client", e.ToString());
                }

                if (!success)
                {
                    logger.LogError(msg);
                    ClientManager.UnregisterClient(client);
                    return;
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
