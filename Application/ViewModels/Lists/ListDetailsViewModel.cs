namespace Application.ViewModels.Lists
{
    public class ListDetailsViewModel
    {
        public string ListId { get; set; } = default!;
        public string ListName { get; set; } = default!;
        public string? ListDescription { get; set; }
        public string OwnerUsername { get; set; } = default!;
        public bool IsOwner { get; set; }
        public List<ListBookItemViewModel> Books { get; set; } = new();
    }

    public class ListBookItemViewModel
    {
        public string BookWorkKey { get; set; } = default!;
        public string Title { get; set; } = default!;
        public string Author { get; set; } = default!;
        public string? CoverUrl { get; set; }
    }
}
