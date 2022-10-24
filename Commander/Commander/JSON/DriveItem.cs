using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

[DataContract]
class DriveItem : IOItem
{
    public DriveItem(string drive, string icon = "images/drive.png")
        : base(icon, drive, DateTime.Now, false)
    {
    }
}

