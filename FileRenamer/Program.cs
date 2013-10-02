using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FileRenamer
{
    class Program
    {
        static void Main(string[] args)
        {
            string rootDir = @"F:\Temp\xxzhs\Finished";
            string[] subDirs = Directory.GetDirectories(rootDir);
            foreach (string dir in subDirs)
            {
                string[] files = Directory.GetFiles(dir);
                foreach (string file in files)
                {
                    FileInfo fi = new FileInfo(file);
                    if (fi.Length > 100 * 1024 * 1024)
                    {
                        string dstPath = Path.Combine(rootDir, Path.GetFileName(dir) + fi.Extension);
                        if (!File.Exists(dstPath))
                            fi.MoveTo(dstPath);
                        break;
                    }
                }
            }
        }
    }
}
