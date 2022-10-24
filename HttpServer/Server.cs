using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Xml.Linq;

namespace HttpServer
{
    public class Server : IServer
    {
        #region Properties

        public Configuration Configuration { get; private set; }

        #endregion

        #region Constructor

        public Server(Configuration configuration)
        {
            Configuration = configuration;
            listener = new TcpListener(IPAddress.Any, configuration.Port);
            if (Configuration.IsTlsEnabled)
                InitializeTls();
            //if (configuration.IsTlsEnabled)
            //    tlsRedirectorListener = new TcpListener(IPAddress.Any, Port);
        }

        static Server()
        {
            ServicePointManager.DefaultConnectionLimit = 50;
            ThreadPool.SetMinThreads(50, 50);
        }

        #endregion

        #region Methods

        public void Start()
        {
            listener.Start();
            ThreadPool.QueueUserWorkItem(Connect);
            if (Configuration.IsTlsEnabled)
            {
//                tlsRedirectorListener.Start();
//                ThreadPool.QueueUserWorkItem(TlsRedirectConnect);
            }
        }

        void InitializeTls()
        {
            X509Store store = new X509Store(StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);
            X509Certificate2Collection cers = store.Certificates.Find(X509FindType.FindBySubjectName, Configuration.TlsDomainName, false);
            certificate = cers[0];
        }

        void Connect(object state)
        {
            try
            {
                TcpClient client = listener.AcceptTcpClient();
                client.ReceiveTimeout = socketTimeout;
                client.SendTimeout = socketTimeout;
                ThreadPool.QueueUserWorkItem(Receive, client);
                ThreadPool.QueueUserWorkItem(Connect);
            }
            catch { }
        }

        //void TlsRedirectConnect(object state)
        //{
        //    try
        //    {
        //        TcpClient client = tlsRedirectorListener.AcceptTcpClient();
        //        client.ReceiveTimeout = socketTimeout;
        //        client.SendTimeout = socketTimeout;
        //        ThreadPool.QueueUserWorkItem(TlsRedirectConnect);
        //        Session redirectSession = new Session(this, client.GetStream());
        //        redirectSession.TlsRedirect();
        //    }
        //    catch { }
        //}

        void Receive(object state)
        {
            TcpClient client = state as TcpClient;
            try
            {
                Session session = new Session(this, GetNetworkStream(client));
                while (true)
                {
                    if (!session.Receive())
                        return;
                }
            }
            catch (IOException)
            {
                client.Close();
            }
            catch (CloseException)
            {
                client.Close();
            }
            catch (SocketException)
            {
                client.Close();
            }
        }

        Stream GetNetworkStream(TcpClient tcpClient)
        {
            Stream stream = tcpClient.GetStream();
            if (!Configuration.IsTlsEnabled)
                return stream;

            SslStream sslStream = new SslStream(stream);
            sslStream.AuthenticateAsServer(certificate, false, System.Security.Authentication.SslProtocols.Tls, false);
            //sslStream.AuthenticateAsServer(certificate, false, System.Security.Authentication.SslProtocols.Tls12, true);
            var welches = sslStream.SslProtocol;
            return sslStream;
        }

        #endregion

        #region Fields

        static object locker = new object();
        static X509Certificate2 certificate;
        TcpListener listener;
        //TcpListener tlsRedirectorListener;
        int socketTimeout = 20000;
        
        #endregion
    }
}
