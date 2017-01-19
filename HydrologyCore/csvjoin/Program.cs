using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace csvjoin
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine(
@"Takes the 2nd line of given CSV files and collect them all from the experiment result folder into one CVS. Builds the header based on cycle parameters. 
  
  Usage: cvsjoin.exe [search_directory] file_mask

  If search directoy is empty then it takes all files from current directory.");
                return;
            }

            var dir = args.Length == 1 ? Directory.GetCurrentDirectory() : args[0];
            var mask = args.Length == 1 ? args[0] : args[1];
            var res = Directory.GetFiles(dir, mask, SearchOption.AllDirectories);
            StreamWriter sw = new StreamWriter(dir + "\\joined.csv");
            bool headerWritten = false;

            Console.WriteLine("{0} files found", res.Length);

            foreach (var f in res)
            {                                
                var relative = f.Replace(dir, "");
                StreamReader r = new StreamReader(f);
                r.ReadLine();
                var value = r.ReadLine();

                if (!headerWritten)
                {
                    headerWritten = true;
                    sw.WriteLine(makeHeader(relative) + ",T");
                }
                var row = string.Format("{0},{1}", makeParams(relative), value);
                r.Close();
                sw.WriteLine(row);
            }
            sw.Close();
            Console.WriteLine("Done.");
        }

        private static string makeParams(string relative)
        {
            return splitAndCollect(relative, 1);
        }

        private static string splitAndCollect(string relative, int idx)
        {
            var parts = relative.Split('\\');
            var sb = new StringBuilder();

            foreach (var p in parts)
            {
                if (string.IsNullOrEmpty(p))
                    continue;

                if (p.Contains("="))
                {
                    var pps = p.Split('=');
                    if (sb.Length == 0)
                        sb.Append(pps[idx]);
                    else
                        sb.AppendFormat(",{0}", pps[idx]);
                }
            }
            return sb.ToString();
        }

        private static string makeHeader(string relative)
        {
            return splitAndCollect(relative, 0);
        }
    }
}
