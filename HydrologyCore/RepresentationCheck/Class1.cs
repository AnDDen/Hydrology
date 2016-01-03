using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlgorithmInterface;

namespace RepresentationCheck
{
    public class RepresentationCheck : IAlgorithm
    {
        DataSet data;
        DataSet resultSet;

        public void Init(DataSet data)
        {
            this.data = data;
        }

        public void Run(IContext ctx)
        {
            resultSet = data;
        }

        public DataSet Results
        {
            get { return resultSet; }
        }
    }
}
