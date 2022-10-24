using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commander
{
    static class FileAttributes
    {
        public static bool IsHidden(System.IO.FileAttributes attributes)
        {
            return (attributes & System.IO.FileAttributes.Hidden) == System.IO.FileAttributes.Hidden;
        }
    }
}
