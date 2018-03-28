using ChinoIM.Common.Enums;
using ChinoIM.Common.Helpers;
using ChinoIM.Common.Requests;
using System;

namespace ChinoIM.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new ChinoClient();
            client.Receive += Client_Receive;
            while(true)
            {
                var str = Console.ReadLine();
                var request = JsonSerializer.Deserialize<Request>(str);
                client.SendRequest(request);
                if (request == null)
                {
                    continue;
                }
                if(request.Type == RequestType.User_Logout)
                {
                    break;
                }
            }
            Console.WriteLine("Client disconnected.");
            Console.ReadLine();
        }

        private static void Client_Receive(object sender, Request e)
        {
            // Console.WriteLine(JsonSerializer.Serialize(e));
        }
    }
}
