using HydrologyCore.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydrologyCore.Experiment
{
    public class Port
    {
        public IRunable Owner { get; }

        private string name;
        public string Name {
            get { return name; }
            set
            {
                name = value;
                NameChanged?.Invoke(this, new NameChangedEventArgs(name));
            }
        }

        public string DisplayedName { get; set; }
        public string Description { get; set; }

        public bool Displayed { get; set; }
        
        public Type Type
        {
            get
            {
                return DataTypeHelpers.GetType(DataType, ElementType);
            }
            set
            {
                DataType = DataTypeHelpers.GetDataType(value);
                if (DataType == DataType.ARRAY)
                    ElementType = value.GetElementType();
                else if (DataType == DataType.MATRIX)
                    ElementType = value.GetElementType().GetElementType();
                else
                    ElementType = value;
            }
        }

        public DataType DataType { get; set; }
        public Type ElementType { get; set; }

        public event NameChangedEventHandler NameChanged;

        public Port(IRunable owner, string name, string displayedName, string description, Type type, bool displayed)
        {
            Owner = owner;
            this.name = name;
            DisplayedName = displayedName;
            Description = description;
            Displayed = displayed;
            Type = type;
        }

        public Port(IRunable owner, string name, string displayedName, string description, Type type) : this(owner, name, displayedName, description, type, true) { }

        public Port(IRunable owner, string name, string description, Type type, bool displayed) : this(owner, name, name, description, type, displayed) { }

        public Port(IRunable owner, string name, string description, Type type) : this(owner, name, name, description, type, true) { }


        public Port(IRunable owner, string name, string displayedName, string description, DataType dataType, Type elementType, bool displayed)
        {
            Owner = owner;
            this.name = name;
            DisplayedName = displayedName;
            Description = description;
            Displayed = displayed;
            DataType = dataType;
            ElementType = elementType;
        }

        public Port(IRunable owner, string name, string displayedName, string description, DataType dataType, Type elementType) 
            : this(owner, name, displayedName, description, dataType, elementType, true) { }

        public Port(IRunable owner, string name, string description, DataType dataType, Type elementType, bool displayed) 
            : this(owner, name, name, description, dataType, elementType, displayed) { }

        public Port(IRunable owner, string name, string description, DataType dataType, Type elementType) 
            : this(owner, name, name, description, dataType, elementType, true) { }

        public IList<Connection> GetConnections()
        {
            return Owner?.Parent.Connections.Where(c => c.From == this || c.To == this).ToList();
        }
    }
}
