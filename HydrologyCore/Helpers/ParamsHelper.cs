using System.Data;
using System.Text;

namespace Helpers
{
    public class ParamsHelper
    {
        DataTable _paramsTable;
        public ParamsHelper(DataTable paramsTable)
        {
            _paramsTable = paramsTable;
        }

        public T GetValue<T>(string name)
        {
            T t = default(T);
            foreach (DataRow r in _paramsTable.Rows)
            {
                if (r["Name"].ToString() == name)
                {
                    t = (T)System.Convert.ChangeType(r["Value"].ToString(), typeof(T));
                    break;
                }
            }
            return t;
        }
            
    }
}
