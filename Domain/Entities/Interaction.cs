using Domain.Enums;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Domain.Entities
{
    [Table("user_interactions")]
    public class Interaction : BaseModel
    {
        [PrimaryKey("id", false)]
        public string Id { get; set; } = default!;

        [Column("user_id")]
        public string UserId { get; set; } = default!;

        [Column("book_id")]
        public string BookId { get; set; } = default!;

        [Column("status")]
        [JsonConverter(typeof(StringEnumConverter))]
        public InteractionStatus Status { get; set; }

        [Column("rating")]
        public int? Rating { get; set; }

        [Column("finished_at")]
        public DateTime? FinishedAt { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }
    }
}


