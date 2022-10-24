using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace HttpServer
{
    public class Extension
    {
        #region Properties

        public string Url { get; private set; }

        #endregion

        #region Constructor

        public Extension(string url, string path, string typeInfo)
        {
            Url = url;
            this.path = path;
            this.typeInfo = typeInfo;
        }

        public Extension(string url, IExtension extension)
        {
            Url = url;
            this.extension = extension;
        }

        #endregion

        #region Methods

        public bool Request(ISession session, Method method, string path, string query)
        {
            if (extension == null)
                CreateExtension();
            if (session.CheckWsUpgrade())
            {
                session.UpgradeToWebSocket();
                extension.InitializeWebSocket(session);
                return false;
            }

            UrlQueryComponents urlQuery = new UrlQueryComponents(query);
            extension.Request(session, method, path, urlQuery);
            return true;
        }

        void CreateExtension()
        {
            if (!path.Contains(":"))
                path = Path.Combine(Directory.GetCurrentDirectory(), path);
            Assembly assi = Assembly.LoadFile(path);
            Type type = assi.GetType(typeInfo);
            extension = assi.CreateInstance(type.FullName) as IExtension;
        }

        #endregion

        #region Fields

        IExtension extension;
        string path;
        string typeInfo;

        #endregion
    }
}
