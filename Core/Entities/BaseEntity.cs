// Core/Entities/BaseEntity.cs
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace venar_bus_api_jakar_bckd_net.Core.Entities
{
    public abstract class BaseEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
        
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? UpdatedAt { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
}