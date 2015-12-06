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
        private const string COMMA = ",";
        private const string EXP = "~";

        private const string CR = "\r";
        private const string LF = "\n";
        private const string CRLF = "\r\n";

        private const string DQUOTE = "\"";
        private const string DDQUOTE = "\"\"";
        private const string SPACE = " ";
        private const string TAB = "\t";

        private string[] safechar;
        private string[] chr;
        private string[] anychar;

        private string str = null;
        private int index = 0;

        private void InitChars()
        {
            List<string> list = new List<string>();
            for (char c = '\u0021'; c < '\u00ff'; c++)
                if (c != '\u0022' && c != '\u002c')
                    list.Add(c.ToString());
            safechar = list.ToArray();

            list.Add(SPACE);
            chr = list.ToArray();

            list.Add(COMMA);
            list.Add(DDQUOTE);
            list.Add(CR);
            list.Add(LF);
            list.Add(TAB);
            anychar = list.ToArray();
        }

        public CSVParser(string str) 
        {
            this.str = str;
            InitChars();
        }

        private char this[int i]
        {
            get
            {
                return i < str.Length ? str[i] : (char) 0;
            }
        }

        private char Current
        {
            get { return this[index]; }
        }

        private bool End
        {
            get { return Current == 0; }
        }

        private void Next()
        {
            if (!End) index++;
        }

        private string Match(params string[] terms)
        {
            int i = index;
            foreach (string s in terms)
            {
                bool match = true;
                foreach (char c in s)
                    if (Current == c) Next();
                    else
                    {
                        index = i;
                        match = false;
                        break;
                    }
                if (match) return s;
            }
            return null;
        }

        private bool IsMatch(params string[] terms)
        {
            int i = index;
            string result = Match(terms);
            index = i;
            return result != null;
        }

        public DataTable Parse()
        {
            index = 0;
            DataTable table = new DataTable();
            IList<string> columnNames = ParseRecord();
            foreach (string column in columnNames)
                table.Columns.Add(column);
            while (!End)
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
            record.Add(ParseFieldColumn());
            while (!IsMatch(CRLF, CR, LF))
            {
                if (IsMatch(COMMA))
                {
                    Match(COMMA);
                    record.Add(ParseFieldColumn());
                }
                else throw new CSVParserException("Waited for comma or end of line at position " + index);
            }
            Match(CRLF, CR, LF);
            return record;
        }

        private string ParseFieldColumn()
        {
            if (IsMatch(DQUOTE))
            {   
                Match(DQUOTE);
                if (IsMatch(EXP)) Match(EXP);
                string fieldpayload = ParseFieldPayload();
                if (fieldpayload.Contains(DDQUOTE))
                    fieldpayload = fieldpayload.Replace(DDQUOTE, DQUOTE);
                if (IsMatch(DQUOTE))
                {
                    Match(DQUOTE);
                    return fieldpayload;
                }
                else 
                    throw new CSVParserException(string.Format("Waited for symbol {0} at position {1}", DQUOTE, index));
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
            while (IsMatch(anychar))
                s += Match(anychar);
            return s;
        }

        private string ParseRawFieldPayload()
        {
            string s = "";
            if (IsMatch(safechar))
                s += Match(safechar);
            else throw new CSVParserException("Unexpected symbol at position " + index);

            if (IsMatch(chr) || IsMatch(safechar))
            {
                while (IsMatch(chr))
                    s += Match(chr);

                if (IsMatch(safechar))
                    s += Match(safechar);
            }

            return s;
        }
    }
}
