using System;
using System.Reflection;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CsvParser;
using System.Xml.Linq;
using HydrologyCore.Context;
using CoreInterfaces;

namespace HydrologyCore.Experiment.Nodes
{
    public class AlgorithmNode : AbstractNode
    {
        private Type algorithmType;
        public Type AlgorithmType => algorithmType;

        public string DisplayedTypeName => algorithmType.GetCustomAttribute<NameAttribute>().Name;

        private List<Port> parameters = new List<Port>();
        public IList<Port> Parameters => parameters;

        private Dictionary<Port, object> valueParams = new Dictionary<Port, object>();
        public IDictionary<Port, object> ValueParams => valueParams;

        // Output
        private Dictionary<Port, bool> saveToFile;
        public IDictionary<Port, bool> SaveToFile => saveToFile;

        private Dictionary<Type, Type> genericTypeValues;
        public IDictionary<Type, Type> GenericTypeValues => genericTypeValues;

        public AlgorithmNode(string name, Type algorithmType, Block parent) : base(name, parent)
        {           
            this.algorithmType = algorithmType;

            if (algorithmType.IsGenericTypeDefinition)
            {
                genericTypeValues = algorithmType.GetGenericArguments().ToDictionary(x => x, x => typeof(int));
            }

            InitInputs();
            InitOutputs();
        }

        private void InitInputs()
        {
            foreach (PropertyInfo property in algorithmType.GetProperties())
            {
                var attr = property.GetCustomAttribute<InputAttribute>();
                if (attr != null)
                {
                    Port port = new Port(this, property.Name, attr.DisplayedName, attr.Description, property.PropertyType);
                    inPorts.Add(port);

                    if (port.DataType == DataType.VALUE)
                    {
                        port.Displayed = false;
                        parameters.Add(port);
                        var defaultValue = Activator.CreateInstance(port.ElementType);
                        if (attr.DefaultValue != null)
                        {
                            defaultValue = attr.DefaultValue;
                        }
                        valueParams.Add(port, defaultValue);
                    }                 
                }
            }
        }

        public object GetPortValue(Port port)
        {
            if (valueParams.ContainsKey(port))
                return valueParams[port];
            return null;
        }

        public void SetPortValue(Port port, string value)
        {
            object v = value;

            if (port.ElementType == typeof(double))
                v = Convert.ToDouble(value);
            else if (port.ElementType == typeof(int))
                v = Convert.ToInt32(value);

            if (valueParams.ContainsKey(port))
                valueParams[port] = v;
            else
                valueParams.Add(port, v);
        }

        public void SetSaveToFile(Port port, bool isSaveToFile)
        {
            if (saveToFile.ContainsKey(port))
                saveToFile[port] = isSaveToFile;
            else
                saveToFile.Add(port, isSaveToFile);
        }

        private void InitOutputs()
        {
            saveToFile = new Dictionary<Port, bool>();
            foreach (PropertyInfo property in algorithmType.GetProperties())
            {
                var attr = property.GetCustomAttribute<OutputAttribute>();
                if (attr != null)
                {
                    var name = attr.DisplayedName;
                    var description = attr.Description;
                    var type = property.PropertyType;
                    Port port = new Port(this, property.Name, name, description, type);
                    outPorts.Add(port);
                    saveToFile.Add(port, true);
                }
            }
        }

        private IAlgorithm CreateInstance()
        {
            Type type = algorithmType;
            if (algorithmType.IsGenericTypeDefinition && genericTypeValues != null)
            {
                Type[] typeArgs = new Type[GenericTypeValues.Keys.Count];
                for (int i = 0; i < GenericTypeValues.Keys.Count; i++)
                {
                    var key = GenericTypeValues.Keys.First(x => x.GenericParameterPosition == i);
                    typeArgs[i] = GenericTypeValues[key];
                }
                type = algorithmType.MakeGenericType(typeArgs);
            }
            return (IAlgorithm) Activator.CreateInstance(type);
        }

        public override void Run(IContext ctx)
        {
            var alg = CreateInstance();
            SetInputVariables(alg, ctx);
            alg.Run();
            GetOutputVariables(alg, ctx);
            SaveOutputToFiles(ctx);
        }

        private void SetInputVariables(IAlgorithm alg, IContext ctx)
        {
            foreach (PropertyInfo property in algorithmType.GetProperties())
            {
                var attr = property.GetCustomAttribute<InputAttribute>();
                Port port = inPorts.Find(p => p.Name == property.Name);
                if (attr != null)
                {
                    var value = ctx.GetPortValue(port);
                    // todo : convert
                    property.SetValue(alg, value);
                }
            }
        }

        private void GetOutputVariables(IAlgorithm alg, IContext ctx)
        {
            foreach (PropertyInfo property in algorithmType.GetProperties())
            {
                var attr = property.GetCustomAttribute<OutputAttribute>();
                Port port = inPorts.Find(p => p.Name == property.Name);
                if (attr != null)
                {
                    var value = property.GetValue(alg);
                    ctx.SetPortValue(port, value);
                }
            }
        }

        private void SaveOutputToFiles(IContext ctx)
        {
            IWriter writer = new CSVWriter();
            foreach (var entity in saveToFile)
            {
                string name = entity.Key.DisplayedName;
                bool save = entity.Value;
                if (save)
                {
                    var value = ctx.GetPortValue(entity.Key);
                    var path = Parent.Path + "/" + Name + (entity.Key.DataType == DataType.DATASET ? "/" : ".csv");
                    // todo : save + other datatypes
                    if (entity.Key.DataType == DataType.DATASET)
                    {
                        var ds = value as DataSet;
                        foreach (DataTable table in ds.Tables)
                        {
                            writer.Write(table, path + table.TableName + ".csv");
                        }
                    }
                    else if (entity.Key.DataType == DataType.DATATABLE)
                    {
                        var table = value as DataTable;
                        writer.Write(table, path);
                    } 
                }
            }
        }
    }
}
