using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CitizenRegistry.Models
{
    public class FileRecord
    {
        public ObjectId Id { get; set; }
        public ObjectId PersonId { get; set; }
        public string Hash { get; set; } //SHA-256 файла до шифрования
        public string OriginalName { get; set; }
        public DateTime AddedDate { get; set; }

        [BsonIgnore]
        public string ShortName => OriginalName.Length > 12 ? $"{OriginalName.Substring(0, 12)}..." : OriginalName;
        [BsonIgnore]
        public bool IsSelected { get; set; }
    }
}
