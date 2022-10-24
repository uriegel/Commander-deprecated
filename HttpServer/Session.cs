using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Xml.Linq;

namespace HttpServer
{
    class Session : ISession
    {
        #region ISession Members

        public IServer Server { get; private set; }

        public Dictionary<string, KeyValuePair<string, string>> Headers { get; private set; }

        public void SendHtmlString(string html)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(html);
            string contentType = "text/html; charset=UTF-8";
            string headers = string.Format("HTTP/1.1 200 OK\r\nContent-Length: {0}\r\nETag: \"0815\"\r\nAccept-Ranges: bytes\r\nContent-Type: {1}\r\nKeep-Alive: timeout=5, max=99\r\nConnection: Keep-Alive\r\n\r\n", bytes.Length, contentType);
            byte[] vorspannBuffer = ASCIIEncoding.ASCII.GetBytes(headers);
            networkStream.Write(vorspannBuffer, 0, vorspannBuffer.Length);
            networkStream.Write(bytes, 0, bytes.Length);
        }

        public void SendOK(string responseText)
        {
            byte[] responseBytes = Encoding.UTF8.GetBytes(responseText);
            string headers = string.Format("HTTP/1.1 200 OK\r\nContent-Type: text/html; charset=UTF-8\r\nContent-Length: {0}\r\n\r\n", responseBytes.Length);
            byte[] vorspannBuffer = ASCIIEncoding.ASCII.GetBytes(headers);
            networkStream.Write(vorspannBuffer, 0, vorspannBuffer.Length);
            networkStream.Write(responseBytes, 0, responseBytes.Length);
        }

        public void SendError(string htmlHead, string htmlBody, int errorCode, string errorText)
        {
            string response = string.Format("<html><head>{0}</head><body>{1}</body</html>", htmlHead, htmlBody);
            byte[] responseBytes = Encoding.UTF8.GetBytes(response);
            string headers = string.Format("HTTP/1.1 {0} {1}\r\nContent-Length: {2}\r\nContent-Type: text/html; charset=UTF-8\r\n\r\n",
                errorCode, errorText, responseBytes.Length);
            byte[] vorspannBuffer = ASCIIEncoding.ASCII.GetBytes(headers);
            networkStream.Write(vorspannBuffer, 0, vorspannBuffer.Length);
            networkStream.Write(responseBytes, 0, responseBytes.Length);
        }

        public void SendJson(object jsonResult)
        {
            DataContractJsonSerializer jason = new DataContractJsonSerializer(jsonResult.GetType());
            MemoryStream memStream = new MemoryStream();
            jason.WriteObject(memStream, jsonResult);
            memStream.Position = 0;

            int contentLength = (int)memStream.Length;
            string headers = string.Format("HTTP/1.1 200 OK\r\nContent-Length: {0}\r\nContent-Type: application/json; charset=UTF-8\r\nCache-Control: no-cache,no-store\r\n\r\n", contentLength);
            byte[] vorspannBuffer = ASCIIEncoding.ASCII.GetBytes(headers);
            networkStream.Write(vorspannBuffer, 0, vorspannBuffer.Length);

            byte[] bytes = new byte[4000];
            while (contentLength > 0)
            {
                int read = memStream.Read(bytes, 0, Math.Min(bytes.Length, contentLength));
                if (read == 0)
                    return;
                contentLength -= read;
                networkStream.Write(bytes, 0, read);
            }
        }

        public void SendXml(XElement xml)
        {
            using (MemoryStream memStream = new MemoryStream())
            {
                using (System.Xml.XmlWriter xmlWriter = System.Xml.XmlWriter.Create(memStream,
                    new System.Xml.XmlWriterSettings { Encoding = new UTF8Encoding(false), OmitXmlDeclaration = true })) //ohne BOM speichern
                {
                    xml.Save(xmlWriter);
                }

                memStream.Position = 0;

                int contentLength = (int)memStream.Length;
                string headers = string.Format("HTTP/1.1 200 OK\r\nContent-Length: {0}\r\nContent-Type: text/xml; charset=UTF-8\r\n\r\n", contentLength);
                byte[] vorspannBuffer = ASCIIEncoding.ASCII.GetBytes(headers);
                networkStream.Write(vorspannBuffer, 0, vorspannBuffer.Length);
                byte[] bytes = new byte[4000];
                while (contentLength > 0)
                {
                    int read = memStream.Read(bytes, 0, Math.Min(bytes.Length, contentLength));
                    if (read == 0)
                        return;
                    contentLength -= read;
                    networkStream.Write(bytes, 0, read);
                }
            }
        }

        public void SendJsonString(string json)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(json);

            string headers = string.Format("HTTP/1.1 200 OK\r\nContent-Length: {0}\r\nContent-Type: application/json; charset=UTF-8\r\nCache-Control: no-cache,no-store\r\n\r\n", bytes.Length);
            byte[] vorspannBuffer = ASCIIEncoding.ASCII.GetBytes(headers);
            networkStream.Write(vorspannBuffer, 0, vorspannBuffer.Length);
            networkStream.Write(bytes, 0, bytes.Length);
        }

        public T GetJson<T>()
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    ReadStream(ms);
                    ms.Position = 0;
                    return DecodeJson<T>(ms);
                }
            }
            catch (Exception e)
            {
                string was = e.ToString();
                throw;
            }
        }

        public byte[] Get()
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    ReadStream(ms);
                    ms.Position = 0;
                    byte[] result = new byte[ms.Length];
                    ms.Read(result, 0, result.Length);
                    return result;
                }
            }
            catch (Exception e)
            {
                string was = e.ToString();
                throw;
            }
        }

        public void ReadStream(Stream stream)
        {
            string cls = Headers["content-length"].Value;
            int length = int.Parse(cls);
            int rbp = readBufferPosition;

            while (length > 0)
            {
                int read;
                if (rbp > 0)
                {
                    int cache = readBufferCount - rbp;
                    if (cache > 0)
                        read = Math.Min(length, cache);
                    else
                    {
                        rbp = 0;
                        continue;
                    }
                }
                else
                {
                    int readLength = Math.Min(readBuffer.Length, length);
                    read = networkStream.Read(readBuffer, 0, readLength);
                }
                length -= read;
                stream.Write(readBuffer, rbp, read);
                rbp = 0;
            }
        }

        public string ReadString()
        {
            MemoryStream ms = new MemoryStream();
            ReadStream(ms);
            ms.Position = 0;
            byte[] buffer = new byte[ms.Length];
            ms.Read(buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer);
        }

        public void SendStream(Stream stream, string contentType, string lastModified, bool noCache)
        {
            if (!noCache)
            {
                KeyValuePair<string, string> ims;
                if (Headers.TryGetValue("if-modified-since", out ims))
                {
                    if (ims.Value == Constants.NotModified)
                    {
                        Send304();
                        return;
                    }
                }
            }

            if (lastModified == null)
                lastModified = DateTime.Now.ToUniversalTime().ToString("r");

            if (noCache)
                lastModified = null;

            string expires = "\r\nExpires: " + DateTime.Now.ToUniversalTime().ToString("r");
            string lastModifiedString = lastModified != null ? string.Format("\r\nLast-Modified: {0}", lastModified) : null;
            string headerString;
            if (contentType != null)
            {
                if (contentType == "video/mp4")
                    headerString = string.Format("HTTP/1.1 200 OK\r\nContent-Length: {0}\r\nETag: \"0815\"\r\nAccept-Ranges: bytes\r\nContent-Type: {1}\r\nKeep-Alive: timeout=5, max=99\r\nConnection: Keep-Alive\r\n\r\n",
                        stream.Length, contentType);
                else
                    headerString = string.Format("HTTP/1.1 200 OK\r\nContent-Length: {0}\r\nContent-Type: {1}{2}{3}\r\n\r\n", stream.Length, contentType, expires, lastModifiedString);
            }

            else
                headerString = string.Format("HTTP/1.1 200 OK\r\nContent-Length: {0}{1}{2}\r\n\r\n", stream.Length, expires, lastModifiedString);

            if (noCache)
                headerString = headerString.Insert(headerString.Length - 4, "\r\nCache-Control: no-cache,no-store");

            byte[] vorspannBuffer = ASCIIEncoding.ASCII.GetBytes(headerString);
            networkStream.Write(vorspannBuffer, 0, vorspannBuffer.Length);
            byte[] bytes = new byte[4000];
            while (true)
            {
                int read = stream.Read(bytes, 0, bytes.Length);
                if (read == 0)
                    return;
                networkStream.Write(bytes, 0, read);
            }
        }

        public bool CheckWsUpgrade()
        {
            KeyValuePair<string, string> upgrade;
            if (Headers.TryGetValue("upgrade", out upgrade))
                return (string.Compare(upgrade.Value, "websocket", true) == 0);
            return false;
        }

        public void UpgradeToWebSocket()
        {
            string secKey = Headers["sec-websocket-key"].Value;
            secKey += webSocketKeyConcat;
            byte[] hashKey = SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(secKey));
            string base64Key = Convert.ToBase64String(hashKey);
            string response = string.Format("HTTP/1.1 101 Switching Protocols\r\nConnection: Upgrade\r\nUpgrade: websocket\r\nSec-WebSocket-Accept: {0}\r\n\r\n",
                base64Key);
            Console.WriteLine();
            Console.WriteLine(response);
            byte[] bytes = Encoding.UTF8.GetBytes(response);
            networkStream.Write(bytes, 0, bytes.Length);
        }

        public IAsyncResult WsBeginReceive(byte[] buffer, AsyncCallback callback, object state)
        {
            return networkStream.BeginRead(buffer, 0, buffer.Length, callback, state);
        }

        public int WsEndReceive(IAsyncResult ar)
        {
            int read = networkStream.EndRead(ar);
            if (read == 0)
            {
                try
                {
                    networkStream.Close();
                }
                catch { }
                throw new CloseException();
            }
            return read;
        }

        public string DecodeWSBuffer(byte[] buffer)
        {
            byte eins = buffer[0];
            // eins == 129: Opcode 1 Text
            // Opcode A pong
            // Opcode 8 close
            byte opcode = (byte)((byte)eins & 0xf);
            if (opcode == 8)
            {
                try
                {
                    networkStream.Close();
                }
                catch { }
                throw new CloseException();
            }
            int zwei = buffer[1];
            long length = zwei - 128;
            int bufferIndex = 2;
            //If the second byte minus 128 is between 0 and 125, this is the length of message. 
            if (length == 0)
                return null;
            if (length == 126)
            {
                byte[] ushortbytes = new byte[2];
                ushortbytes[0] = buffer[3];
                ushortbytes[1] = buffer[2];
                length = BitConverter.ToUInt16(ushortbytes, 0);
                bufferIndex += 2;
            }
            // If it is 126, the following 2 bytes (16-bit unsigned integer), if 127, the following 8 bytes (64-bit unsigned integer) are the length.

            byte[] key = new byte[4] { buffer[bufferIndex], buffer[bufferIndex + 1], buffer[bufferIndex + 2], buffer[bufferIndex + 3] };
            Byte[] decoded = new Byte[length];
            for (int i = 0; i < length; i++)
            {
                decoded[i] = (Byte)(buffer[i + bufferIndex + 4] ^ key[i % 4]);
            }
            return Encoding.UTF8.GetString(decoded);
        }

        public void WsSend(string text)
        {
            byte[] payload = Encoding.UTF8.GetBytes(text);
            byte FRRROPCODE = Convert.ToByte("10000001", 2); //'FIN is set, and OPCODE is 1 or Text

            byte[] header;
            if (payload.Length <= 125)
                header = new byte[] { FRRROPCODE, Convert.ToByte(payload.Length) };
            else if (payload.Length <= ushort.MaxValue)
            {
                header = new byte[] { FRRROPCODE, 126, 0, 0 };
                ushort sl = (ushort)payload.Length;
                byte[] byteArray = BitConverter.GetBytes(sl);
                byte eins = byteArray[0];
                byteArray[0] = byteArray[1];
                byteArray[1] = eins;
                Buffer.BlockCopy(byteArray, 0, header, 2, 2);
            }
            else
            {
                header = new byte[] { FRRROPCODE, 127, 0, 0, 0, 0, 0, 0, 0, 0 };
                byte[] byteArray = BitConverter.GetBytes((ulong)payload.Length);
                byte eins = byteArray[0];
                byte zwei = byteArray[1];
                byte drei = byteArray[2];
                byte vier = byteArray[3];
                byte fünf = byteArray[4];
                byte sechs = byteArray[5];
                byte sieben = byteArray[6];
                byteArray[0] = byteArray[7];
                byteArray[1] = sieben;
                byteArray[2] = sechs;
                byteArray[3] = fünf;
                byteArray[4] = vier;
                byteArray[5] = drei;
                byteArray[6] = zwei;
                byteArray[7] = eins;
                Buffer.BlockCopy(byteArray, 0, header, 2, 8);
            }

            byte[] responseData = new byte[(header.Length + payload.Length)];
            int index = 0;
            Buffer.BlockCopy(header, 0, responseData, index, header.Length);
            index += header.Length;

            Buffer.BlockCopy(payload, 0, responseData, index, payload.Length);
            index += payload.Length;
            try
            {
                networkStream.Write(responseData, 0, responseData.Length);
            }
            catch
            {
                try
                {
                    networkStream.Close();
                }
                catch { }
                throw new CloseException();
            }
        }

        public T DecodeJson<T>(string json)
        {
            using (MemoryStream memStream = new MemoryStream())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(json);
                memStream.Write(bytes, 0, bytes.Length);
                memStream.Position = 0;
                return DecodeJson<T>(memStream);
            }
        }

        public T DecodeJson<T>(Stream stream)
        {
            DataContractJsonSerializer jason = new DataContractJsonSerializer(typeof(T));
            return (T)jason.ReadObject(stream);
        }

        public string EncodeJson<T>(T obj)
        {
            DataContractJsonSerializer jason = new DataContractJsonSerializer(typeof(T));
            MemoryStream memStream = new MemoryStream();
            jason.WriteObject(memStream, obj);
            int length = (int)memStream.Position;
            memStream.Position = 0;
            byte[] bytes = new byte[length];
            memStream.Read(bytes, 0, length);
            return Encoding.UTF8.GetString(bytes);
        }

        #endregion

        #region Constructor

        public Session(Server server, Stream networkStream)
        {
            Headers = new Dictionary<string, KeyValuePair<string, string>>();
            this.Server = server;
            this.networkStream = networkStream;
            sessionID = Interlocked.Increment(ref sessionIDCreator);
            Console.WriteLine("New Session: {0}", sessionID);
        }

        #endregion

        #region Methods

        public bool Receive()
        {
            try
            {
                ReadHeaders();

                //string refererPath = null;
                //if (headers.ContainsKey("Referer"))
                //{
                //    string path = headers["Referer"];
                //    path = path.Substring(path.IndexOf("//") + 2);
                //    refererPath = path.Substring(path.IndexOf('/') + 1);
                //}

                string query;
                Extension extension = CheckExtension(url);
                if (extension != null)
                {
                    string path = extension.Url;
                    query = url.Substring(path.Length).Trim('/');
                    return extension.Request(this, method, path, query);
                }

                bool isDirectory = url.EndsWith("/");
                string file = CheckFile(out query);

                if (!string.IsNullOrEmpty(file))
                {
                    if (file.EndsWith(".php", StringComparison.InvariantCultureIgnoreCase))
                        SendPhp(file, query);
                    if (file.EndsWith(".mp4", StringComparison.InvariantCultureIgnoreCase))
                        SendMp4(file);
                    else
                        SendFile(file);
                }
                else
                {
                    if (!isDirectory)
                        RedirectDirectory(url + '/');
                    else
                    {
                        if (url.Length > 2)
                        {
                            url = url.Substring(0, url.Length - 1);
                            string relativePath = url.Replace('/', '\\');
                            relativePath = relativePath.Substring(1);
                            string path = Path.Combine(Server.Configuration.Webroot, relativePath);
                            FileInfo fi = new FileInfo(path);
                            if (fi.Exists)
                            {
                                RedirectDirectory(url);
                                return true;
                            }
                        }
                        SendNotFound();
                    }
                }
            }
            catch (SocketException se)
            {
                if (se.SocketErrorCode == SocketError.TimedOut)
                {
                    Close();
                    return false;
                }
            }
            catch (CloseException)
            {
                return false;
            }
            catch (ObjectDisposedException)
            {
                return false;
            }
            catch (IOException)
            {
                Close();
                return false;
            }
            catch (Exception)
            {
            }
            return true;
        }

        void SendFile(string file)
        {
            FileInfo fi = new FileInfo(file);
            bool noCache = Server.Configuration.NoCacheFiles.Contains(file.ToLower());

            if (!noCache)
            {
                KeyValuePair<string, string> ims;
                if (Headers.TryGetValue("if-modified-since", out ims))
                {
                    string dt = ims.Value;
                    int pos = dt.IndexOf(';');
                    if (pos != -1)
                        dt = dt.Substring(0, pos);
                    DateTime ifModifiedSince = Convert.ToDateTime(dt);
                    DateTime fileTime = fi.LastWriteTime.AddTicks(-(fi.LastWriteTime.Ticks % TimeSpan.FromSeconds(1).Ticks));
                    TimeSpan diff = fileTime - ifModifiedSince;
                    if (diff <= TimeSpan.FromMilliseconds(0))
                    {
                        Send304();
                        return;
                    }
                }
            }

            string contentType = null;
            switch (fi.Extension)
            {
                case ".html":
                case ".htm":
                    contentType = "text/html; charset=UTF-8";
                    break;
                case ".css":
                    contentType = "text/css; charset=UTF-8";
                    break;
                case ".js":
                    contentType = "application/javascript; charset=UTF-8";
                    break;
                case ".mp4":
                    contentType = "video/mp4";
                    break;
                case ".appcache":
                    contentType = "text/cache-manifest";
                    break;
            }

            DateTime dateTime = fi.LastWriteTime;
            string lastModified = dateTime.ToUniversalTime().ToString("r");

            try
            {
                using (Stream stream = File.OpenRead(file))
                {
                    SendStream(stream, contentType, lastModified, noCache);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void RedirectDirectory(string newUrl)
        {
            if (Headers.ContainsKey("host"))
            {
                string response = "<html><head>Moved permanently</head><body><h1>Moved permanently</h1>The specified resource moved permanently.</body</html>";
                byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                string host = Headers["host"].Value;
                string redirectHeaders = string.Format("HTTP/1.1 301 Moved Permanently\r\nLocation: http://{0}{1}\r\nContent-Length: {2}\r\n\r\n", host, newUrl, responseBytes.Length);

                byte[] vorspannBuffer = ASCIIEncoding.ASCII.GetBytes(redirectHeaders);
                networkStream.Write(vorspannBuffer, 0, vorspannBuffer.Length);
                networkStream.Write(responseBytes, 0, responseBytes.Length);
            }
        }

        public void TlsRedirect()
        {
            ReadHeaders();
            if (Headers.ContainsKey("host"))
            {
                string host = Headers["host"].Value;
                string redirect = string.Format("HTTP/1.1 301 Moved Permanently\r\nLocation: https://{0}{1}\r\nConnection: close", host, url);
                byte[] bytes = Encoding.UTF8.GetBytes(redirect);
                networkStream.Write(bytes, 0, bytes.Length);
            }
            Close();
        }

        void Close()
        {
            networkStream.Close();
        }

        string ReadHeaderFromStream()
        {
            int index = 0;
            while (true)
            {
                int read = networkStream.Read(readBuffer, index, readBuffer.Length - index);
                if (read == 0)
                    throw new CloseException();
                for (int i = index; i < Math.Min(read + index, readBuffer.Length); i++)
                {
                    if (i > 4 && readBuffer[i] == '\n' && readBuffer[i - 1] == '\r' && readBuffer[i - 2] == '\n')
                    {
                        readBufferPosition = i + 1;
                        readBufferCount = index + read;
                        return Encoding.ASCII.GetString(readBuffer, 0, i - 1);
                    }
                }
                index += read;
            }
        }

        void ReadHeaders()
        {
            string headerstring = ReadHeaderFromStream();
            string[] headerParts = headerstring.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (headerParts[0].StartsWith("GET"))
                method = Method.GET;
            else if (headerParts[0].StartsWith("POST"))
                method = Method.POST;
            int start = headerParts[0].IndexOf(' ') + 1;
            url = headerParts[0].Substring(start, headerParts[0].IndexOf(" HTTP") - start);
            if (url.EndsWith(".mp4"))
            {

            }

            Headers.Clear();
            var keyValues = headerParts.Skip(1).Select(s => new KeyValuePair<string, string>(s.Substring(0, s.IndexOf(": ")), s.Substring(s.IndexOf(": ") + 2)));
            foreach (var keyValue in keyValues)
                Headers[keyValue.Key.ToLower()] = keyValue;

            Console.WriteLine();
            Console.WriteLine("Method: {0}", method);
            Console.WriteLine("URL: {0}", url);
            foreach (var header in Headers)
            {
                Console.WriteLine("{0}: {1}", header.Value.Key, header.Value.Value);
            }
        }

        string CheckFile(out string query)
        {
            int raute = url.IndexOf('#');
            if (raute != -1)
                url = url.Substring(0, raute);
            bool isDirectory = url.EndsWith("/");
            string relativePath = url.Replace('/', '\\');
            relativePath = relativePath.Substring(1);
            string path = Path.Combine(Server.Configuration.Webroot, relativePath);

            int queryStart = path.IndexOf('?');
            if (queryStart != -1)
            {
                query = path.Substring(queryStart + 1);
                path = path.Substring(0, queryStart);
            }
            else
                query = null;
            if (File.Exists(path))
                return path;
            else if (!isDirectory)
                return null;
            string subpath = path;
            path = Path.Combine(subpath, "index.html");
            if (File.Exists(path))
                return path;
            path = Path.Combine(subpath, "index.php");
            if (File.Exists(path))
                return path;
            return null;
        }

        Extension CheckExtension(string url)
        {
            return Server.Configuration.Extensions.FirstOrDefault(n => url.ToLower().StartsWith(n.Url.ToLower()));
        }

        void SendMp4(string file)
        {
            FileInfo fi = new FileInfo(file);
            if (!Headers.ContainsKey("range"))
            {
                SendFile(file);
                return;
            }

            string rangeString = Headers["range"].Value;
            rangeString = rangeString.Substring(rangeString.IndexOf("bytes=") + 6);
            int minus = rangeString.IndexOf('-');
            long start = 0;
            long end = fi.Length - 1;
            if (minus == 0)
                end = long.Parse(rangeString.Substring(1));
            else if (minus == rangeString.Length - 1)
                start = long.Parse(rangeString.Substring(0, minus));
            else
            {
                start = long.Parse(rangeString.Substring(0, minus));
                end = long.Parse(rangeString.Substring(minus + 1));
            }

            string headerString = string.Format("HTTP/1.1 206 Partial Content\r\nETag: \"0815\"\r\nAccept-Ranges: bytes\r\nContent-Length: {0}\r\nContent-Range: bytes {1}-{2}/{3}\r\nKeep-Alive: timeout=5, max=99\r\nConnection: Keep-Alive\r\nContent-Type: video/mp4\r\n\r\n", end - start + 1, start, end, fi.Length);
            byte[] vorspannBuffer = ASCIIEncoding.ASCII.GetBytes(headerString);
            networkStream.Write(vorspannBuffer, 0, vorspannBuffer.Length);
            byte[] bytes = new byte[40000];
            long length = end - start;
            using (Stream stream = File.OpenRead(file))
            {
                stream.Seek(start, SeekOrigin.Begin);
                long completeRead = 0;
                while (true)
                {
                    int read = stream.Read(bytes, 0, bytes.Length);
                    if (read == 0)
                        return;
                    completeRead += read;
                    if (completeRead == end - start)
                        return;
                    networkStream.Write(bytes, 0, read);
                }
            }
        }

        void Send304()
        {
            string headerString = "HTTP/1.1 304 Not Modified\r\n\r\n";
            byte[] vorspannBuffer = ASCIIEncoding.ASCII.GetBytes(headerString);
            networkStream.Write(vorspannBuffer, 0, vorspannBuffer.Length);
        }

        void SendPhp(string file, string query)
        {
            ProcessStartInfo psi = new ProcessStartInfo(Server.Configuration.PhpBin);
            psi.Arguments = string.Format(@"""{0}""", file);
            if (query != null)
                psi.Arguments += " " + query.Replace('&', ' ');
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            Process p = Process.Start(psi);
            string headers = "HTTP/1.1 200 OK\r\nConnection: close\r\n";
            byte[] vorspannBuffer = ASCIIEncoding.ASCII.GetBytes(headers);
            networkStream.Write(vorspannBuffer, 0, vorspannBuffer.Length);
            string text = p.StandardOutput.ReadToEnd();
            Console.WriteLine(text);
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            networkStream.Write(bytes, 0, bytes.Length);
            Close();
        }

        void SendNotFound()
        {
            SendError("Resource nicht gefunden",
                "<h1>Datei nicht gefunden</h1><p>Die angegebene Resource konnte auf dem Server nicht gefunden werden.</p>",
                404, "Not Found");
        }

        #endregion

        #region Fields

        static int sessionIDCreator;
        int sessionID;
        Stream networkStream;
        Method method = Method.UNDEFINED;
        string url;
        string webSocketKeyConcat = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
        byte[] readBuffer = new byte[20000];
        int readBufferPosition;
        int readBufferCount;

        #endregion
    }
}
