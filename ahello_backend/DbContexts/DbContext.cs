using System.Data;
using MySqlConnector;

namespace ahello_backend.DbContexts
{
    public class DbContext
    {
        public interface IDbContext
        {
            IDbConnection GetConnection();
        }
        private readonly IConfiguration _configuration;
        private readonly string? _mySqlconnectionString;

        public DbContext(IConfiguration configuration)
        {
            _configuration = configuration;
            _mySqlconnectionString = configuration.GetConnectionString("MySqlConnection");
        }
        public IDbConnection GetConnection()
        {
            return new MySqlConnection(_mySqlconnectionString);

        }
    }
}
