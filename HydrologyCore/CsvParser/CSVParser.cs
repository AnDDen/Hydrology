using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.IO;

namespace CsvParser
{
    public class CSVParserException : Exception
    {
        public CSVParserException() : base() { }

        public CSVParserException(string message) : base(message) { }
    }

    public class CSVParser
    {
        private const char COMMA = ',';
        private const char EXP   = '~';

        private const string EOR      = "\n\r";
        private const char DQUOTE     = '\"';
        private const string DDQUOTE  = "\"\"";

        private string str;
        private int index;

        private bool IsMatch(string matchStr)
        {
            if (index + matchStr.Length > str.Length) return false;
            if (str.Substring(index, matchStr.Length) == matchStr) return true;
            return false;
        }

        private bool IsMatch(char matchChar)
        {
            if (index >= str.Length) return false;
            return str[index] == matchChar;
        }

        private void Match(char matchChar)
        {
            index++;
        }

        private void Match(string matchStr)
        {
            index += matchStr.Length;
        }

        public CSVParser(string str)
        {
            this.str = str;
            index = 0;
        }

        public DataTable Parse()
        {
            index = 0;
            DataTable table = new DataTable();
            IList<string> columnNames = ParseRecord();
            foreach (string column in columnNames) {
                table.Columns.Add(column);
            }
            while (str != "")
            {
                IList<string> row = ParseRecord();
                DataRow dataRow = table.NewRow();
                for (int i = 0; i < row.Count; i++)
                    dataRow[columnNames[i]] = row[i];
                table.Rows.Add(dataRow);
            }
            return table;
        }

        private IList<string> ParseRecord()
        {
            IList<string> record = new List<string>();
            string fieldColumn = ParseFieldColumn();
            record.Add(fieldColumn);
            while (!IsMatch(EOR))
            {
                if (!IsMatch(COMMA))
                {
                    Match(COMMA);
                    fieldColumn = ParseFieldColumn();
                    record.Add(fieldColumn);
                }
            }
            Match(EOR);
            return record;
        }

        private string ParseFieldColumn()
        {
            if (IsMatch(DQUOTE) && !IsMatch(DDQUOTE))
            {
                Match(DQUOTE);
                if (IsMatch(EXP)) Match(EXP);
                string s = ParseFieldPayload();
                if (!IsMatch(DQUOTE))
                    throw new CSVParserException(string.Format("Waited for symbol \" at position {0}", index));
                Match(DQUOTE);
                return s;
            }
            else
            {
                if (IsMatch(EXP)) Match(EXP);
                return ParseRawFieldPayload();
            }
        }

        private string ParseFieldPayload()
        {
            string s = "";
            while (!IsMatch(DQUOTE) || IsMatch(DDQUOTE))
            {
                if (IsMatch(DDQUOTE)) {
                    s += DDQUOTE;
                    Match(DDQUOTE);
                }
                else
                    s += str[index++];
            }
            return s;
        }

        private string ParseRawFieldPayload()
        {
            // TODO

            string s = "";
            while (!IsMatch(DQUOTE) || IsMatch(DDQUOTE))
            {
                if (IsMatch(DDQUOTE))
                {
                    s += DDQUOTE;
                    Match(DDQUOTE);
                }
                else
                    s += str[index++];
            }
            return s;
        }
    }
}
