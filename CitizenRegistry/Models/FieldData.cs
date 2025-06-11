using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CitizenRegistry.Models
{
    public class FieldData
    {
        public string Key { get; set; }
        public string Value { get; set; }
        [LiteDB.BsonIgnore]
        public string DisplayFormat => BasicSettings.Instance().RequiredFields.Contains(Key) ? $"{Key}*" : Key;

        public FieldData(string key, string value = null)
        {
            Key = key;
            Value = value;
        }

        public FieldData Clone()
        {
            return new FieldData(Key, Value);
        }
    }
}
