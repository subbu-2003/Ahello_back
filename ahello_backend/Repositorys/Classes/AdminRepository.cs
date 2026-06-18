using Dapper;
using ahello_backend.DbContexts;
using ahello_backend.Models.Admin;
using ahello_backend.Repositorys.Interfaces;


namespace ahello_backend.Repositorys.Classes
{
    public class AdminRepository : IAdminRepository
    {
        private readonly DbContext _db;

        public AdminRepository(DbContext db)
        {
            _db = db;
        }

        public async Task<AdminUser?> GetByUsernameAsync(string username)
        {
            string sql = @"SELECT * FROM adminusers 
                       WHERE Username = @Username AND IsActive = 1";

            return await _db.GetConnection().QueryFirstOrDefaultAsync<AdminUser>(
                sql, new { Username = username });
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            string sql = @"SELECT COUNT(1) FROM adminusers WHERE Username = @Username";
            int count = await _db.GetConnection().ExecuteScalarAsync<int>(sql, new { Username = username });
            return count > 0;
        }

        public async Task<int> CreateAsync(AdminUser admin)
        {
            string sql = @"
        INSERT INTO adminusers
        (Username, PasswordHash, FullName, Email, Phone, IsActive)
        VALUES
        (@Username, @PasswordHash, @FullName, @Email, @Phone, @IsActive);

        SELECT LAST_INSERT_ID();
        ";

            return await _db.GetConnection().ExecuteScalarAsync<int>(sql, admin);
        }
    }
}
