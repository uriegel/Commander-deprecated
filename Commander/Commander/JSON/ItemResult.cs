using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Commander;

[DataContract]
public class ItemResult
{
    internal ItemResult(string currentDirectory, IOItem[] items)
    {
        this.items = items;
        this.currentDirectory = currentDirectory;
        this.resultID = Interlocked.Increment(ref lastResultID);
        AccessTime = DateTime.Now;
    }

    [DataMember]
    public int resultID;

    [DataMember]
    public string currentDirectory;

    [DataMember]
    public IOItem[] items;

    public DateTime AccessTime;

    static int lastResultID;
}

