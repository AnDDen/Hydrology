using HydrologyCore.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydrologyDesktop
{
    public class VariableTypeHelper
    {
        public static string VariableTypeToString(VariableType varType)
        {
            switch (varType)
            {
                case VariableType.VALUE:
                    return UIConsts.VALUE_TYPE_NAME;
                case VariableType.REFERENCE:
                    return UIConsts.REFERENCE_TYPE_NAME;
                case VariableType.LOOP:
                    return UIConsts.LOOP_TYPE_NAME;
            }
            return null;
        }

        public static VariableType StringToVariableType(string str)
        {
            switch(str)
            {
                case UIConsts.VALUE_TYPE_NAME:
                    return VariableType.VALUE;
                case UIConsts.REFERENCE_TYPE_NAME:
                    return VariableType.REFERENCE;
                case UIConsts.LOOP_TYPE_NAME:
                    return VariableType.LOOP;
            }
            throw new ArgumentException(string.Format("Can't find VaribleType with name {0}", str));
        }

        public static IList<string> GetAllVariableTypeStrings()
        {
            IList<string> list = new List<string>();
            list.Add(UIConsts.VALUE_TYPE_NAME);
            list.Add(UIConsts.REFERENCE_TYPE_NAME);
            list.Add(UIConsts.LOOP_TYPE_NAME);
            return list;
        }
    }
}
