using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Commander
{
    class ServiceStateProcessor
    {
        public ServiceStateProcessor(ServiceController[] services, WebSocket webSocket)
        {
            this.services = services;
            this.webSocket = webSocket;
            timer = new Timer(300);
            timer.Elapsed += Timer_Elapsed; 
            timer.Start();
        }

        void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach (var service in services)
            {
                var status = service.Status;
                service.Refresh();
                if (status != service.Status)
                {
                    ServiceItem si = new ServiceItem(service);
                    webSocket.SendServiceItemChanged(si);
                }
            }
        }

        public void Stop()
        {
            timer.Stop();
        }

        WebSocket webSocket;
        ServiceController[] services;
        Timer timer;
    }
}
