using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace concator
{
    class Program
    {
        static MyComparer myComparer = new MyComparer();

        static void Main(string[] args)
        {
            string baseDir = @"D:\Files\Temp\xxzhs";
            var dirs = Directory.GetDirectories(baseDir);
            var cmdSB = new StringBuilder();

            foreach(var dir in dirs)
            {
                var files = Directory.GetFiles(dir, "*.mp4");
                List<string> filenames = new List<string>();
                foreach(var file in files)
                {
                    filenames.Add(Path.GetFileName(file));
                }

                filenames.Sort(myComparer);

                var listFilePath = Path.Combine(dir, "files.txt");
                StringBuilder sb = new StringBuilder();
                foreach(var filename in filenames)
                {
                    sb.AppendFormat("file '{0}'{1}", Path.Combine(dir, filename), Environment.NewLine);
                }
                File.WriteAllText(listFilePath, sb.ToString());
                
                var cmd = string.Format("C:\\Tools\\ffmpeg\\bin\\ffmpeg.exe -f concat -safe 0 -i \"{0}\" -c copy \"{1}\\{2}.mp4\"", listFilePath, baseDir, Path.GetFileName(dir).Substring(Path.GetFileName(dir).IndexOf("2017")));
                cmdSB.AppendLine(cmd);
            }

            File.WriteAllText(Path.Combine(baseDir, "concat.cmd"), cmdSB.ToString());

        }


        class MyComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                x = Path.GetFileNameWithoutExtension(x);
                y = Path.GetFileNameWithoutExtension(y);

                var xx = x.Substring(x.IndexOf("-") + 1);
                var yy = y.Substring(y.IndexOf("-") + 1);
                return int.Parse(xx) - int.Parse(yy);
            }
        }
    }
}
