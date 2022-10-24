using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceProcess;
using System.Text;
using Commander;
using Microsoft.Win32;

[DataContract]
class ServiceItem : IOItem
{
    [DataMember]
    public string serviceName;
    [DataMember]
    public int startType;
    [DataMember]
    public string status;


    public ServiceItem(ServiceController sc)
        : base("images/serviceStopped.png", sc.DisplayName, DateTime.Now, false)
    {
        serviceName = sc.ServiceName;
        RegistryKey serviceKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\" + serviceName);
        if (serviceKey != null)
            startType = (int)serviceKey.GetValue("Start");
        switch (sc.Status)
        {
            case ServiceControllerStatus.Running:
                status = "rennt";
                imageUrl = "images/service.png";
                break;
            case ServiceControllerStatus.Stopped:
                status = "aus";
                break;
            case ServiceControllerStatus.StartPending:
                status = "läuft an";
                break;
            case ServiceControllerStatus.StopPending:
                status = "fährt runter";
                break;
        }
    }
}

