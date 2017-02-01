using System;
using System.Reflection;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreInterfaces;
using HydrologyCore.Data;
using System.IO;
using CsvParser;

namespace HydrologyCore.Experiment.Nodes
{
    public class AlgorithmNode : AbstractNode
    {
        private Type algorithmType;

        public string DisplayedTypeName
        {
            get
            {
                return algorithmType.GetCustomAttribute<NameAttribute>().Name;
            }
        }

        // Input
        private Dictionary<string, VariableInfo> inputInfo;
        public IDictionary<string, VariableInfo> InputInfo { get { return inputInfo; } }

        private Dictionary<string, VariableValue> inputValues;
        public IDictionary<string, VariableValue> InputValues { get { return inputValues; } }

        // Output
        private Dictionary<string, bool> saveToFile;
        public IDictionary<string, bool> SaveToFile { get { return saveToFile; } }

        public AlgorithmNode(string name, Type algorithmType, NodeContainer nodeContainer) : base(name, nodeContainer)
        {           
            this.algorithmType = algorithmType;
            inputInfo = new Dictionary<string, VariableInfo>();
            inputValues = new Dictionary<string, VariableValue>();
            InitInputVariables();
        }

        private void InitInputVariables()
        {
            foreach (PropertyInfo property in algorithmType.GetProperties())
            {
                var attr = property.GetCustomAttribute<InputAttribute>();
                if (attr != null)
                {
                    var type = property.PropertyType;
                    inputInfo.Add(property.Name, new VariableInfo(attr.DisplayedName, attr.Description, type));

                    if (attr.DefaultValue != null)
                    {
                        object defVal = attr.DefaultValue;
                        inputValues.Add(property.Name, new VariableValue(VariableType.VALUE, defVal.ToString()));
                    }                    
                }
            }
        }

        private void InitOutputInfo()
        {
            foreach (PropertyInfo property in algorithmType.GetProperties())
            {
                var attr = property.GetCustomAttribute<OutputAttribute>();
                if (attr != null)
                {
                    var name = attr.DisplayedName;
                    var description = attr.Description;
                    var type = property.PropertyType;
                    outputInfo.Add(name, new VariableInfo(name, description, type));
                    saveToFile.Add(name, true);
                }
            }
        }

        public override bool DependsOn(AbstractNode node)
        {
            foreach (var v in inputValues.Values)
            {
                if (v.VariableType != VariableType.VALUE)
                {
                    var nodeName = v.GetReferenceInfo().Item1;
                    if (nodeName == node.Name)
                        return true;
                }
            }
            return false;
        }

        public override void Run()
        {
            var alg = (IAlgorithm)Activator.CreateInstance(algorithmType);
            SetInputVariables(alg);
            alg.Run();
            GetOutputVariables(alg);
            SaveOutputToFiles();
        }

        private void SetInputVariables(IAlgorithm alg)
        {
            foreach (PropertyInfo property in algorithmType.GetProperties())
            {
                var attr = property.GetCustomAttribute<InputAttribute>();
                if (attr != null)
                {
                    var value = inputValues[property.Name];
                    property.SetValue(alg, value.GetValue(NodeContainer, property.PropertyType));
                }
            }
        }

        private void GetOutputVariables(IAlgorithm alg)
        {
            output = new Dictionary<string, object>();
            foreach (PropertyInfo property in algorithmType.GetProperties())
            {
                var attr = property.GetCustomAttribute<OutputAttribute>();
                if (attr != null)
                {
                    var value = property.GetValue(alg);
                    output.Add(property.Name, value);
                }
            }
        }

        private void SaveOutputToFiles()
        {
            IWriter writer = new CSVWriter();
            foreach (var entity in saveToFile)
            {
                string name = entity.Key;
                bool save = entity.Value;
                if (save)
                {
                    var variableInfo = outputInfo[name];
                    var value = GetVarValue(name);
                    var path = NodeContainer.Path + "/" + Name + (variableInfo.DataType == DataType.DATASET ? "/" : ".csv");
                    // todo : save + other datatypes
                    if (variableInfo.DataType == DataType.DATASET)
                    {
                        var ds = value as DataSet;
                        foreach (DataTable table in ds.Tables)
                        {
                            writer.Write(table, path + table.TableName + ".csv");
                        }
                    }
                    else if (variableInfo.DataType == DataType.DATATABLE)
                    {
                        var table = value as DataTable;
                        writer.Write(table, path);
                    } 
                }
            }
        }        
    }
}
