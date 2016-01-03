using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlgorithmInterface;
using System.Data;

namespace HydrologyCore
{
    public class Context : IContext
    {
        public Context()
        {
            _history = new List<AlgorithmNode>();
        }
        List<AlgorithmNode> _history;
        public List<AlgorithmNode> History
        {
            get
            { 
                return _history; 
            }
        }

        AlgorithmNode Top
        {
            get
            {
                if (History.Count == 0)
                    throw new ApplicationException("History is empty, can not find the latest node");
                return History[History.Count - 1];
            }
        }

        DataSet InitialData
        {
            get;
            set;
        }

        /// <summary>
        /// Результат выполнения последнего алгоритма, или исходные данные в случае если алгоритм первый
        /// </summary>
        public DataSet Data
        {
            get
            {
                return History.Count == 0 ? InitialData : Top.Results;
            }
        }

        /// <summary>
        /// Получение последнего результата работы алгоритма по его имени
        /// </summary>
        /// <param name="algorithmName">Имя алгоритма</param>
        /// <returns></returns>
        public DataSet GetData(string algorithmName)
        {
            return null;
        }

        /// <summary>
        /// Получение результата работы алгоритма по его имени и номеру шага в истории вызовов
        /// </summary>
        /// <param name="algorithmName">Имя алгоритма</param>
        /// <param name="step">Номер шага в истории</param>
        /// <returns></returns>
        public DataSet GetData(string algorithmName, int step)
        {

            //возможно метод лишний
            return null;
        }

        /// <summary>
        /// Получение результата работы алгоритма по номеру шага в истории вызовов
        /// </summary>
        /// <param name="step">Номер шага в истории</param>
        /// <returns></returns>
        public DataSet GetData(int step)
        {
            return null;
        }

        /// <summary>
        /// Длина предыдущей истории
        /// </summary>
        public int HistoryLength
        {
            get
            {
                return History.Count;
            }
        }

    }
}
