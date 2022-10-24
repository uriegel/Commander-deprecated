using System;
using System.Windows;
using HttpServer;

namespace Commander
{
    public partial class App : Application
    {
        public App()
        {
            var root = Environment.GetCommandLineArgs().Length > 1 && Environment.GetCommandLineArgs()[1] == "-webroot"
                ? @"..\..\..\webroot" 
                : ".";
            Configuration configuration = new Configuration
            {
                Webroot = root,
                Port = 9865,
            };
            configuration.Extensions.Add(new HttpServer.Extension("/Commander", new Extension()));
            Server server = new Server(configuration);
            server.Start();
        }
    }
}
