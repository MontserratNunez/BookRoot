namespace Application.Dtos.Lists
{
    public class BookListDto
    {
        public string Id { get; set; } = default!;
        public string ListName { get; set; } = default!;
        public string? ListDescription { get; set; }
        public required string ListOwnerUsername {get; set; }
        public List<string> BooksIds { get; set; } = new();
        public DateTime? CreatedAt { get; set; }
    }
}
