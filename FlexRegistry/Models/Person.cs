using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexRegistry.Models
{
    public class Person
    {
        private string _searchValuesString;
        private string _visibleValue;
        public ObjectId Id { get; set; }
        public IEnumerable<FieldData> BasicFields { get; set; } = new List<FieldData>();
        public IEnumerable<FieldData> AdditionalFields { get; set; } = new List<FieldData>();
        [BsonIgnore]
        public string VisibleValue => _visibleValue;
        [BsonIgnore]
        public string ConcatenatedValues => _searchValuesString;
        [BsonIgnore]
        public bool IsSelected { get; set; }


        public void Refresh()
        {
            var settings = BasicSettings.Instance();
            List<string> visibleValues = new List<string>();
            List<string> values = new List<string>();

            bool noVis = settings.VisibleFields == null || settings.VisibleFields.Count() == 0;

            bool hasVis = false;

            foreach (var field in BasicFields)
            {
                if (string.IsNullOrWhiteSpace(field.Value)) continue;
                values.Add(field.Value.ToLower());
                if (!hasVis)
                    visibleValues.Add(field.Value);
                if (settings.VisibleFields.Contains(field.Key) || noVis)
                {
                    if (!hasVis)
                    {
                        hasVis = true;
                        visibleValues.Clear();
                    }
                    visibleValues.Add(field.Value);
                }
            }
            noVis = visibleValues.Count == 0;
            foreach (var field in AdditionalFields)
            {
                if (string.IsNullOrWhiteSpace(field.Value)) continue;
                values.Add(field.Value.ToLower());
                if(noVis)
                    visibleValues.Add(field.Value);
            }

            _searchValuesString = string.Join(" ", values);
            _visibleValue = visibleValues.Count == 0 ? "[Пусто]" : string.Join(" ", visibleValues);
        }
    }
}
