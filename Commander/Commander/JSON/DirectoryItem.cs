using Commander;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

[DataContract]
class DirectoryItem : IOItem
{
    public DirectoryItem(string name, string fullName, DateTime dateTime, System.IO.FileAttributes attributes)
        : base("images/Folder.png", name, dateTime, FileAttributes.IsHidden(attributes))
    {
        this.fullName = fullName;
    }
    [DataMember]
    public string fullName;
}

