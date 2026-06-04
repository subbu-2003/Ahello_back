using ahello_backend.DbContexts;
using ahello_backend.Models.Category;
using ahello_backend.Repositorys.Interfaces;
using Dapper;

namespace ahello_backend.Repositorys.Classes
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly DbContext _db;

        public CategoryRepository(DbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            var query = @"SELECT * FROM Categories";

            using var connection = _db.GetConnection();

            return await connection.QueryAsync<Category>(query);
        }

        public async Task<Category> GetByIdAsync(int categoryId)
        {
            var query = @"SELECT * FROM Categories
                          WHERE CategoryId = @CategoryId";

            using var connection = _db.GetConnection();

            return await connection.QueryFirstOrDefaultAsync<Category>(
                query,
                new { CategoryId = categoryId }
            );
        }

        public async Task<int> CreateAsync(CategoryCreate model)
        {
            var query = @"INSERT INTO Categories
                        (
                            CategoryName,
                            CreatedAt,
                            CreatedBy,
                            ModifiedAt,
                            ModifiedBy
                        )
                        VALUES
                        (
                            @CategoryName,
                            NOW(),
                            @CreatedBy,
                            NOW(),
                            @CreatedBy
                        )";

            using var connection = _db.GetConnection();

            return await connection.ExecuteAsync(query, model);
        }

        public async Task<int> UpdateAsync(CategoryUpdate model)
        {
            var query = @"UPDATE Categories
                          SET
                              CategoryName = @CategoryName,
                              ModifiedAt = NOW(),
                              ModifiedBy = @ModifiedBy
                          WHERE CategoryId = @CategoryId";

            using var connection = _db.GetConnection();

            return await connection.ExecuteAsync(query, model);
        }

        public async Task<int> DeleteAsync(int categoryId)
        {
            var query = @"DELETE FROM Categories
                          WHERE CategoryId = @CategoryId";

            using var connection = _db.GetConnection();

            return await connection.ExecuteAsync(
                query,
                new { CategoryId = categoryId }
            );
        }
    }
}
