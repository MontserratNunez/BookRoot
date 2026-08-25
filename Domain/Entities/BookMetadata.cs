using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Domain.Entities
{
    [Table("books_metadata")]
    public class BookMetadata : BaseModel
    {
        [PrimaryKey("id", false)]
        public string Id { get; set; } = default!;

        [Column("title")]
        public string Title { get; set; } = default!;

        [Column("author")]
        public string Author { get; set; } = default!;

        [Column("cover_edition_key")]
        public string? CoverEditionKey { get; set; }

        [Column("book_work_key")]
        public string? BookWorkKey { get; set; }

        [Column("year")]
        public int Year { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }
    }
}
