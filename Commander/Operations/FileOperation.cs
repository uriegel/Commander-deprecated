using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace Commander
{
    class FileOperation
    {
        public FileOperation(string path, Api.FileFuncFlags function) : this(path, null, function)
        {
        }

        public FileOperation(string sourcePath, string targetPath, Api.FileFuncFlags function)
        {
            this.sourcePath = sourcePath;
            this.targetPath = targetPath;
            fileop = new Api.SHFILEOPSTRUCT();
            fileop.hwnd = Application.Current.Dispatcher.Invoke(() => new WindowInteropHelper(Application.Current.MainWindow).Handle);
            fileop.lpszProgressTitle = "Captain Kirk";
            fileop.fFlags = Api.FILEOP_FLAGS.FOF_NOCONFIRMATION | Api.FILEOP_FLAGS.FOF_NOCONFIRMMKDIR;
            
            fileop.wFunc = function;
        }

        public void Copy(IOItem[] items)
        {
            if (items.Count() == 0)
                return;

            fileop.pFrom = CreateFileOperationPaths(items.Select(n => Path.Combine(sourcePath, n.name)));
            fileop.pTo = CreateFileOperationPaths(Enumerable.Repeat<string>(targetPath, items.Length));
            int ret = Api.SHFileOperation(ref fileop);
        }

        public void  Move(IOItem[] items)
        {
            if (items.Count() == 0)
                return;

            fileop.pFrom = CreateFileOperationPaths(items.Select(n => Path.Combine(sourcePath, n.name)));
            fileop.pTo = CreateFileOperationPaths(Enumerable.Repeat<string>(targetPath, items.Length));
            int ret = Api.SHFileOperation(ref fileop);
        }

        public void Copy(IEnumerable<OperationItem> operationitems, OperationItem[] conflictItems)
        {
            var noConflictitems = operationitems.Where(n => !conflictItems.Contains(n));
            if (noConflictitems.Count() == 0)
                return;

            fileop.fFlags |= Api.FILEOP_FLAGS.FOF_MULTIDESTFILES;
            fileop.pFrom = CreateFileOperationPaths(noConflictitems.Select(n => Path.Combine(sourcePath, n.NameWithSubPath)));
            fileop.pTo = CreateFileOperationPaths(noConflictitems.Select(n => Path.Combine(targetPath, n.NameWithSubPath)));
            int ret = Api.SHFileOperation(ref fileop);
        }

        public void Delete(IOItem[] items)
        {
            if (items.Count() == 0)
                return;

            fileop.fFlags = Api.FILEOP_FLAGS.FOF_NOCONFIRMATION | Api.FILEOP_FLAGS.FOF_ALLOWUNDO;
            fileop.pFrom = CreateFileOperationPaths(items.Select(n => Path.Combine(sourcePath, n.name)));
            fileop.pTo = null;
            int ret = Api.SHFileOperation(ref fileop);
        }

        string CreateFileOperationPaths(IEnumerable<string> paths)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string path in paths)
            {
                sb.Append(path);
                sb.Append("\x0");
            }
            sb.Append("\x0");
            return sb.ToString();
        }

        string sourcePath;
        string targetPath;
        Api.SHFILEOPSTRUCT fileop;
    }
}
