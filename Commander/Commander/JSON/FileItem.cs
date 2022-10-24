using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Commander;

[DataContract]
class FileItem : IOItem
{
    public FileItem(string name, string fullname, string extension, DateTime dateTime, long fileSize, System.IO.FileAttributes attributes)
        : base(ImageUrl.Get(name, extension, fullname), name, dateTime, FileAttributes.IsHidden(attributes))
    {
        this.fullname = fullname;
        this.fileSize = fileSize;
    }

    internal string fullname;

    [DataMember]
    public long fileSize;

    internal string Extension
    {
        get
        {
            int pos = name.LastIndexOf('.');
            if (pos == -1)
                return "";
            return name.Substring(pos);
        }
    }
}

