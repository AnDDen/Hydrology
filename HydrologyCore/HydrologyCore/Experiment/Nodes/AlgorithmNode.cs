using System;
using System.Reflection;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreInterfaces;
using System.IO;
using CsvParser;
using System.Xml.Linq;

namespace HydrologyCore.Experiment.Nodes
{
    public class AlgorithmNode : AbstractNode
    {
        private Type algorithmType;

        public string DisplayedTypeName => algorithmType.GetCustomAttribute<NameAttribute>().Name;

        private List<Port> parameters = new List<Port>();
        public IList<Port> Parameters => parameters;

        private Dictionary<Port, object> valueParams = new Dictionary<Port, object>();
        public IDictionary<Port, object> ValueParams => valueParams;

        // Output
        private Dictionary<Port, bool> saveToFile;
        public IDictionary<Port, bool> SaveToFile => saveToFile;

        public AlgorithmNode(string name, Type algorithmType, Block parent) : base(name, parent)
        {           
            this.algorithmType = algorithmType;
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
                        if (attr.DefaultValue != null)
                        {
                            valueParams.Add(port, attr.DefaultValue);
                        }
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
                valueParams[port] = value;
            else
                valueParams.Add(port, value);
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

        public override void Run(Context ctx)
        {
            var alg = (IAlgorithm)Activator.CreateInstance(algorithmType);
            SetInputVariables(alg, ctx);
            alg.Run();
            GetOutputVariables(alg, ctx);
            SaveOutputToFiles(ctx);
        }

        private void SetInputVariables(IAlgorithm alg, Context ctx)
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

        private void GetOutputVariables(IAlgorithm alg, Context ctx)
        {
            foreach (PropertyInfo property in algorithmType.GetProperties())
            {
                var attr = property.GetCustomAttribute<OutputAttribute>();
                Port port = inPorts.Find(p => p.Name == property.Name);
                if (attr != null)
                {
                    var value = property.GetValue(alg);
                    ctx.AddPortValue(port, value);
                }
            }
        }

        private void SaveOutputToFiles(Context ctx)
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
