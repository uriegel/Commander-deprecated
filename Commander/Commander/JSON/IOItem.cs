using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

[DataContract]
[KnownType(typeof(DriveItem))]
[KnownType(typeof(ParentItem))]
[KnownType(typeof(FileItem))]
[KnownType(typeof(DirectoryItem))]
[KnownType(typeof(ServiceItem))]
public class IOItem
{
    public IOItem(string name)
    {
        this.name = name;
    }

    public IOItem(string imageUrl, string name, DateTime dateTime, bool isHidden)
    {
        this.name = name;
        this.isHidden = isHidden;
        SetDateTime(dateTime);
        this.imageUrl = imageUrl;
    }

    [DataMember]
    public string imageUrl;

    [DataMember]
    public string name;

    [DataMember]
    public string dateTimeString;

    [DataMember]
    public string dateTime;

    [DataMember]
    public bool isHidden;

    internal void SetDateTime(DateTime dt)
    {
        dateTime = dt.ToString("o");
        dateTimeString = dt.ToString("dd.MM.yyy HH:mm");
    }
}

