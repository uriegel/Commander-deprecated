using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Timers;
using HttpServer;

namespace Commander
{
    class WebSocket
    {
        #region Constructor 

        public WebSocket(ISession session)
        {
            this.session = session;
            byte[] buffer = new byte[1024];
            session.WsBeginReceive(buffer, WsAsyncCallback, buffer);
        }

        #endregion

        #region Methods

        public void SendServiceItemChanged(ServiceItem serviceItem)
        {
            var c = new Json.ServiceItemChanged { method = "serviceItem", serviceItem = serviceItem  };
            session.WsSend(session.EncodeJson<Json.ServiceItemChanged>(c));
        }

        public void SendCopyCommand()
        {
            var c = new Json.Command { method = "copy" };
            session.WsSend(session.EncodeJson<Json.Command>(c));
        }

        public void SendMoveCommand()
        {
            var c = new Json.Command { method = "move" };
            session.WsSend(session.EncodeJson<Json.Command>(c));
        }

        public void SendDeleteCommand()
        {
            var c = new Json.Command { method = "delete" };
            session.WsSend(session.EncodeJson<Json.Command>(c));
        }

        public void SendRefreshCommand()
        {
            var c = new Json.Command { method = "refresh" };
            session.WsSend(session.EncodeJson<Json.Command>(c));
        }
        
        void WsAsyncCallback(IAsyncResult ar)
        {
            try
            {
                byte[] buffer = ar.AsyncState as byte[];
                int read = session.WsEndReceive(ar);
                if (read > 0)
                {
                    string json = session.DecodeWSBuffer(buffer);
                    if (json != null)
                    {
                        string method = parseJson(json, "method");
                        switch (method)
                        {
                            case "initialize":
                                CommanderContext.Initialize(this);
                                break;
                        }
                    }
                }
                session.WsBeginReceive(buffer, WsAsyncCallback, buffer);
            }
            catch (Exception)
            {
            }
        }

        string parseJson(string json, string key)
        {
            string searchstring = "\"" + key + "\":";
            int pos = json.IndexOf(searchstring);
            if (pos == -1)
                return null;
            pos += searchstring.Length;
            int posstart = json.IndexOf("\"", pos) + 1;
            if (posstart == 0)
                return null;
            int posend = json.IndexOf("\"", posstart);
            if (posend == -1)
                return null;
            return json.Substring(posstart, posend - posstart);
        }

        #endregion

        #region Fields

        ISession session;

        #endregion
    }
}
