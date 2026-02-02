using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Services
{
    public class BookService : IBookService
    {
        private readonly ApplicationDbContext _context;

        public BookService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Book>> GetAllBooksAsync()
        {
            return await _context.Books.OrderBy(b => b.Title).ToListAsync();
        }

        public async Task<Book> GetBookByIdAsync(int id)
        {
            return await _context.Books.FindAsync(id);
        }

        public async Task<Book> GetBookByISBNAsync(string isbn)
        {
            return await _context.Books.FirstOrDefaultAsync(b => b.ISBN == isbn);
        }

        public async Task<Book> AddBookAsync(Book book)
        {
            book.AvailableCopies = book.TotalCopies;
            _context.Books.Add(book);
            await _context.SaveChangesAsync();
            return book;
        }

        public async Task<Book> UpdateBookAsync(Book book)
        {
            var existingBook = await _context.Books.FindAsync(book.BookId);
            if (existingBook == null)
                return null;

            // Calculate new available copies
            int issuedCopies = existingBook.TotalCopies - existingBook.AvailableCopies;
            book.AvailableCopies = Math.Max(book.TotalCopies - issuedCopies, 0);

            _context.Entry(existingBook).CurrentValues.SetValues(book);
            await _context.SaveChangesAsync();
            return book;
        }

        public async Task DeleteBookAsync(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Book>> SearchBooksAsync(string searchTerm, string searchBy)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllBooksAsync();

            return searchBy.ToLower() switch
            {
                "title" => await _context.Books
                    .Where(b => b.Title.Contains(searchTerm))
                    .ToListAsync(),
                "author" => await _context.Books
                    .Where(b => b.Author.Contains(searchTerm))
                    .ToListAsync(),
                "isbn" => await _context.Books
                    .Where(b => b.ISBN.Contains(searchTerm))
                    .ToListAsync(),
                "category" => await _context.Books
                    .Where(b => b.Category.Contains(searchTerm))
                    .ToListAsync(),
                _ => await _context.Books
                    .Where(b => b.Title.Contains(searchTerm) ||
                               b.Author.Contains(searchTerm) ||
                               b.ISBN.Contains(searchTerm))
                    .ToListAsync()
            };
        }

        public async Task<bool> BookExistsAsync(int id)
        {
            return await _context.Books.AnyAsync(e => e.BookId == id);
        }

        public async Task<int> GetTotalBooksCountAsync()
        {
            return await _context.Books.SumAsync(b => b.TotalCopies);
        }

        public async Task<int> GetAvailableBooksCountAsync()
        {
            return await _context.Books.SumAsync(b => b.AvailableCopies);
        }
    }
}