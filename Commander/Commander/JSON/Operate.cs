using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

[DataContract]
public class Operate
{
    [DataMember]
    public int id;

    [DataMember]
    public bool ignoreConflicts;

    [DataMember(EmitDefaultValue = false)]
    public OperationItem[] conflictItems;
}

