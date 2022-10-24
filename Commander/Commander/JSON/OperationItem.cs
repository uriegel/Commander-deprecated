using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Commander;
using System.IO;

[DataContract]
[KnownType(typeof(OperationFileItem))]
public class OperationItem
{
    public OperationItem(string imageUrl, string name, string subPath)
    {
        this.imageUrl = imageUrl;
        this.name = name;
        SubPath = subPath;
    }
    [DataMember]
    public string imageUrl;
    [DataMember]
    public string name { get; private set; }
    [DataMember]
    public string SubPath { get; private set; }
    public string NameWithSubPath
    {
        get { return SubPath != null ? Path.Combine(SubPath, name) : name; }
    }
}

[DataContract]
class OperationFileItem : OperationItem 
{
    public OperationFileItem(string name, string extension, string fullname, long sourceFileSize, string sourceDateTime, string subPath) : 
        base(ImageUrl.Get(name, extension, fullname), name, subPath)
    {
        this.extension = extension;
        this.sourceFileSize = sourceFileSize;
        SetSourceDateTime(sourceDateTime);
    //    this.sourceVersion = FileVersion.Get(fullname);
    }

    public OperationFileItem(FileItem fileItem) :
        base(fileItem.imageUrl, fileItem.name, null)
    {
        extension = fileItem.Extension;
        SetSourceDateTime(fileItem.dateTime);
        sourceFileSize = fileItem.fileSize;
    }

    [DataMember]
    public string extension { get; private set; }
    [DataMember]
    public string sourceVersion { get; set; }
    [DataMember]
    public string targetVersion { get; set; }
    [DataMember]
    public long sourceFileSize { get; set; }
    [DataMember]
    public long targetFileSize { get; set; }
    [DataMember]
    string sourceDateTimeString;
    [DataMember]
    string sourceDateTime;
    internal void SetSourceDateTime(string dt)
    {
        sourceDateTime = dt;
        DateTime dateTime = DateTime.ParseExact(dt, "o", CultureInfo.InvariantCulture); 
        sourceDateTimeString = dateTime.ToString("dd.MM.yyy HH:mm");
    }
    internal void SetSourceDateTime(DateTime dt)
    {
        sourceDateTime = dt.ToString("o");
        sourceDateTimeString = dt.ToString("dd.MM.yyy HH:mm");
    }

    [DataMember]
    string targetDateTimeString;
    [DataMember]
    string targetDateTime;

    internal void SetTargetDateTime(DateTime dt)
    {
        targetDateTime = dt.ToString("o");
        targetDateTimeString = dt.ToString("dd.MM.yyy HH:mm");
    }
}

class OperationDirectoryItem : OperationItem
{
    public OperationDirectoryItem(string name, string subPath)
        : base(null, name, subPath)
    {
    }
}
