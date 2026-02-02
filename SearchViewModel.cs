namespace LibraryManagementSystem.Models
{
    public class SearchViewModel
    {
        public string? SearchTerm { get; set; }
        public string SearchBy { get; set; } = "Title";
        public List<Book> Results { get; set; } = new List<Book>();
    }
}