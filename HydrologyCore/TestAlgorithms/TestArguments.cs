using CoreInterfaces;
using System.Data;

namespace TestAlgorithms
{
    [Name("Test Arguments")]
    public class TestArgumentsAlgorithm : IAlgorithm
    {
        private DataSet results;

        [Argument(123)]
        public int IntArgument { get; set; }

        [Argument]
        public int IntArgument2 { get; set; }

        [Argument(123.456)]
        public double DoubleArgument { get; set; }

        [Argument]
        public double DoubleArgument2 { get; set; }

        [Argument("abc")]
        public string StringArgument { get; set; }

        [Argument]
        public string StringArgument2 { get; set; }

        public DataSet Results
        {
            get { return results; }
        }

        public void Init(DataSet data)
        {
            
        }

        public void Run(IContext ctx)
        {
            results = new DataSet();
            DataTable table = new DataTable("test_arguments_out");
            table.Columns.Add("Name");
            table.Columns.Add("Value");

            var row = table.NewRow();
            row["Name"] = "IntArgument";
            row["Value"] = IntArgument;
            table.Rows.Add(row);

            row = table.NewRow();
            row["Name"] = "IntArgument2";
            row["Value"] = IntArgument2;
            table.Rows.Add(row);

            row = table.NewRow();
            row["Name"] = "DoubleArgument";
            row["Value"] = DoubleArgument;
            table.Rows.Add(row);

            row = table.NewRow();
            row["Name"] = "DoubleArgument2";
            row["Value"] = DoubleArgument2;
            table.Rows.Add(row);

            row = table.NewRow();
            row["Name"] = "StringArgument";
            row["Value"] = StringArgument;
            table.Rows.Add(row);

            row = table.NewRow();
            row["Name"] = "StringArgument2";
            row["Value"] = StringArgument2;
            table.Rows.Add(row);

            results.Tables.Add(table);
        }
    }
}
