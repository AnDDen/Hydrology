using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.IO;
using CoreInterfaces;

namespace CsvParser
{
    public class CSVWriter : IWriter
    {
        private const string COMMA = ",";
        private const string EXP = "~";

        private const string CR = "\r";
        private const string LF = "\n";
        private const string CRLF = "\r\n";

        private const string DQUOTE = "\"";
        private const string DDQUOTE = "\"\"";
        private const string SPACE = " ";
        private const string TAB = "\t";

        private static string WriteFieldColumn(string str)
        {
            string s = str;
            if (s.Contains(DQUOTE))
                s = s.Replace(DQUOTE, DDQUOTE);
            if (s.Contains(DQUOTE) || s.Contains(COMMA) || s.Contains(CR) || s.Contains(LF) || s.Contains(TAB))
                s = DQUOTE + s + DQUOTE;
            return s;
        }

        public void Write(DataTable table, string filePath)
        {
            FileStream fs = null;
            StreamWriter writer = null;
            try
            {
                fs = new FileStream(filePath, FileMode.Create);
                writer = new StreamWriter(fs);

                for (int i = 0; i < table.Columns.Count; i++)
                {
                    writer.Write(WriteFieldColumn(table.Columns[i].ColumnName));
                    if (i < table.Columns.Count - 1)
                        writer.Write(COMMA);
                }
                writer.Write(CRLF);

                foreach (DataRow row in table.Rows)
                {
                    for (int i = 0; i < row.ItemArray.Length; i++)
                    {
                        writer.Write(WriteFieldColumn(row.ItemArray[i].ToString()));
                        if (i < row.ItemArray.Length - 1)
                            writer.Write(COMMA);
                    }
                    writer.Write(CRLF);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                if (writer != null)
                    writer.Close();
                if (fs != null)
                    fs.Close();
            }
        }
    }
}
