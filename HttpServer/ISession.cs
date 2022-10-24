using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace HttpServer
{
    public interface ISession
    {
        IServer Server { get; }
        Dictionary<string, KeyValuePair<string, string>> Headers { get; }
        void SendHtmlString(string html);
        void SendOK(string responseText);
        void SendError(string htmlHead, string htmlBody, int errorCode, string errorText);
        void SendJson(object jsonResult);
        void SendJsonString(string json);
        void SendXml(XElement xml);
        void SendStream(Stream stream, string contentType, string lastModified = null, bool noCache = false);
        void ReadStream(Stream stream);
        byte[] Get();
        T GetJson<T>();
        string ReadString();
        bool CheckWsUpgrade();
        void UpgradeToWebSocket();
        IAsyncResult WsBeginReceive(byte[] buffer, AsyncCallback callback, object state);
        int WsEndReceive(IAsyncResult ar);
        string DecodeWSBuffer(byte[] buffer);
        void WsSend(string text);
        T DecodeJson<T>(string json);
        T DecodeJson<T>(Stream stream);
        string EncodeJson<T>(T obj);
    }
}
