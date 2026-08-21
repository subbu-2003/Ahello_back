namespace ahello_backend.Models.DigitalBook
{
    public class DigitalBookUpdateDto
    {
        public int DigitalBookId { get; set; }
        public int UserId { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }

        public IFormFile? PreviewImage { get; set; }
        public IFormFile? PdfFile { get; set; }

        public string Status { get; set; }
        public decimal Price { get; set; }

        public int ModifiedBy { get; set; }
    }
}
