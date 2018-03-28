namespace ChinoIM.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new ChinoServer();
            server.Start();
        }
    }
}
