using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CitizenRegistry.Models
{
    public class DatabaseItem
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public bool IsSelected { get; set; }

        public DatabaseItem(string name, string path)
        {
            Name = name;
            Path = path;
        }

        public override int GetHashCode()
        {
            return Path.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if(obj is DatabaseItem)
            {
                return ((DatabaseItem)obj).Path == this.Path;
            }
            return base.Equals(obj);
        }

        public override string ToString()
        {
            return Path;
        }
    }
}
