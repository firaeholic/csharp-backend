using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Backend_Web
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("_id")]
        public ObjectId? Id { get; set; }

        [BsonElement("fullname")]
        public string FullName { get; set; }

        [BsonElement("username")]
        public string UserName { get; set; }

        [BsonElement("email")]
        public string Email { get; set; }

        [BsonElement("passwordHash")]
        public string PasswordHash { get; set; }

        [BsonElement("generatedSalt")]
        public string GeneratedSalt { get; set; }

        public User(string passwordHash, string generatedSalt, string fullname, string email, string username)
        {
            PasswordHash = passwordHash;
            GeneratedSalt = generatedSalt;
            FullName = fullname;
            Email = email;
            UserName = username;
        }

    }
}
