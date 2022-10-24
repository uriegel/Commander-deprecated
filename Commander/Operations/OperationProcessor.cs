using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Commander
{
    /// <summary>
    /// Entweder die Quelle oder das Ziel einer Operation
    /// </summary>
    class OperationProcessor
    {
        public string Directory { get; private set; }
        
        public OperationProcessor(string directory)
    	{
            Directory = directory;
	    }

        public static OperationProcessor Create(string directory)
        {
            if (directory == "drives")
                return new DriveProcessor();
            else
                return new DirectoryProcessor(directory);
        }

        public virtual OperationItem[] GetTargetItems(IEnumerable<OperationItem> source)
        {
            return new OperationItem[0];
        }

        public virtual bool CheckSubordinates(IOItem[] items, OperationProcessor target)
        {
            return true;
        }

        public virtual void FillConflicts(OperationItem oi, bool target)
        {
        }

        public virtual void ExtractSubItems(List<OperationItem> operationItems, OperationItem directoryItem)
        {
        }
        
        public void Cancel()
        {
            //TODO:
            //isCancelled = true;
        }

        protected virtual string[] GetParentItems()
        {
            return new string[0];
        }

        protected bool isCancelled;
    }
}
