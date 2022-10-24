using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

[DataContract]
public class IOItemExtension
{
    [DataMember]
    public int index;

    [DataMember]
    public string version;

    [DataMember(Name = "dateTimeString")]
    string _dateTimeString;

    [DataMember(Name = "dateTime")]
    string _dateTime { get; set; }

    // This attribute prevents the ReturnDate property from being serialised.
    [IgnoreDataMember]
    // This property is used by your code.
    public DateTime dateTime
    {
        // Replace "o" with whichever DateTime format specifier you need.
        get
        {
            if (string.IsNullOrEmpty(_dateTime))
                return new DateTime();
            return DateTime.ParseExact(_dateTime, "o", CultureInfo.InvariantCulture);
        }
        set
        {
            _dateTime = value.ToString("o");
            _dateTimeString = value.ToString("dd.MM.yyy HH:mm");
        }
    }
}
