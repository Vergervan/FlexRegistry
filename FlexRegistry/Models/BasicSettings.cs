using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexRegistry.Models
{
    public class BasicSettings
    {
        [LiteDB.BsonId]
        public int Id { get; } = 1;
        private List<FieldSettings> _basicFields = new List<FieldSettings>();
        private static BasicSettings _instance = null;
        private BasicSettings() { }
        public static BasicSettings Instance()
        {
            if (_instance == null)
                _instance = new BasicSettings();
            return _instance;
        }

        public static void Reset() => _instance = null;

        public IEnumerable<FieldSettings> BasicFields
        {
            get => _basicFields;
            set
            {
                _basicFields = value.ToList();
                RefreshSettings();
            }
        }
        [LiteDB.BsonIgnore]
        public ISet<string> VisibleFields { get; private set; }
        [LiteDB.BsonIgnore]
        public ISet<string> RequiredFields { get; private set; }

        public void RefreshSettings()
        {
            VisibleFields = BasicFields.Where(x => x.IsVisible).Select(x => x.Name).ToHashSet();
            RequiredFields = BasicFields.Where(x => x.IsRequired).Select(x => x.Name).ToHashSet();
        }
    }
}
