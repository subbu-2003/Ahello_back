namespace ahello_backend.DTO.Payment
{
    public class UpdateProductDto
    {
        public int UserId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string IfscCode { get; set; } = string.Empty;
        public string BeneficiaryName { get; set; } = string.Empty;
    }
}
