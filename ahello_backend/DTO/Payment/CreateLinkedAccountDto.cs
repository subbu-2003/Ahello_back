namespace ahello_backend.DTO.Payment
{
    public class CreateLinkedAccountDto
    {
        public int UserId { get; set; }
        public string BusinessType { get; set; } = string.Empty;
        public string CustomerFacingBusinessName { get; set; } = string.Empty;
    }
}
