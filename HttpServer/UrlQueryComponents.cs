using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace HttpServer
{
    public struct UrlQueryComponents
    {
        public string Method;
        public Dictionary<string, string> Parameters;

        public UrlQueryComponents(string query)
        {
            Method = null;

            if (!string.IsNullOrEmpty(query) && query.Contains('?'))
            {
                int pos = query.IndexOf('?');
                if (pos > 0)
                {
                    Method = query.Substring(0, pos);
                    Parameters = UrlUtility.GetParameters(query).ToDictionary(n => n.Key, n => n.Value);
                }
                else
                {
                    Method = query;
                    Parameters = new Dictionary<string, string>();
                }
            }
            else
            {
                Method = query;
                Parameters = new Dictionary<string, string>();
            }
        }
    }
}
