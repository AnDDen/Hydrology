﻿using System;
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
            InitialData = new DataSet();
        }
        List<IExperimentNode> _history;
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

        IContext _parentCtx;

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

        public Context(IContext parentCtx = null)
        {
            _parentCtx = parentCtx;
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
            if (_parentCtx != null)
                return _parentCtx.GetData(name);
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
    }
}
