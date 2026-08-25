using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Domain.Entities
{
    [Table("books_lists")]
    public class BookList : BaseModel
    {
        [PrimaryKey("id", false)]
        public string Id { get; set; } = default!;

        [Column("profile_id")]
        public string ProfileId { get; set; } = default!;

        [Column("list_name")]
        public string ListName { get; set; } = default!;

        [Column("list_description")]
        public string? ListDescription { get; set; }

        [Column("books_ids")]
        public List<string> BooksIds { get; set; } = new();

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }
    }
}
