using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Services
{
    public interface ITransactionService
    {
        Task<BookTransaction> IssueBookAsync(int bookId, int userId);
        Task<BookTransaction> ReturnBookAsync(int transactionId);
        Task<List<BookTransaction>> GetTransactionsAsync();
        Task<BookTransaction> GetTransactionByIdAsync(int id);
        Task<List<BookTransaction>> GetUserTransactionsAsync(int userId);
        Task<List<BookTransaction>> GetOverdueTransactionsAsync();
        Task<decimal> CalculateFineAsync(int transactionId);
        Task<int> GetIssuedBooksCountAsync();
        Task<bool> CanUserBorrowMoreBooksAsync(int userId);
    }
}