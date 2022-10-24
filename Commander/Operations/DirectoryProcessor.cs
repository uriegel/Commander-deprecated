using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Commander
{
    class DirectoryProcessor : OperationProcessor
    {
        public DirectoryProcessor(string directory) : base(directory)
        {
        }

        public override OperationItem[] GetTargetItems(IEnumerable<OperationItem> source)
        {
            return source.Where(n => File.Exists(Path.Combine(Directory, n.NameWithSubPath))).ToArray();
        }

        public override void FillConflicts(OperationItem oi, bool target)
        {
            OperationFileItem conflict = oi as OperationFileItem;
            if (conflict != null)
            {
                var source = Path.Combine(Directory, conflict.NameWithSubPath);
                var info = new FileInfo(source);
                if (target)
                {
                    conflict.targetFileSize = info.Length;
                    conflict.targetVersion = FileVersion.Get(source);
                    conflict.SetTargetDateTime(info.LastWriteTime);
                }
                else
                {
                    conflict.sourceVersion = FileVersion.Get(source);
                    conflict.SetSourceDateTime(info.LastWriteTime);
                }
            }
        }

        public override void ExtractSubItems(List<OperationItem> operationItems, OperationItem directoryItem)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(Directory, directoryItem.NameWithSubPath));
            RefreshFiles(operationItems, directoryInfo, directoryItem.SubPath);
            var subDirs = RefreshDirectories(directoryInfo, directoryItem.SubPath);
            foreach (var subDir in subDirs)
                ExtractSubItems(operationItems, subDir);
        }

        public override bool CheckSubordinates(IOItem[] items, OperationProcessor target)
        {
            DirectoryProcessor targetProcessor = target as DirectoryProcessor;
            if (targetProcessor == null)
                // verschiedene Processoren, kann nicht untergeordnet sein
                return true;

            string[] parents = targetProcessor.GetParentItems();
            var directories = items.OfType<DirectoryItem>().ToArray();
            return directories.Any(n => CheckSubordinates(n, parents) == false) ? false : true;
        }

        protected override string[] GetParentItems()
        {
            List<string> parents = new List<string>();
            string directory = Directory;
            while (true)
            {
                DirectoryInfo di = new DirectoryInfo(directory);
                if (di.Parent == null)
                    return parents.ToArray();
                directory = di.Parent.FullName;
                parents.Add(directory);
            }
        }

        void RefreshFiles(List<OperationItem> operationItems, DirectoryInfo directoryInfo, string subPath)
        {
            var files = directoryInfo.SafeGetFiles()
                .Where(n => Show(n.Attributes))
                .Select(n => new OperationFileItem(n.Name, n.Extension, n.FullName, n.Length,
                    n.LastWriteTime.ToString("o"), CombineSubPath(subPath, directoryInfo.Name)));
            foreach (var file in files)
            {
                if (isCancelled)
                    throw new CancelledException();
                operationItems.Add(file);
            }
        }

        OperationDirectoryItem[] RefreshDirectories(DirectoryInfo directoryInfo, string subPath)
        {
            return directoryInfo.SafeGetDirectories()
                .Where(n => Show(n.Attributes))
                .OrderBy(n => n.Name)
                .Select(n => new OperationDirectoryItem(n.Name, CombineSubPath(subPath, directoryInfo.Name))).ToArray();
        }

        bool Show(System.IO.FileAttributes attributes)
        {
            // TODO: Show Hidden
            return true;
        }

        string CombineSubPath(string subPath, string directoryName)
        {
            if (subPath == null)
                return directoryName;
            return Path.Combine(subPath, directoryName);
        }

        bool CheckSubordinates(DirectoryItem directoryItem, string[] parents)
        {
            Uri sourceUri = new Uri(directoryItem.fullName);
            IEnumerable<Uri> targetUris = parents.Select(n => new Uri(n));
            return targetUris.Any(n => n == sourceUri) ? false : true;
        }
    }
}
