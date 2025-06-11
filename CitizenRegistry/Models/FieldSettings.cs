using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CitizenRegistry.Models
{
    public class FieldSettings
    {
        public string Name { get; set; }
        public bool IsVisible { get; set; }
        public bool IsRequired { get; set; }
        public FieldSettings(string name) => Name = name;
    }
}
