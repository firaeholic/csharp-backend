using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Backend_Web
{
    public class Complaint
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("_id")]
        public ObjectId? Id { get; set; }


        [BsonElement("complaint")]
        public string? ComplaintText { get; set; }

        [BsonElement("__v")]
        public int? V { get; set; }
    }
}
