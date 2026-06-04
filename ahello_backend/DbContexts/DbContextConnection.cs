using MySqlConnector;
namespace ahello_backend.DbContexts
{
    public class DbContextConnection
    {
        private readonly IConfiguration _configuration;
        private readonly string? _mySqlconnectionString;
        public DbContextConnection(IConfiguration configuration)
        {
            _configuration = configuration;
            _mySqlconnectionString = configuration.GetConnectionString("MySqlConnection");
        }
        public MySqlConnection GetMyConnection()
        {
            return new MySqlConnection(_mySqlconnectionString);
        }
    }
}
