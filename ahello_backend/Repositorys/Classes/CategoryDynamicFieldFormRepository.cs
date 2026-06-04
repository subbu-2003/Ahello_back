using ahello_backend.DbContexts;
using ahello_backend.Models.Category;
using ahello_backend.Repositorys.Interfaces;
using Dapper;

namespace ahello_backend.Repositorys.Classes
{
    public class CategoryDynamicFieldFormRepository : ICategoryDynamicFieldFormRepository
    {
        private readonly DbContext _db;

        public CategoryDynamicFieldFormRepository(DbContext db)
        {
            _db = db;
        }

        public async Task<CategoryDynamicFieldFormResponse> GetCategoryDynamicFieldFormAsync(int categoryId)
        {
            using var connection = _db.GetConnection();

            // 🔹 Dynamic Fields
            var fields = await connection.QueryAsync<CategoryDynamicField>(
                @"SELECT
                    CategoryFieldId,
                    FieldName,
                    FieldCode,
                    Placeholder,
                    DataTypeId,
                    IsRequired
                  FROM categoryfields
                  WHERE CategoryId = @CategoryId
                    AND IsActive = 1",
                new { CategoryId = categoryId });

            var fieldList = fields.ToList();

            // 🔹 Static Fields
            var basicFields = new List<CategoryBasicField>
            {
                new()
                {
                    FieldCode = "CategoryId",
                    Label = "Category",
                    DataType = "dropdown",
                    IsRequired = true
                }
            };

            // 🔹 Category Dropdown
            var categories = await connection.QueryAsync<CategoryDropdownOption>(
                @"SELECT
                    CategoryName AS Label,
                    CAST(CategoryId AS CHAR) AS Value
                  FROM categories",
                new { CategoryId = categoryId });

            var categoryField = basicFields.First(f => f.FieldCode == "CategoryId");

            categoryField.Options = categories.ToList();

            // 🔹 Final Response
            return new CategoryDynamicFieldFormResponse
            {
                BasicFields = basicFields,
                DynamicFields = fieldList
            };
        }
    }
}
