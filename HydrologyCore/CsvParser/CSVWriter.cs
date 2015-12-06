using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace CsvParser
{
    public class CSVWriter
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

        public static string Write(DataTable table)
        {
            string output = "";

            foreach (DataColumn column in table.Columns)
                output += WriteFieldColumn(column.ColumnName) + COMMA;
            output = output.Remove(output.Length - 1, 1);
            output += CRLF;

            foreach (DataRow row in table.Rows)
            {
                foreach (string s in row.ItemArray)
                    output += WriteFieldColumn(s) + COMMA;
                output = output.Remove(output.Length - 1, 1);
                output += CRLF;
            }

            return output;
        }
    }
}
