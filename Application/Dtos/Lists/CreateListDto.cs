namespace Application.Dtos.Lists
{
    public class CreateListDto
    {
        public string ListName { get; set; } = default!;
        public string? ListDescription { get; set; }
    }
}
