namespace ahello_backend.Models.Login
{
    public class EmailCon
    {
        public string Host { get; set; } = "smtp.gmail.com";

        public int Port { get; set; } = 587;

        public string Username { get; set; } = "otpe2o@gmail.com";

        public string Password { get; set; } = "fuou adty dpmy uato";

        public string DisplayName { get; set; } = "Ahello";

        public string FromEmail { get; set; } = "otpe2o@gmail.com";

        public bool EnableSsl { get; set; } = true;
    }
}
