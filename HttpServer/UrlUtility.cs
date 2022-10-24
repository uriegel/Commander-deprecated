using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace HttpServer
{
    public static class UrlUtility
    {
        #region Methods

        public static KeyValuePair<string, string>[] GetParameters(string urlParameterString)
        {
            MatchCollection mc = urlParameterRegex.Matches(urlParameterString);
            return mc.OfType<Match>().Select(n => new KeyValuePair<string, string>(n.Groups["key"].Value,
                Uri.UnescapeDataString(UnescapeSpaces(n.Groups["value"].Value)))).ToArray();
        }

        static string UnescapeSpaces(string uri)
        {
            return uri.Replace('+', ' ');
        }

        #endregion

        #region Fields

        static Regex urlParameterRegex = new Regex(@"(?<key>[^&?]*?)=(?<value>[^&?]*)", RegexOptions.Compiled);

        #endregion
    }
}
