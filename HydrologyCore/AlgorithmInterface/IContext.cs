using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace AlgorithmInterface
{
    public interface IContext
    {
        /// <summary>
        /// Результат выполнения последнего алгоритма, или исходные данные в случае если алгоритм первый
        /// </summary>
        DataSet Data
        { 
            get;
        }

        /// <summary>
        /// Получение последнего результата работы алгоритма по его имени
        /// </summary>
        /// <param name="algorithmName">Имя алгоритма</param>
        /// <returns></returns>
        DataSet GetData(string algorithmName);

        /// <summary>
        /// Получение результата работы алгоритма по его имени и номеру шага в истории вызовов
        /// </summary>
        /// <param name="algorithmName">Имя алгоритма</param>
        /// <param name="step">Номер шага в истории</param>
        /// <returns></returns>
        DataSet GetData(string algorithmName, int step);

        /// <summary>
        /// Получение результата работы алгоритма по номеру шага в истории вызовов
        /// </summary>
        /// <param name="step">Номер шага в истории</param>
        /// <returns></returns>
        DataSet GetData(int step);

        /// <summary>
        /// Длина предыдущей истории
        /// </summary>
        int HistoryLength
        {
            get;
        }
    }
}
