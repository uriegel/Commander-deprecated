using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

[DataContract]
public class ExtendedInfosResult
{
    [DataMember]
    public IOItemExtension[] items;
}
