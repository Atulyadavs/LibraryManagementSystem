using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public TransactionService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<BookTransaction> IssueBookAsync(int bookId, int userId)
        {
            var book = await _context.Books.FindAsync(bookId);
            var user = await _context.Users.FindAsync(userId);

            if (book == null || user == null || book.AvailableCopies < 1)
                return null;

            // Check if user can borrow more books
            if (!await CanUserBorrowMoreBooksAsync(userId))
                return null;

            var transaction = new BookTransaction
            {
                BookId = bookId,
                UserId = userId,
                IssueDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(
                    _configuration.GetValue<int>("LoanSettings:LoanPeriodDays", 14)),
                Status = "Issued"
            };

            // Update book availability
            book.AvailableCopies--;
            _context.Books.Update(book);

            _context.BookTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            return transaction;
        }

        public async Task<BookTransaction> ReturnBookAsync(int transactionId)
        {
            var transaction = await _context.BookTransactions
                .Include(t => t.Book)
                .FirstOrDefaultAsync(t => t.TransactionId == transactionId);

            if (transaction == null || transaction.Status == "Returned")
                return null;

            transaction.ReturnDate = DateTime.Now;
            transaction.Status = "Returned";

            // Calculate fine if overdue
            if (transaction.ReturnDate > transaction.DueDate)
            {
                var overdueDays = (transaction.ReturnDate.Value - transaction.DueDate).Days;
                var finePerDay = _configuration.GetValue<decimal>("LoanSettings:FinePerDay", 1.00m);
                transaction.FineAmount = overdueDays * finePerDay;
            }

            // Update book availability
            transaction.Book.AvailableCopies++;
            _context.Books.Update(transaction.Book);

            _context.BookTransactions.Update(transaction);
            await _context.SaveChangesAsync();

            return transaction;
        }

        public async Task<List<BookTransaction>> GetTransactionsAsync()
        {
            return await _context.BookTransactions
                .Include(t => t.Book)
                .Include(t => t.User)
                .OrderByDescending(t => t.IssueDate)
                .ToListAsync();
        }

        public async Task<BookTransaction> GetTransactionByIdAsync(int id)
        {
            return await _context.BookTransactions
                .Include(t => t.Book)
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.TransactionId == id);
        }

        public async Task<List<BookTransaction>> GetUserTransactionsAsync(int userId)
        {
            return await _context.BookTransactions
                .Include(t => t.Book)
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.IssueDate)
                .ToListAsync();
        }

        public async Task<List<BookTransaction>> GetOverdueTransactionsAsync()
        {
            return await _context.BookTransactions
                .Include(t => t.Book)
                .Include(t => t.User)
                .Where(t => t.Status == "Issued" && t.DueDate < DateTime.Now)
                .OrderBy(t => t.DueDate)
                .ToListAsync();
        }

        public async Task<decimal> CalculateFineAsync(int transactionId)
        {
            var transaction = await GetTransactionByIdAsync(transactionId);
            if (transaction == null || transaction.Status == "Returned")
                return 0;

            if (DateTime.Now > transaction.DueDate)
            {
                var overdueDays = (DateTime.Now - transaction.DueDate).Days;
                var finePerDay = _configuration.GetValue<decimal>("LoanSettings:FinePerDay", 1.00m);
                return overdueDays * finePerDay;
            }

            return 0;
        }

        public async Task<int> GetIssuedBooksCountAsync()
        {
            return await _context.BookTransactions
                .CountAsync(t => t.Status == "Issued");
        }

        public async Task<bool> CanUserBorrowMoreBooksAsync(int userId)
        {
            var maxBooks = _configuration.GetValue<int>("LoanSettings:MaxBooksPerUser", 5);
            var currentIssued = await _context.BookTransactions
                .CountAsync(t => t.UserId == userId && t.Status == "Issued");

            return currentIssued < maxBooks;
        }
    }
}