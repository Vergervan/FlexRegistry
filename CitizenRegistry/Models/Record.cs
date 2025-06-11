using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;

namespace CitizenRegistry.Models
{
    public class Record
    {
        public ObjectId Id { get; set; }
        public ObjectId PersonId { get; set; }
        public string Text { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ChangedAt { get; set; }

        [BsonIgnore]
        public DateTime DisplayDate => ChangedAt == DateTime.MinValue ? CreatedAt : ChangedAt;

        public Record(ObjectId personId)
        {
            PersonId = personId;
        }
    }
}
