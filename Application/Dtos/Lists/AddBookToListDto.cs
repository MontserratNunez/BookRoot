namespace Application.Dtos.Lists
{
    public class AddBookToListDto
    {
        public string ListId { get; set; } = default!;
        public string BookWorkKey { get; set; } = default!;
    }
}
