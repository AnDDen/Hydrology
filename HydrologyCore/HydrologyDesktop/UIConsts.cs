using HydrologyCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydrologyDesktop
{
    public class UIConsts
    {
        public const string INIT_NODE_NAME = "Инициализация";
        public const string LOOP_NODE_NAME = "Цикл";
        public const string BLOCK_NODE_NAME = "Блок";
        public const string PORT_NODE_NAME = "Порт";
        public const string IN_PORT_NODE_NAME = "Входной параметр";
        public const string OUT_PORT_NODE_NAME = "Выходной параметр";

        public const string VALUE_TYPE_NAME = "Значение";
        public const string REFERENCE_TYPE_NAME = "Ссылка";
        public const string LOOP_TYPE_NAME = "Переменная цикла";

        public const string EXPERIMENT_NAME = "Эксперимент";

        public static Dictionary<DataType, string> DATA_TYPE_NAMES = new Dictionary<DataType, string>()
        {
            [DataType.VALUE] = "Значение",
            [DataType.ARRAY] = "Массив",
            [DataType.MATRIX] = "Матрица",
            [DataType.DATATABLE] = "Таблица",
            [DataType.DATASET] = "Массив таблиц"
        };

        public static Dictionary<Type, string> ELEMENT_TYPE_NAMES = new Dictionary<Type, string>()
        {
            [typeof(int)] = "Целое число",
            [typeof(double)] = "Вещественное число",
            [typeof(string)] = "Строка"
        };
    }
}
