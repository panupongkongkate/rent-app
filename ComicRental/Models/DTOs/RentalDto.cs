namespace ComicRental.Models.DTOs
{
    public class BorrowBookDto
    {
        public int CustomerId { get; set; }
        public int BookId { get; set; }
        public int RentalDays { get; set; }
        public decimal DiscountPercent { get; set; } = 0;
        public string? Notes { get; set; }
    }

    public class ReturnBookDto
    {
        public int RentalId { get; set; }
        public string? Notes { get; set; }
    }

    public class ReturnWithFineDto
    {
        public int RentalId { get; set; }
        public decimal FineAmount { get; set; }
        public string? Notes { get; set; }
    }

    public class ForceReturnDto
    {
        public int RentalId { get; set; }
        public string? Notes { get; set; }
    }
}