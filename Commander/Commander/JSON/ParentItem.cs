using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

[DataContract]
class ParentItem : IOItem
{
    public ParentItem(string parent)
        : base("images/parentfolder.png", "..", DateTime.Now, false)
    {
        this.parent = parent;
    }

    [DataMember]
    public string parent;
}

