using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;
using LibraryManagementSystem.Data;

namespace LibraryManagementSystem.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly ITransactionService _transactionService;
        private readonly IBookService _bookService;
        private readonly ApplicationDbContext _context;

        public TransactionsController(
            ITransactionService transactionService,
            IBookService bookService,
            ApplicationDbContext context)
        {
            _transactionService = transactionService;
            _bookService = bookService;
            _context = context;
        }

        // GET: Transactions
        public async Task<IActionResult> Index()
        {
            var transactions = await _transactionService.GetTransactionsAsync();
            return View(transactions);
        }


        // GET: Transactions/Issue
        // GET: Transactions/Issue
        public async Task<IActionResult> Issue()
        {
            ViewBag.Books = await _context.Books
                .Where(b => b.AvailableCopies > 0)
                .OrderBy(b => b.Title)
                .ToListAsync();

            ViewBag.Users = await _context.Users
                .Where(u => u.IsActive)
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .ToListAsync();

            // Add recent transactions if your view needs them
            ViewBag.RecentTransactions = await _context.BookTransactions
                .Include(t => t.Book)
                .Include(t => t.User)
                .OrderByDescending(t => t.IssueDate)
                .Take(10)
                .ToListAsync();

            // Choose ONE of these:
            return View("Transactions");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Issue(int bookId, int userId)
        {
            var transaction = await _transactionService.IssueBookAsync(bookId, userId);

            if (transaction == null)
            {
                TempData["ErrorMessage"] = "Unable to issue book. Please check availability or user limits.";
            }
            else
            {
                TempData["SuccessMessage"] = "Book issued successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Transactions/Return/5
        public async Task<IActionResult> Return(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _transactionService.GetTransactionByIdAsync(id.Value);
            if (transaction == null)
            {
                return NotFound();
            }

            // Calculate fine if any
            if (transaction.Status == "Issued")
            {
                ViewBag.FineAmount = await _transactionService.CalculateFineAsync(id.Value);
            }

            return View(transaction);
        }

        // POST: Transactions/Return/5
        [HttpPost, ActionName("Return")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReturnConfirmed(int id)
        {
            var transaction = await _transactionService.ReturnBookAsync(id);

            if (transaction == null)
            {
                TempData["ErrorMessage"] = "Unable to return book.";
            }
            else
            {
                TempData["SuccessMessage"] = transaction.FineAmount > 0
                    ? $"Book returned successfully! Fine: ${transaction.FineAmount}"
                    : "Book returned successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Transactions/Overdue
        public async Task<IActionResult> Overdue()
        {
            var overdueTransactions = await _transactionService.GetOverdueTransactionsAsync();
            return View(overdueTransactions);
        }

        // GET: Transactions/UserTransactions/5
        public async Task<IActionResult> UserTransactions(int userId)
        {
            var transactions = await _transactionService.GetUserTransactionsAsync(userId);
            ViewBag.User = await _context.Users.FindAsync(userId);

            return View(transactions);
        }

        // GET: Transactions/Statistics
        public async Task<IActionResult> Statistics()
        {
            var stats = new
            {
                TotalBooks = await _bookService.GetTotalBooksCountAsync(),
                AvailableBooks = await _bookService.GetAvailableBooksCountAsync(),
                IssuedBooks = await _transactionService.GetIssuedBooksCountAsync(),
                OverdueBooks = (await _transactionService.GetOverdueTransactionsAsync()).Count,
                TotalTransactions = await _context.BookTransactions.CountAsync(),
                TotalUsers = await _context.Users.CountAsync()

            };

            return View(stats);
        }
    }
}