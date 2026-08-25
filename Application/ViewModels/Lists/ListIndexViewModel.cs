using System.ComponentModel.DataAnnotations;

namespace Application.ViewModels.Lists
{
    public class ListIndexViewModel
    {
        public string Username { get; set; } = default!;
        public bool IsOwner { get; set; }
        public List<ListSummaryViewModel> Lists { get; set; } = new();
    }

    public class ListSummaryViewModel
    {
        public string Id { get; set; } = default!;
        public string ListName { get; set; } = default!;
        public string? ListDescription { get; set; }
        public int BookCount { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
