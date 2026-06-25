namespace ahello_backend.DTO.Payment
{
    public class CreateStakeholderDto
    {
        public int UserId { get; set; }
        public string Pan { get; set; } = string.Empty;
        public string Dob { get; set; } = string.Empty;
    }
}
