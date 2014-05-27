using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edge.Drivers
{
    public class DriverConfigPropertyAttribute : Attribute
    {
        public string Id { get; set; }
        public string Label { get; set; }
        public bool Required { get; set; }
        public string Description { get; set; }
        public Type ValueType { get; set; }

        public DriverConfigPropertyAttribute(string id, string label, bool required, string description, Type type)
        {
            this.Id = id;
            this.Label = label;
            this.Required = required;
            this.Description = description;
            this.ValueType = type;
        }
    }

    public class DevicePropertyAttribute : Attribute
    {
    }

    public class DeviceActionAttribute : Attribute
    {
    }

    public class DeviceEventAttribute : Attribute
    {
    }
}
