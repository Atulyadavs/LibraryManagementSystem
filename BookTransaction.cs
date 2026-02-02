using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagementSystem.Models
{
    public class BookTransaction
    {
        

        [Key]
        public int TransactionId { get; set; }

        [Required]
        [ForeignKey("Book")]
        public int BookId { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime IssueDate { get; set; } = DateTime.Now;

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime DueDate { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? ReturnDate { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Issued";

        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(10,2)")]
        public decimal FineAmount { get; set; } = 0;

        // Navigation properties
        public virtual Book? Book { get; set; }
        public virtual User? User { get; set; }

        [NotMapped]
        public bool IsOverdue => Status == "Issued" && DateTime.Now > DueDate;

     
    }
}