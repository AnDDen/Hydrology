using System;

namespace CoreInterfaces
{
    [AttributeUsage(AttributeTargets.Property)]
    public class OutputAttribute : Attribute
    {
        public string DisplayedName { get; set; }
        public string Description { get; set; }

        public OutputAttribute(string displayedName, string description)
        {
            DisplayedName = displayedName;
            Description = description;
        }

        public OutputAttribute(string displayedName) : this(displayedName, "") { }
    }
}
