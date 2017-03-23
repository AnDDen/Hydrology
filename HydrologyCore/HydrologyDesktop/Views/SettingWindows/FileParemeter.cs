using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydrologyDesktop.Views.SettingWindows
{
    public class FileParemeter
    {
        public string FilePath { get; set; }
        public string VarName { get; set; }

        public FileParemeter(string filePath, string varName)
        {
            FilePath = filePath;
            VarName = varName;
        }
    }
}
