using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Commander
{
    static class DirectoryInfoExtensions
    {
        public static FileInfo[] SafeGetFiles(this DirectoryInfo di)
        {
            try
            {
                return di.GetFiles();
            }
            catch (UnauthorizedAccessException)
            {
                return new FileInfo[0];
            }
        }

        public static DirectoryInfo[] SafeGetDirectories(this DirectoryInfo di)
        {
            try
            {
                return di.GetDirectories();
            }
            catch (UnauthorizedAccessException)
            {
                return new DirectoryInfo[0];
            }
        }
        
    }
}
