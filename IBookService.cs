using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Services
{
    public interface IBookService
    {
        Task<List<Book>> GetAllBooksAsync();
        Task<Book> GetBookByIdAsync(int id);
        Task<Book> GetBookByISBNAsync(string isbn);
        Task<Book> AddBookAsync(Book book);
        Task<Book> UpdateBookAsync(Book book);
        Task DeleteBookAsync(int id);
        Task<List<Book>> SearchBooksAsync(string searchTerm, string searchBy);
        Task<bool> BookExistsAsync(int id);
        Task<int> GetTotalBooksCountAsync();
        Task<int> GetAvailableBooksCountAsync();
    }
}