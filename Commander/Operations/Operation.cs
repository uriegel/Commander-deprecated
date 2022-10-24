using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;

namespace Commander
{
    /// <summary>
    /// Die Operation wird in der Extension angelegt und solange gespeichert, wie sie benötigt wird
    /// Falls mehrere Operations gleichzeitig (Serverbetrieb!) ablaufen können sollen, muss eine Sessionverwaltung eingeführt werden
    /// </summary>
    class Operation
    {
        public Operation(string sourceDir, string targetDir, IOItem[] items)
        {
            this.items = items.Where(n => !(n is ParentItem) && !(n is DriveItem)).ToArray();
            this.sourceProcessor = OperationProcessor.Create(sourceDir);
            if (!string.IsNullOrEmpty(targetDir))
                this.targetProcessor = OperationProcessor.Create(targetDir);
        }

        public bool CheckDirectories()
        {
            if (!IsDualOperation())
                return true;
            return string.Compare(sourceProcessor.Directory, targetProcessor.Directory, true) != 0;
        }

        public bool CheckSelection()
        {
            if (items.Count() == 0)
                return false;
            return true;
        }

        public bool CheckSubordinates()
        {
            if (!IsDualOperation())
                return true;
            return sourceProcessor.CheckSubordinates(items, targetProcessor);
        }

        public bool CheckCompatibility()
        {
            if (!IsDualOperation())
                return true;
            return CheckExtendedCompatibility();
        }

        /// <summary>
        /// Vorbereitung auf die Operation
        /// </summary>
        /// <returns>Einträge, die Konflikte verusachen können</returns>
        public OperationItem[] Prepare()
        {
            if (!IsDualOperation())
            {
                TakeItems(items);
                return new OperationItem[0];
            }
            ExtractSubItems(items);
            var result = targetProcessor.GetTargetItems(operationItems);
            foreach (var conflict in result)
            {
                sourceProcessor.FillConflicts(conflict, false);
                targetProcessor.FillConflicts(conflict, true);
            }
            if (result.Length > 0)
                conflictItems = result;
            return result;
        }

        public void Cancel()
        {
            isCancelled = true;
            if (sourceProcessor != null)
                sourceProcessor.Cancel();
        }

        public void Operate(bool ignoreConflicts)
        {
            var uiThread = new Thread(() =>
            {
                OperationWindow ow = new OperationWindow(() => RunOperation(operationItems, ignoreConflicts));
            });
            uiThread.SetApartmentState(ApartmentState.STA);
            uiThread.Start();
            uiThread.Join();
        }

        protected virtual bool CheckExtendedCompatibility()
        {
            return true;
        }

        protected virtual bool IsDualOperation()
        {
            return false;
        }

        protected virtual void RunOperation(IEnumerable<OperationItem> operationItems, bool ignoreConflicts)
        {
        }

        void ExtractSubItems(IOItem[] items)
        {
            try
            {
                operationItems.Clear();
                foreach (var item in items.OfType<FileItem>())
                {
                    if (isCancelled)
                        throw new CancelledException();
                    operationItems.Add(new OperationFileItem(item));
                }
                var dirs = items.OfType<DirectoryItem>().Select(n => new OperationDirectoryItem(n.name, null));
                foreach (var item in dirs)
                {
                    var odi = new OperationDirectoryItem(item.name, null);

                    sourceProcessor.ExtractSubItems(operationItems, odi);
                }
            }
            catch (CancelledException)
            {
                throw;
            }
            catch (Exception e)
            {
                var var = e;
            }
        }

        void TakeItems(IOItem[] items)
        {
            operationItems.Clear();
            foreach (var item in items.OfType<DirectoryItem>())
            {
                if (isCancelled)
                    throw new CancelledException();
                operationItems.Add(new OperationDirectoryItem(item.name, null));
            }
            foreach (var item in items.OfType<FileItem>())
            {
                if (isCancelled)
                    throw new CancelledException();
                operationItems.Add(new OperationFileItem(item));
            }
        }

        protected OperationProcessor sourceProcessor;
        protected OperationProcessor targetProcessor;
        protected OperationItem[] conflictItems;
        protected IOItem[] items;
        /// <summary>
        /// Alle Items, auf die diese Operation angewendet werden soll, also alle Dateien aus einem angegebenen Verzeichnis,
        /// erst verfügbar, wenn alles entpackt wurde
        /// </summary>
        List<OperationItem> operationItems = new List<OperationItem>();
        bool isCancelled = false;
    }
}
