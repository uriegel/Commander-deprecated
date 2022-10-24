using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using HttpServer;
using System.IO;
using System.Windows.Threading;
using System.Windows;
using System.ServiceProcess;

namespace Commander
{
    // TODO: WNetUseConnection
    // TODO: Manchmal merkwürdiger Fehler WsSocket
    // TODO: Icon bei pdf falsch, testen, ob es an der Funktion liegt, im Vergleich zum WindowsCommander
    // TODO: Per Befehl den Bitmapcache löschen
    // CommanderView.prototype.saveView = function(): Object muss geklont werden!
    // Favoriten
    // Sortierung nach Spalten
    // Im Status bei selektierten Einträgen ( > 1) die Anzahl eintragen, aber erst nach 3 s.
    // TODO: Guter Test von allem
    // TODO: Kopieren in Systemverzeichnis: Focus auf Listbox dauert zu lange
    // TODO: Bei unglaublich vielen Unterverzeichnissen dauert das Auflisten unglaublich lange (copy c:\users)
    // TODO: Nur auf erster Ebene testen
    // TODO: Bei Tiemout oder Abbruch des Zahnraddialoges: Dialogbox mit Optionen, als Administrator, Alle Konflikte überschreiben
    // TODO: c:\users kopieren...
    // TODO: PathToolongExtension: Kopieren nicht möglich, Pfad zu lang-Ausgabe
    // Readonlies beim verschieben
    // Versteckte Dateien berücksichtigen oder nicht
    public class Extension : IExtension
    {
        #region IExtension Members

        public void Request(ISession httpSession, Method method, string path, UrlQueryComponents urlQuery)
        {
            try
            {
                switch (urlQuery.Method)
                {
                    case "GetItems":
                        try
                        {
                            string dir = urlQuery.Parameters["dir"];
                            IEnumerable<IOItem> dirIoItems;
                            ItemResult itemResult;
                            switch (dir)
                            {
                                case "drives":
                                    IOItem[] items = System.IO.DriveInfo.GetDrives().Where(n => n.IsReady).OrderBy(n => n.Name).Select(n => new DriveItem(n.Name))
                                        .Concat(new[] { new DriveItem("Dienste", "images/service.png")}).ToArray();
                                    itemResult = new ItemResult("drives", items);
                                    if (serviceStateProcessor != null)
                                        serviceStateProcessor.Stop();
                                    serviceStateProcessor = null;
                                    break;
                                case "Dienste":
                                    ServiceController[] services = ServiceController.GetServices();
                                    dirIoItems = services.OrderBy(n => n.DisplayName).Select(n => new ServiceItem(n));
                                    items = Enumerable.Repeat<ParentItem>(new ParentItem("drives"), 1).Concat(dirIoItems).ToArray();
                                    itemResult = new ItemResult("Dienste", items);
                                    serviceStateProcessor = new ServiceStateProcessor(services, webSocket);
                                    break;
                                default:
                                    DirectoryInfo di = new DirectoryInfo(dir);
                                    var directories = di.SafeGetDirectories().Select(n => new DirectoryItem(n.Name, n.FullName, n.LastWriteTime, n.Attributes))
                                    .Where(n => ShowHidden ? true : !n.isHidden);
                                    var files = di.SafeGetFiles().Select(n => new FileItem(n.Name, n.FullName, n.Extension, n.LastWriteTime, n.Length, n.Attributes))
                                    .Where(n => ShowHidden ? true : !n.isHidden);
                                    dirIoItems = directories;
                                    string parent = null;
                                    if (di.Parent != null)
                                        parent = di.Parent.FullName;
                                    else
                                        parent = "drives";
                                    items = Enumerable.Repeat<ParentItem>(new ParentItem(parent), 1).Concat(dirIoItems).Concat(files).ToArray();
                                    itemResult = new ItemResult(di.FullName, items);
                                    if (serviceStateProcessor != null)
                                        serviceStateProcessor.Stop();
                                    serviceStateProcessor = null;
                                    break;
                            }

                            itemResults.TryAdd(itemResult.resultID, itemResult);
                            httpSession.SendJson(itemResult);
                        }
                        catch (UnauthorizedAccessException)
                        {
                            httpSession.SendError("Tut nich", "Tut nich", 500, "UnauthorizedAccessException");
                        }
                        break;
                    case "GetExtendedInfos":
                        ItemResult lastItemResult;
                        if (!itemResults.TryGetValue(int.Parse(urlQuery.Parameters["id"]), out lastItemResult))
                        {
                            httpSession.SendJson(new ExtendedInfosResult());
                            break;
                        }
                        httpSession.SendJson(ExtendInfos(lastItemResult));
                        break;
                    case "CheckFileOperation":
                        currentId += 1;
                        CheckFileOperation cfo = httpSession.GetJson<CheckFileOperation>();
                        switch (cfo.operation)
                        {
                            case "copy":
                                currentOperation = new CopyOperation(cfo.sourceDir, cfo.targetDir, cfo.items);
                                break;
                            case "move":
                                currentOperation = new MoveOperation(cfo.sourceDir, cfo.targetDir, cfo.items);
                                break;
                            case "delete":
                                currentOperation = new DeleteOperation(cfo.sourceDir, cfo.items);
                                break;
                        }
                        CheckFileOperationResult result = new CheckFileOperationResult();
                        if (!currentOperation.CheckSelection())
                            result.result = "noSelection"; // Es sind keine gültigen Elemente ausgewählt
                        else if (!currentOperation.CheckCompatibility())
                            result.result = "incompatible"; // Sie können diese Elemente nicht in diesen Zielordner kopieren/verschieben (Dateien nach drives)
                        else if (!currentOperation.CheckDirectories())
                            result.result = "identicalDirectories"; // Die Ordner sind identisch
                        else if (!currentOperation.CheckSubordinates())
                            result.result = "subordinateDirectory"; // Der Zielordner ist dem Quellordner untergeordnet
                        else
                            result.conflictItems = currentOperation.Prepare();
                        if (string.IsNullOrEmpty(result.result))
                            result.id = currentId;

                        httpSession.SendJson(result);
                        break;
                    case "Cancel":
                        currentOperation.Cancel();
                        httpSession.SendOK("");
                        break;
                    case "Operate":
                        Operate operate = httpSession.GetJson<Operate>();
                        if (currentId == operate.id)
                            currentOperation.Operate(operate.ignoreConflicts);
                        currentOperation = null;
                        httpSession.SendOK("");
                        break;
                    case "Icon":
                        string ico = urlQuery.Parameters["file"];
                        Api.SHFILEINFO shinfo = new Api.SHFILEINFO();
                        IntPtr ptr = Api.SHGetFileInfo(ico,
                            Api.FILE_ATTRIBUTE_NORMAL, ref shinfo, (uint)Marshal.SizeOf(shinfo),
                            (int)(Api.SHGetFileInfoConstants.SHGFI_ICON |
                            Api.SHGetFileInfoConstants.SHGFI_SMALLICON |
                            Api.SHGetFileInfoConstants.SHGFI_USEFILEATTRIBUTES |
                            Api.SHGetFileInfoConstants.SHGFI_TYPENAME));
                        if (shinfo.hIcon == IntPtr.Zero)
                        {
                            httpSession.SendError("Nicht gefunden", "Nicht gefunden", 404, "Not Found");
                            break;
                        }
                        var bitmap = Bitmap.FromHicon(shinfo.hIcon);
                        Api.DestroyIcon(shinfo.hIcon);
                        using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                        {
                            bitmap.Save(ms, ImageFormat.Png);
                            ms.Position = 0;
                            httpSession.SendStream(ms, "image/png", Constants.NotModified);
                        }
                        bitmap.Dispose();
                        break;
                    case "Process":
                        ProcessItem item = httpSession.GetJson<ProcessItem>();
                        // Options
                        //Process.Start("rundll32.exe", string.Format("shell32, Options_RunDLL {0}", item.file));
                        if (!item.openWith)
                        {
                            Process p = new Process();
                            p.StartInfo.UseShellExecute = true;
                            p.StartInfo.ErrorDialog = true;
                            p.StartInfo.FileName = item.file;
                            p.Start();
                        }
                        else
                            Process.Start("rundll32.exe", string.Format("shell32, OpenAs_RunDLL {0}", item.file));
                        httpSession.SendOK("");
                        break;
                }
                                
            }
            catch (CancelledException)
            {
                CheckFileOperationResult result = new CheckFileOperationResult();
                result.result = "cancelled";
                httpSession.SendJson(result);
            }
        }

        public void InitializeWebSocket(ISession session)
        {
            webSocket = new WebSocket(session);
        }

        #endregion

        #region Properties	

        public static bool ShowHidden { get; set; }

        #endregion

        #region Methods	
        ExtendedInfosResult ExtendInfos(ItemResult itemResult)
        {
            ConcurrentBag<IOItemExtension> result = new ConcurrentBag<IOItemExtension>();
            Parallel.ForEach(itemResult.items, (item, pls, index) =>
            {
                try
                {
                    FileItem fi = item as FileItem;
                    if (fi == null)
                        return;

                    IOItemExtension ioie = null;
                    string version = FileVersion.Get(fi.fullname);
                    if (!string.IsNullOrEmpty(version))
                    {
                        ioie = new IOItemExtension
                        {
                            index = (int)index,
                            version = version
                        };
                        result.Add(ioie);
                    }

                    if (fi.Extension.ToLower() == ".jpg")
                    {
                        using (ExifReader er = new ExifReader(fi.fullname))
                        {
                            DateTime aufnahme;
                            if (er.GetTagValue<DateTime>(ExifReader.ExifTags.DateTimeOriginal, out aufnahme))
                            {
                                if (ioie == null)
                                {
                                    ioie = new IOItemExtension
                                    {
                                        index = (int)index,
                                        dateTime = aufnahme
                                    };
                                    result.Add(ioie);
                                }
                                else
                                    ioie.dateTime = aufnahme;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    var wahr = e;
                }
            });

            var eir = new ExtendedInfosResult { items = result.OrderBy(n => n.index).ToArray() };
            ItemResult del;
            itemResults.TryRemove(itemResult.resultID, out del);
            return eir;
        }

        #endregion

        #region Fields	

        static int currentId;
        ConcurrentDictionary<int, ItemResult> itemResults = new ConcurrentDictionary<int, ItemResult>();
        Operation currentOperation;
        WebSocket webSocket;
        uint uniqueValue = 0x32A86FF;
        ServiceStateProcessor serviceStateProcessor;

        #endregion
    }
}
