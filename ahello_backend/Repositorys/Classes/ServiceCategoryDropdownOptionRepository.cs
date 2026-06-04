using ahello_backend.DbContexts;
using ahello_backend.Models.Service;
using ahello_backend.Repositorys.Interfaces;
using Dapper;

namespace ahello_backend.Repositorys.Classes
{
    public class ServiceCategoryDropdownOptionRepository : IServiceCategoryDropdownOptionRepository
    {
        private readonly DbContext _db;

        public ServiceCategoryDropdownOptionRepository(DbContext db)
        {
            _db = db;
        }

        public async Task<int> AddAsync(ServiceCategoryDropdownOption model)
        {
            using var connection = _db.GetConnection();

            string query = @"
                INSERT INTO ServiceCategoryDropdownOption
                (
                    ServiceCategoryFieldId,
                    ServiceCategoryId,
                    OptionValue,
                    OptionLabel,
                    IsActive,
                    CreatedDate,
                    CreatedBy
                )
                VALUES
                (
                    @ServiceCategoryFieldId,
                    @ServiceCategoryId,
                    @OptionValue,
                    @OptionLabel,
                    @IsActive,
                    @CreatedDate,
                    @CreatedBy
                );

                SELECT LAST_INSERT_ID();
            ";

            return await connection.ExecuteScalarAsync<int>(query, model);
        }

        public async Task<int> UpdateAsync(ServiceCategoryDropdownOption model)
        {
            using var connection = _db.GetConnection();

            string query = @"
                UPDATE ServiceCategoryDropdownOption
                SET
                    ServiceCategoryFieldId = @ServiceCategoryFieldId,
                    ServiceCategoryId = @ServiceCategoryId,
                    OptionValue = @OptionValue,
                    OptionLabel = @OptionLabel,
                    IsActive = @IsActive,
                    ModifiedDate = @ModifiedDate,
                    ModifiedBy = @ModifiedBy
                WHERE ServiceCategoryDropDownId = @ServiceCategoryDropDownId
            ";

            return await connection.ExecuteAsync(query, model);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var connection = _db.GetConnection();

            string query = @"
                DELETE FROM ServiceCategoryDropdownOption
                WHERE ServiceCategoryDropDownId = @Id
            ";

            return await connection.ExecuteAsync(query, new { Id = id });
        }

        public async Task<ServiceCategoryDropdownOption?> GetByIdAsync(int id)
        {
            using var connection = _db.GetConnection();

            string query = @"
                SELECT *
                FROM ServiceCategoryDropdownOption
                WHERE ServiceCategoryDropDownId = @Id
            ";

            return await connection.QueryFirstOrDefaultAsync<ServiceCategoryDropdownOption>(
                query,
                new { Id = id });
        }

        public async Task<IEnumerable<ServiceCategoryDropdownOption>> GetAllAsync()
        {
            using var connection = _db.GetConnection();

            string query = @"
                SELECT *
                FROM ServiceCategoryDropdownOption
                ORDER BY ServiceCategoryDropDownId DESC
            ";

            return await connection.QueryAsync<ServiceCategoryDropdownOption>(query);
        }
    }
}
