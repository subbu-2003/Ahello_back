namespace ahello_backend.Models.DigitalBook
{
    public class DigitalBookCreateDto
    {
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public IFormFile PreviewImage { get; set; }
        public IFormFile PdfFile { get; set; }

        public string Status { get; set; } = "Draft";
        public decimal Price { get; set; }

        public int CreatedBy { get; set; }
    }
}
