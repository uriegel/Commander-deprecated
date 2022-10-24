using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace HttpServer
{
    public class Configuration
    {
        #region Properties

        public string Webroot
        {
            get
            {
                if (string.IsNullOrEmpty(_Webroot))
                    _Webroot = Directory.GetCurrentDirectory();
                return _Webroot;
            }
            set { _Webroot = value; }
        }
        string _Webroot;

        public string PhpBin { get; set; }
        public List<Extension> Extensions { get; set; }
        public bool IsTlsEnabled { get; set; }
        public int Port
        {
            get
            {
                if (_Port == 0)
                    _Port = IsTlsEnabled ? 443 : 80;
                return _Port;
            }
            set { _Port = value; }
        }
        int _Port;

        public string[] AppCaches { get; set; }
        public string[] NoCacheFiles
        {
            get
            {
                if (_NoCacheFiles == null)
                    _NoCacheFiles = InitializeAppCaches();
                return _NoCacheFiles;
            }
        }
        string[] _NoCacheFiles;

        public string TlsDomainName
        {
            get
            {
                //                if (_TlsDomainName == null)
                return _TlsDomainName;
            }
            set { _TlsDomainName = value; }
        }
        string _TlsDomainName;

        #endregion

        #region Constructor

        public Configuration()
        {
            Extensions = new List<Extension>();
        }

        #endregion

        #region Methods

        string[] InitializeAppCaches()
        {
            var ncf = Enumerable.Empty<string>();
            if (AppCaches == null)
                return new string[0];
            foreach (var appcache in AppCaches)
            {
                var file = Path.Combine(Webroot, appcache);
                var root = Path.GetDirectoryName(file);
                using (StreamReader sr = new StreamReader(file))
                {
                    string content = sr.ReadToEnd();
                    int start = content.IndexOf("CACHE:\r\n") + 8;
                    int stop = content.IndexOf("\r\n\r\n", start);
                    string caches = content.Substring(start, stop - start);
                    var cacheFiles = caches.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    ncf = ncf.Concat(cacheFiles.Select(n => Path.Combine(root, n.Replace('/', '\\')).ToLower()));
                }
            }
            return ncf.ToArray();
        }

        #endregion
    }
}
