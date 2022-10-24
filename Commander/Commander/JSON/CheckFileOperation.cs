using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

[DataContract]
public class CheckFileOperation
{
    [DataMember]
    public string operation;
    [DataMember]
    public string sourceDir;
    [DataMember]
    public string targetDir;
    [DataMember]
    public IOItem[] items;
}

[DataContract]
class CheckFileOperationResult
{
    [DataMember(EmitDefaultValue = false)]
    public OperationItem[] conflictItems;
    [DataMember]
    public string result;
    [DataMember]
    public int id;
}
