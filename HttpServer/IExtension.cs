using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HttpServer
{
    public interface IExtension
    {
        void Request(ISession session, Method method, string path, UrlQueryComponents urlQuery);

        void InitializeWebSocket(ISession session);
    }
}
