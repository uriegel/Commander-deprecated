using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Commander
{
    class CommanderContext
    {
        #region Properties	

        public bool ShowHidden
        {
            get
            {
                return Extension.ShowHidden;
            }
            set
            {
                Extension.ShowHidden = value;
                if (webSocket != null)
                    webSocket.SendRefreshCommand();
            }
        }

        #endregion

        #region Commands	

        public Command Copy { get; private set; }
        public Command Move { get; private set; }
        public Command Delete { get; private set; }

        #endregion

        #region Methods

        public static void Initialize(WebSocket webSocket)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    var dc = Application.Current.MainWindow.DataContext as CommanderContext;
                    if (dc.webSocket == null)
                        dc.webSocket = webSocket;
                }));
        }

        #endregion

        #region Constructor	

        public CommanderContext()
        {
            Copy = new Command(CopyExecuted);
            Move = new Command(MoveExecuted);
            Delete = new Command(DeleteExecuted);
        }

        #endregion
        
        #region CommandBindings

        void CopyExecuted(object parameter)
        {
            if (webSocket != null)
                webSocket.SendCopyCommand();    
        }

        void MoveExecuted(object parameter)
        {
            if (webSocket != null)
                webSocket.SendMoveCommand();
        }

        void DeleteExecuted(object parameter)
        {
            if (webSocket != null)
                webSocket.SendDeleteCommand();
        }

        #endregion

        #region Fields	

        WebSocket webSocket;

        #endregion
    }
}