using ahello_backend.DbContexts;
using ahello_backend.Models.Category;
using ahello_backend.Repositorys.Interfaces;
using Dapper;

namespace ahello_backend.Repositorys.Classes
{
    public class CategoryDynamicRepository : ICategoryDynamicRepository
    {
        private readonly DbContext _db;

        public CategoryDynamicRepository(DbContext db)
        {
            _db = db;
        }

        public async Task<PagedCategoryDynamicResponse> GetAllAsync(
        int pageNumber,
        int pageSize,
        string? search = null,
        DateTime? createdDate = null)
        {
            using var connection = _db.GetConnection();

            var offset = (pageNumber - 1) * pageSize;

            var whereClause = "WHERE 1=1";

            if (!string.IsNullOrWhiteSpace(search))
            {
                whereClause += " AND c.CategoryName LIKE @Search";
            }

            if (createdDate.HasValue)
            {
                whereClause += " AND DATE(c.CreatedAt) = @CreatedDate";
            }

            var totalRecords = await connection.ExecuteScalarAsync<int>(
                $@"SELECT COUNT(*)
                   FROM categories c
                   {whereClause}",
                new
                {
                    Search = $"%{search}%",
                    CreatedDate = createdDate?.Date
                });

            var categories = (await connection.QueryAsync<CategoryDynamicGetResponse>(
                $@"SELECT
                c.CategoryId,
                c.CategoryName,
                c.CreatedBy,
                c.CreatedAt,
                c.ModifiedBy,
                c.ModifiedAt,
                c.IsActive
                  FROM categories c
                  {whereClause}
                  ORDER BY c.CategoryId DESC
                  LIMIT @PageSize OFFSET @Offset",
                new
                {
                    Search = $"%{search}%",
                    CreatedDate = createdDate?.Date,
                    PageSize = pageSize,
                    Offset = offset
                })).ToList();

            foreach (var category in categories)
            {
                var fields = await connection.QueryAsync<CategoryDynamicFieldResponse>(
                    @"SELECT
                cf.CategoryFieldId,
                cf.FieldName,
                cf.FieldCode,
                cfv.FieldValue
                FROM categoryfieldvalues cfv
                INNER JOIN categoryfields cf
                ON cfv.CategoryFieldId = cf.CategoryFieldId
                 WHERE cfv.CategoryId = @CategoryId",
                    new { CategoryId = category.CategoryId });

                category.Fields = fields.ToList();
            }

            return new PagedCategoryDynamicResponse
            {
                TotalRecords = totalRecords,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Data = categories
            };
        }

        public async Task<CategoryDynamicGetResponse> GetByIdAsync(int categoryId)
        {
            using var connection = _db.GetConnection();

            var category = await connection.QueryFirstOrDefaultAsync<CategoryDynamicGetResponse>(
                @"SELECT
                    CategoryId,
                    CategoryName,
                    CreatedBy,
                    CreatedAt,
                     IsActive
                  FROM categories
                  WHERE CategoryId = @CategoryId",
                new { CategoryId = categoryId });

            if (category == null)
                return null;

            var fields = await connection.QueryAsync<CategoryDynamicFieldResponse>(
                @"SELECT
                    cf.CategoryFieldId,
                    cf.FieldName,
                    cf.FieldCode,
                    cfv.FieldValue
                  FROM categoryfieldvalues cfv
                  INNER JOIN categoryfields cf
                    ON cfv.CategoryFieldId = cf.CategoryFieldId
                  WHERE cfv.CategoryId = @CategoryId",
                new { CategoryId = categoryId });

            category.Fields = fields.ToList();

            return category;
        }

        public async Task<int> CreateAsync(CategoryDynamicPost model)
        {
            using var connection = _db.GetConnection();

            connection.Open();

            using var tx = connection.BeginTransaction();

            try
            {
                var categorySql = @"
                    INSERT INTO categories
                    (
                        CategoryName,
                        IsActive,
                        CreatedAt,
                        CreatedBy
                    )
                    VALUES
                    (
                        @CategoryName,
                        @IsActive,
                        NOW(),
                        @CreatedBy
                    );

                    SELECT LAST_INSERT_ID();";

                var categoryId = await connection.ExecuteScalarAsync<int>(
                    categorySql,
                    new
                    {
                        model.CategoryName,
                        model.IsActive,
                        model.CreatedBy
                    },
                    tx);

                if (model.Fields != null && model.Fields.Any())
                {
                    var fieldSql = @"
                        INSERT INTO categoryfieldvalues
                        (
                            CategoryId,
                            FieldCode,
                            CategoryFieldId,
                            FieldValue,
                            CreatedDate,
                            CreatedBy,
                            CreatedAt
                        )
                        SELECT
                            @CategoryId,
                            cf.FieldCode,
                            @CategoryFieldId,
                            @FieldValue,
                            NOW(),
                            @CreatedBy,
                            CURRENT_TIMESTAMP
                        FROM categoryfields cf
                        WHERE cf.CategoryFieldId = @CategoryFieldId;";

                    foreach (var field in model.Fields)
                    {
                        await connection.ExecuteAsync(
                            fieldSql,
                            new
                            {
                                CategoryId = categoryId,
                                CategoryFieldId = field.CategoryFieldId,
                                FieldValue = field.FieldValue,
                                model.CreatedBy
                            },
                            tx);
                    }
                }

                tx.Commit();

                return categoryId;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public async Task<bool> UpdateAsync(int categoryId, CategoryDynamicPut model)
        {
            using var connection = _db.GetConnection();

            connection.Open();

            using var tx = connection.BeginTransaction();

            try
            {
                var updateSql = @"
                    UPDATE categories
                    SET
                        CategoryName = @CategoryName,
                        IsActive = @IsActive,
                        ModifiedAt = NOW(),
                        ModifiedBy = @ModifiedBy
                    WHERE CategoryId = @CategoryId";

                var rows = await connection.ExecuteAsync(
                    updateSql,
                    new
                    {
                        CategoryId = categoryId,
                        model.CategoryName,
                        model.IsActive,
                        model.ModifiedBy
                    },
                    tx);

                if (rows <= 0)
                {
                    tx.Rollback();
                    return false;
                }

                await connection.ExecuteAsync(
                    @"DELETE FROM categoryfieldvalues
                      WHERE CategoryId = @CategoryId",
                    new { CategoryId = categoryId },
                    tx);

                if (model.Fields != null && model.Fields.Any())
                {
                    var fieldSql = @"
                        INSERT INTO categoryfieldvalues
                        (
                            CategoryId,
                            FieldCode,
                            CategoryFieldId,
                            FieldValue,
                            CreatedDate,
                            CreatedBy,
                            CreatedAt
                        )
                        SELECT
                            @CategoryId,
                            cf.FieldCode,
                            @CategoryFieldId,
                            @FieldValue,
                            NOW(),
                            @ModifiedBy,
                            CURRENT_TIMESTAMP
                        FROM categoryfields cf
                        WHERE cf.CategoryFieldId = @CategoryFieldId;";

                    foreach (var field in model.Fields)
                    {
                        await connection.ExecuteAsync(
                            fieldSql,
                            new
                            {
                                CategoryId = categoryId,
                                CategoryFieldId = field.CategoryFieldId,
                                FieldValue = field.FieldValue,
                                model.ModifiedBy
                            },
                            tx);
                    }
                }

                tx.Commit();

                return true;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int categoryId)
        {
            using var connection = _db.GetConnection();

            connection.Open();

            using var tx = connection.BeginTransaction();

            try
            {
                await connection.ExecuteAsync(
                    @"DELETE FROM categoryfieldvalues
                      WHERE CategoryId = @CategoryId",
                    new { CategoryId = categoryId },
                    tx);

                var rows = await connection.ExecuteAsync(
                    @"DELETE FROM categories
                      WHERE CategoryId = @CategoryId",
                    new { CategoryId = categoryId },
                    tx);

                tx.Commit();

                return rows > 0;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }
        public async Task<PagedCategoryDynamicResponse> GetAllPagedAsync(
     int pageNumber,
     int pageSize,
     string? search = null)
        {
            using var connection = _db.GetConnection();

            var offset = (pageNumber - 1) * pageSize;

            var whereClause = "WHERE c.IsActive = 1";

            if (!string.IsNullOrWhiteSpace(search))
            {
                whereClause += " AND CategoryName LIKE @Search";
            }

            var totalRecords = await connection.ExecuteScalarAsync<int>(
             $@"SELECT COUNT(*)
               FROM categories c
               {whereClause}",
             new
             {
                 Search = $"%{search}%"
             });

            var categories = (await connection.QueryAsync<CategoryDynamicGetResponse>(
            $@"SELECT
                    c.CategoryId,
                    c.CategoryName,
                    c.CreatedBy,
                    c.CreatedAt,
                    c.IsActive
               FROM categories c
               {whereClause}
               ORDER BY c.CategoryId DESC
               LIMIT @PageSize OFFSET @Offset",
            new
            {
                Search = $"%{search}%",
                PageSize = pageSize,
                Offset = offset
            })).ToList();

            foreach (var category in categories)
            {
                var fields = await connection.QueryAsync<CategoryDynamicFieldResponse>(
                    @"SELECT
                    cf.CategoryFieldId,
                    cf.FieldName,
                    cf.FieldCode,
                    cfv.FieldValue
              FROM categoryfieldvalues cfv
              INNER JOIN categoryfields cf
                    ON cfv.CategoryFieldId = cf.CategoryFieldId
              WHERE cfv.CategoryId = @CategoryId",
                    new { CategoryId = category.CategoryId });

                category.Fields = fields.ToList();
            }

            return new PagedCategoryDynamicResponse
            {
                TotalRecords = totalRecords,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Data = categories
            };
        }
        public async Task<bool> UpdateStatusAsync(
        int categoryId,
        bool isActive,
        string modifiedBy)
        {
            using var connection = _db.GetConnection();

            var rows = await connection.ExecuteAsync(
                @"UPDATE categories
          SET
              IsActive = @IsActive,
              ModifiedAt = NOW(),
              ModifiedBy = @ModifiedBy
          WHERE CategoryId = @CategoryId",
                new
                {
                    CategoryId = categoryId,
                    IsActive = isActive,
                    ModifiedBy = modifiedBy
                });

            return rows > 0;
        }
    }
}