using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreInterfaces;
using System.Data;

namespace HydrologyCore
{
    public class Context : IContext
    {
        public Context()
        {
            _history = new List<IExperimentNode>();
            _historyStack = new Stack<List<IExperimentNode>>();
            InitialData = new DataSet();
        }
        List<IExperimentNode> _history;

        Stack<List<IExperimentNode>> _historyStack;

        public List<IExperimentNode> History
        {
            get
            { 
                return _history; 
            }
        }

        IExperimentNode Top
        {
            get
            {
                if (History.Count == 0)
                    throw new ApplicationException("History is empty, can not find the latest node");
                return History[History.Count - 1];
            }
        }

        public DataSet InitialData
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
        /// <param name="name">Имя алгоритма</param>
        /// <returns></returns>
        public DataSet GetData(string name)
        {
            //todo
            IExperimentNode node = Top;
            while (node.Name != name && node.Prev != null)
            {
                node = node.Prev;
            }
            if (node.Name == name)
                return node.Results;
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
            if (step > HistoryLength || step <= 0) 
                throw new IndexOutOfRangeException(string.Format("Can not find step {0}", step));
            if (History[step].Name != algorithmName) 
                throw new ApplicationException(string.Format("Algorithm on step {0} does not match {1}", step, algorithmName));
            return History[step].Results;
        }

        /// <summary>
        /// Получение результата работы алгоритма по номеру шага в истории вызовов
        /// </summary>
        /// <param name="step">Номер шага в истории</param>
        /// <returns></returns>
        public DataSet GetData(int step)
        {
            if (step == 0) 
                return InitialData;
            if (step > HistoryLength || step < 0) 
                throw new IndexOutOfRangeException(string.Format("Can not find step {0}", step));
            return History[step].Results;
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

        /// <summary>
        /// Pushes the context history one level down
        /// </summary>
        public void PushHistory()
        {
            _historyStack.Push(_history);
            List<IExperimentNode> newHistory = new List<IExperimentNode>(_history);
            _history = newHistory;
        }

        /// <summary>
        /// Pops the context history one level up
        /// </summary>
        public void PopHistory()
        {
            _history = _historyStack.Pop();
        }


        public DataTable FindInHistory(string tableName)
        {
            if (_history == null || _history.Count == 0)
                return null;

            foreach (var en in _history)
            {
                if (en.Results.Tables.Contains(tableName))
                    return en.Results.Tables[tableName];
            }

            return null;
        }

    }
}
