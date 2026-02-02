using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;
using LibraryManagementSystem.Data;

namespace LibraryManagementSystem.Controllers
{
    public class BooksController : Controller
    {
        private readonly IBookService _bookService;
        private readonly ApplicationDbContext _context;

        public BooksController(IBookService bookService, ApplicationDbContext context)
        {
            _bookService = bookService;
            _context = context;
        }

        // GET: Books
        public async Task<IActionResult> Index(string searchTerm = "", string searchBy = "Title")
        {
            ViewData["SearchTerm"] = searchTerm;
            ViewData["SearchBy"] = searchBy;

            List<Book> books;
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                books = await _bookService.SearchBooksAsync(searchTerm, searchBy);
                ViewBag.Message = $"Found {books.Count} book(s)";
            }
            else
            {
                books = await _bookService.GetAllBooksAsync();
            }

            return View(books);
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _bookService.GetBookByIdAsync(id.Value);
            if (book == null)
            {
                return NotFound();
            }

            // Get transaction history for this book
            var transactions = await _context.BookTransactions
                .Include(t => t.User)
                .Where(t => t.BookId == id.Value)
                .OrderByDescending(t => t.IssueDate)
                .Take(10)
                .ToListAsync();

            ViewBag.Transactions = transactions;
            return View(book);
        }


        // GET: Books/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Books/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookId,Title,Author,ISBN,Publisher,PublicationYear,Category,TotalCopies")] Book book)
        {
            if (ModelState.IsValid)
            {
                // Check if ISBN already exists
                var existingBook = await _bookService.GetBookByISBNAsync(book.ISBN);
                if (existingBook != null)
                {
                    ModelState.AddModelError("ISBN", "A book with this ISBN already exists.");
                    return View(book);
                }

                await _bookService.AddBookAsync(book);
                TempData["SuccessMessage"] = "Book added successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(book);
        }

        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _bookService.GetBookByIdAsync(id.Value);
            if (book == null)
            {
                return NotFound();
            }
            return View(book);
        }

        // POST: Books/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookId,Title,Author,ISBN,Publisher,PublicationYear,Category,TotalCopies,AvailableCopies")] Book book)
        {
            if (id != book.BookId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _bookService.UpdateBookAsync(book);
                    TempData["SuccessMessage"] = "Book updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _bookService.BookExistsAsync(book.BookId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(book);
        }

        // GET: Books/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _bookService.GetBookByIdAsync(id.Value);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _bookService.DeleteBookAsync(id);
            TempData["SuccessMessage"] = "Book deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}