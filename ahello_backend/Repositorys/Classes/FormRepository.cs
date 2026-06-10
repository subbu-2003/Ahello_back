using ahello_backend.DbContexts;
using ahello_backend.Models.Form;
using ahello_backend.Repositorys.Interfaces;
using Dapper;

namespace ahello_backend.Repositorys.Classes
{
    public class FormRepository : IFormRepository
    {
        private readonly DbContext _db;

        public FormRepository(DbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Form>> GetAllAsync()
        {
            var query = @"SELECT * FROM forms";

            using var connection = _db.GetConnection();

            return await connection.QueryAsync<Form>(query);
        }

        public async Task<Form> GetByIdAsync(int formId)
        {
            var query = @"SELECT * FROM forms
                          WHERE FormId = @FormId";

            using var connection = _db.GetConnection();

            return await connection.QueryFirstOrDefaultAsync<Form>(
                query,
                new { FormId = formId }
            );
        }
        public async Task<IEnumerable<Form>> GetByUserIdAsync(int userId)
        {
            var query = @"SELECT * FROM forms
                  WHERE UserId = @UserId";

            using var connection = _db.GetConnection();

            return await connection.QueryAsync<Form>(
                query,
                new { UserId = userId }
            );
        }
        public async Task<int> CreateAsync(FormCreate model)
        {
            if (model.UserId == model.ClientId)
            {
                throw new Exception("UserId and ClientId cannot be same.");
            }
            var query = @"INSERT INTO forms
                        (
                            UserId,
                            ClientId,
                            Title,
                            Description,
                            IsActive,
                            CreatedAt,
                            CreatedBy
                        )
                        VALUES
                        (
                            @UserId,
                            @ClientId,
                            @Title,
                            @Description,
                            @IsActive,
                            NOW(),
                            @CreatedBy
                        )";

            using var connection = _db.GetConnection();

            return await connection.ExecuteAsync(query, model);
        }

        public async Task<int> UpdateAsync(FormUpdate model)
        {
            if (model.UserId == model.ClientId)
            {
                throw new Exception("UserId and ClientId cannot be same.");
            }
            var query = @"UPDATE forms
                          SET
                              UserId = @UserId,
                              ClientId = @ClientId,
                              Title = @Title,
                              Description = @Description,
                              IsActive = @IsActive,
                              ModifiedAt = NOW(),
                              ModifiedBy = @ModifiedBy
                          WHERE FormId = @FormId";

            using var connection = _db.GetConnection();

            return await connection.ExecuteAsync(query, model);
        }

        public async Task<int> DeleteAsync(int formId)
        {
            var query = @"DELETE FROM forms
                          WHERE FormId = @FormId";

            using var connection = _db.GetConnection();

            return await connection.ExecuteAsync(
                query,
                new { FormId = formId }
            );
        }
    }
}