using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagementSystem.Models
{
    public class Book
    {

       
        [Key]
        public int BookId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string? Title { get; set; }

        [Required(ErrorMessage = "Author is required")]
        [StringLength(100, ErrorMessage = "Author name cannot exceed 100 characters")]
        public string? Author { get; set; }

        [Required(ErrorMessage = "ISBN is required")]
        [StringLength(20, ErrorMessage = "ISBN cannot exceed 20 characters")]
        [RegularExpression(@"^(?:\d{10}|\d{13})$", ErrorMessage = "Invalid ISBN format")]
        public string? ISBN { get; set; }

        [StringLength(100, ErrorMessage = "Publisher name cannot exceed 100 characters")]
        public string? Publisher { get; set; }

        [Range(1000, 9999, ErrorMessage = "Please enter a valid year")]
        public int? PublicationYear { get; set; }

        [StringLength(50, ErrorMessage = "Category cannot exceed 50 characters")]
        public string? Category { get; set; }

        [Required(ErrorMessage = "Total copies is required")]
        [Range(1, 1000, ErrorMessage = "Total copies must be between 1 and 1000")]
        public int TotalCopies { get; set; }
      

        [Required]
        [Range(0, 1000, ErrorMessage = "Available copies must be between 0 and 1000")]
        public int AvailableCopies { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}