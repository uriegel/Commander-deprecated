using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander
{
    class DeleteOperation : Operation
    {
        public DeleteOperation(string sourceDir, IOItem[] items)
            : base(sourceDir, null, items)
        {
        }

        protected override void RunOperation(IEnumerable<OperationItem> operationItems, bool ignoreConflicts)
        {
            FileOperation fi = new FileOperation(sourceProcessor.Directory, Api.FileFuncFlags.FO_DELETE);
            fi.Delete(items);
        }

        protected override bool IsDualOperation()
        {
            return false;
        }
    }
}
