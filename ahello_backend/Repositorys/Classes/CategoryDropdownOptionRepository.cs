using ahello_backend.DbContexts;
using ahello_backend.Models.Category;
using ahello_backend.Repositorys.Interfaces;
using Dapper;

namespace ahello_backend.Repositorys.Classes
{
    public class CategoryDropdownOptionRepository
        : ICategoryDropdownOptionRepository
    {
        private readonly DbContext _db;

        public CategoryDropdownOptionRepository(
            DbContext db)
        {
            _db = db;
        }

        public async Task<int> CreateAsync(
            CreateCategoryDropdownOption model)
        {
            var sql = @"
            INSERT INTO categorydropdownoptions
            (
                CategoryFieldId,
                CategoryId,
                OptionValue,
                OptionLabel,
                IsActive,
                CreatedDate,
                CreatedBy
            )
            VALUES
            (
                @CategoryFieldId,
                @CategoryId,
                @OptionValue,
                @OptionLabel,
                @IsActive,
                NOW(),
                @CreatedBy
            );";

            using var connection = _db.GetConnection();

            int rows = 0;

            foreach (var opt in model.Options)
            {
                rows += await connection.ExecuteAsync(
                    sql,
                    new
                    {
                        model.CategoryFieldId,
                        model.CategoryId,
                        opt.OptionValue,
                        opt.OptionLabel,
                        opt.IsActive,
                        model.CreatedBy
                    });
            }

            return rows;
        }

        public async Task<bool> UpdateAsync(
            UpdateCategoryDropdownOption model)
        {
            var sql = @"
            UPDATE categorydropdownoptions
            SET
                OptionValue = @OptionValue,
                OptionLabel = @OptionLabel,
                IsActive = @IsActive,
                ModifiedDate = NOW(),
                ModifiedBy = @ModifiedBy
            WHERE CategoryDropDownId = @CategoryDropDownId";

            using var connection = _db.GetConnection();

            var rows = await connection.ExecuteAsync(
                sql,
                model);

            return rows > 0;
        }

        public async Task<GetCategoryDropdownOption>
            GetByIdAsync(int optionId)
        {
            var sql = @"
            SELECT *
            FROM categorydropdownoptions
            WHERE CategoryDropDownId = @optionId";

            using var connection = _db.GetConnection();

            return await connection
                .QueryFirstOrDefaultAsync<GetCategoryDropdownOption>(
                    sql,
                    new { optionId });
        }

        public async Task<IEnumerable<GetCategoryDropdownOption>>
            GetByFieldIdAsync(
                int categoryFieldId,
                int? categoryId)
        {
            var sql = @"
            SELECT *
            FROM categorydropdownoptions
            WHERE CategoryFieldId = @categoryFieldId
            AND (@categoryId IS NULL 
                 OR CategoryId = @categoryId)
            AND IsActive = 1";

            using var connection = _db.GetConnection();

            return await connection
                .QueryAsync<GetCategoryDropdownOption>(
                    sql,
                    new
                    {
                        categoryFieldId,
                        categoryId
                    });
        }
    }
}