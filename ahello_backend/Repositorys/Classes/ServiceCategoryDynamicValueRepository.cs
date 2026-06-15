using ahello_backend.DbContexts;
using ahello_backend.Models.Pagination;
using ahello_backend.Models.Service;
using ahello_backend.Models.Servicecategory;
using ahello_backend.Repositorys.Interfaces;
using Dapper;


namespace ahello_backend.Repositorys.Classes
{
    public class ServiceCategoryDynamicValueRepository
        : IServiceCategoryDynamicValueRepository
    {
        private readonly DbContext _db;

        public ServiceCategoryDynamicValueRepository(DbContext db)
        {
            _db = db;
        }

        public async Task<int> CreateAsync(ServiceCategoryDynamicPost model)
        {
            using var connection = _db.GetConnection();

            connection.Open();

            using var tx = connection.BeginTransaction();

            try
            {
                // Create Category First
                var serviceCategoryId =
                    await connection.ExecuteScalarAsync<int>(
                    @"
            INSERT INTO servicecategorydynamic
            (
                ServiceCategoryName,
                IsActive,
                CreatedBy,
                CreatedAt
            )
            VALUES
            (
                @ServiceCategoryName,
                @IsActive,
                @CreatedBy,
                NOW()
            );

            SELECT LAST_INSERT_ID();
            ",
                    new
                    {
                        model.ServiceCategoryName,
                        model.IsActive,
                        model.CreatedBy
                    },
                    tx);

                // Insert Field Values
                if (model.Fields != null && model.Fields.Any())
                {
                    var sql = @"
            INSERT INTO servicecategoryfieldvalues
            (
                ServiceCategoryId,
                FieldCode,
                ServiceCategoryFieldId,
                FieldValue,
                CreatedDate,
                CreatedBy,
                CreatedAt
            )
            SELECT
                @ServiceCategoryId,
                scf.FieldCode,
                @ServiceCategoryFieldId,
                @FieldValue,
                NOW(),
                @CreatedBy,
                NOW()
            FROM servicecategoryfields scf
            WHERE scf.ServiceCategoryFieldId =
                  @ServiceCategoryFieldId";

                    foreach (var field in model.Fields)
                    {
                        await connection.ExecuteAsync(
                            sql,
                            new
                            {
                                ServiceCategoryId = serviceCategoryId,
                                field.ServiceCategoryFieldId,
                                field.FieldValue,
                                model.CreatedBy
                            },
                            tx);
                    }
                }

                // Insert Dropdown Options
                if (model.Fields != null && model.Fields.Any())
                {
                    var dropdownSql = @"
            INSERT INTO servicecategorydropdownoption
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
                NOW(),
                @CreatedBy
            )";

                    foreach (var field in model.Fields)
                    {
                        if (field.DropDownOptions != null &&
                            field.DropDownOptions.Any())
                        {
                            foreach (var option in field.DropDownOptions)
                            {
                                await connection.ExecuteAsync(
                                    dropdownSql,
                                    new
                                    {
                                        field.ServiceCategoryFieldId,
                                        ServiceCategoryId = serviceCategoryId,
                                        option.OptionValue,
                                        option.OptionLabel,
                                        option.IsActive,
                                        model.CreatedBy
                                    },
                                    tx);
                            }
                        }
                    }
                }

                tx.Commit();

                return serviceCategoryId;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public async Task<IEnumerable<ServiceCategoryDynamicGetResponse>>
            GetAllAsync()
        {
            using var connection = _db.GetConnection();

            var categories =
                (await connection.QueryAsync<
                    ServiceCategoryDynamicGetResponse>(
                @"SELECT
                ServiceCategoryId,
                ServiceCategoryName,
                IsActive
            FROM servicecategorydynamic
            WHERE IsActive = 1"))
                .ToList();

            foreach (var category in categories)
            {
                var fields =
                    await connection.QueryAsync<
                        ServiceCategoryDynamicFieldResponse>(
                    @"SELECT
                        scf.ServiceCategoryFieldId,
                        scf.FieldName,
                        scf.FieldCode,
                        scf.Placeholder,
                        scf.IsRequired,
                        scf.DataTypeId,
                        scfv.FieldValue

                    FROM servicecategoryfieldvalues scfv

                    INNER JOIN servicecategoryfields scf
                        ON scfv.ServiceCategoryFieldId =
                           scf.ServiceCategoryFieldId

                    WHERE scfv.ServiceCategoryId =
                          @ServiceCategoryId",
                    new
                    {
                        category.ServiceCategoryId
                    });

                category.Fields = fields.ToList();
                var dropdownOptions = await connection.QueryAsync< ServiceCategoryDropdownOptionResponse>(
                @"SELECT DISTINCT
                    scdo.ServiceCategoryDropDownId,
                    scdo.ServiceCategoryFieldId,
                    scdo.ServiceCategoryId,
                    scdo.OptionValue,
                    scdo.OptionLabel,
                    scdo.IsActive

                FROM servicecategorydropdownoption scdo

                INNER JOIN servicecategoryfieldvalues scfv
                    ON scdo.ServiceCategoryFieldId =
                       scfv.ServiceCategoryFieldId

                    AND scdo.ServiceCategoryId =
                        scfv.ServiceCategoryId

                WHERE scdo.ServiceCategoryId =
                      @ServiceCategoryId",
                new
                {
                    category.ServiceCategoryId
                });

                category.DropdownOptions = dropdownOptions.ToList();
            }

            return categories;
        }
        public async Task<PagedResult<ServiceCategoryDynamicGetResponse>> GetAllWithStatusAsync(
    int pageNumber,
    int pageSize,
    string? search,
    DateTime? createdDate)
        {
            using var connection = _db.GetConnection();

            int offset = (pageNumber - 1) * pageSize;

            var whereConditions = new List<string>();
            var parameters = new DynamicParameters();

            if (!string.IsNullOrWhiteSpace(search))
            {
                whereConditions.Add("ServiceCategoryName LIKE @Search");
                parameters.Add("Search", $"%{search}%");
            }

            if (createdDate.HasValue)
            {
                whereConditions.Add("DATE(CreatedAt) = @CreatedDate");
                parameters.Add("CreatedDate", createdDate.Value.Date);
            }

            string whereClause = whereConditions.Any()
                ? $"WHERE {string.Join(" AND ", whereConditions)}"
                : "";

            string countQuery = $@"
        SELECT COUNT(*)
        FROM ServiceCategoryDynamic
        {whereClause};
    ";

            string dataQuery = $@"
        SELECT *
        FROM ServiceCategoryDynamic
        {whereClause}
        ORDER BY ServiceCategoryId DESC
        LIMIT @PageSize OFFSET @Offset;
    ";

            parameters.Add("PageSize", pageSize);
            parameters.Add("Offset", offset);

            var totalRecords = await connection.ExecuteScalarAsync<int>(
                countQuery,
                parameters);

            var data = await connection.QueryAsync<ServiceCategoryDynamicGetResponse>(
                dataQuery,
                parameters);

            return new PagedResult<ServiceCategoryDynamicGetResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalRecords,
                Details = data.ToList()
            };
        }
        public async Task<ServiceCategoryDynamicGetResponse>
            GetByIdAsync(int serviceCategoryId)
        {
            using var connection = _db.GetConnection();

            var category =
                await connection.QueryFirstOrDefaultAsync<
                    ServiceCategoryDynamicGetResponse>(
                @"SELECT
                ServiceCategoryId,
                ServiceCategoryName,
                IsActive
                FROM servicecategorydynamic
                WHERE ServiceCategoryId = @ServiceCategoryId
                AND IsActive = 1",
                new
                {
                    ServiceCategoryId = serviceCategoryId
                });

            if (category == null)
                return null;

            var fields =
                await connection.QueryAsync<
                    ServiceCategoryDynamicFieldResponse>(
                @"SELECT
                    scf.ServiceCategoryFieldId,
                    scf.FieldName,
                    scf.FieldCode,
                    scf.Placeholder,
                    scf.IsRequired,
                    scf.DataTypeId,
                    scfv.FieldValue

                FROM servicecategoryfieldvalues scfv

                INNER JOIN servicecategoryfields scf
                    ON scfv.ServiceCategoryFieldId =
                       scf.ServiceCategoryFieldId

                WHERE scfv.ServiceCategoryId =
                      @ServiceCategoryId",
                new
                {
                    ServiceCategoryId = serviceCategoryId
                });

            category.Fields = fields.ToList();
            var dropdownOptions =await connection.QueryAsync<  ServiceCategoryDropdownOptionResponse>(
            @"SELECT DISTINCT
                scdo.ServiceCategoryDropDownId,
                scdo.ServiceCategoryFieldId,
                scdo.ServiceCategoryId,
                scdo.OptionValue,
                scdo.OptionLabel,
                scdo.IsActive

            FROM servicecategorydropdownoption scdo

            INNER JOIN servicecategoryfieldvalues scfv
                ON scdo.ServiceCategoryFieldId =
                   scfv.ServiceCategoryFieldId

                AND scdo.ServiceCategoryId =
                    scfv.ServiceCategoryId

            WHERE scdo.ServiceCategoryId =
                  @ServiceCategoryId",
            new
            {
                ServiceCategoryId = category.ServiceCategoryId
            });

            category.DropdownOptions =
                dropdownOptions.ToList();

            return category;
        }

        public async Task<bool> UpdateAsync(
            int serviceCategoryId,
            ServiceCategoryDynamicPut model)
        {
            using var connection = _db.GetConnection();

            connection.Open();

            using var tx = connection.BeginTransaction();
            await connection.ExecuteAsync(
            @"UPDATE servicecategorydynamic
              SET ServiceCategoryName = @ServiceCategoryName,
                 IsActive = @IsActive,
                  ModifiedBy = @ModifiedBy,
                  ModifiedAt = NOW()
              WHERE ServiceCategoryId = @ServiceCategoryId",
            new
            {
                ServiceCategoryId = serviceCategoryId,
                model.ServiceCategoryName,
                model.IsActive,
                model.ModifiedBy
            },
            tx);

            try
            {
                await connection.ExecuteAsync(
                    @"DELETE FROM servicecategoryfieldvalues
                      WHERE ServiceCategoryId =
                            @ServiceCategoryId",
                    new
                    {
                        ServiceCategoryId = serviceCategoryId
                    },
                    tx);
                 await connection.ExecuteAsync(
                @"DELETE FROM servicecategorydropdownoption
                  WHERE ServiceCategoryId =
                        @ServiceCategoryId",
                new
                {
                    ServiceCategoryId = serviceCategoryId
                },
                tx);
                if (model.Fields != null && model.Fields.Any())
                {
                    var sql = @"
                    INSERT INTO servicecategoryfieldvalues
                    (
                        ServiceCategoryId,
                        FieldCode,
                        ServiceCategoryFieldId,
                        FieldValue,
                        CreatedDate,
                        CreatedBy,
                        CreatedAt
                    )
                    SELECT
                        @ServiceCategoryId,
                        scf.FieldCode,
                        @ServiceCategoryFieldId,
                        @FieldValue,
                        NOW(),
                        @ModifiedBy,
                        CURRENT_TIMESTAMP
                    FROM servicecategoryfields scf
                    WHERE scf.ServiceCategoryFieldId =
                          @ServiceCategoryFieldId";

                    foreach (var field in model.Fields)
                    {
                        await connection.ExecuteAsync(
                            sql,
                            new
                            {
                                ServiceCategoryId =
                                    serviceCategoryId,

                                field.ServiceCategoryFieldId,

                                field.FieldValue,

                                model.ModifiedBy
                            },
                            tx);
                    }
                }
                if (model.Fields != null && model.Fields.Any())
                {
                    var dropdownSql = @"
                    INSERT INTO servicecategorydropdownoption
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
                        NOW(),
                        @ModifiedBy
                    );";

                    foreach (var field in model.Fields)
                    {
                        if (field.DropDownOptions != null &&
                            field.DropDownOptions.Any())
                        {
                            foreach (var option in field.DropDownOptions)
                            {
                                await connection.ExecuteAsync(
                                    dropdownSql,
                                    new
                                    {
                                        field.ServiceCategoryFieldId,
                                        ServiceCategoryId = serviceCategoryId,
                                        option.OptionValue,
                                        option.OptionLabel,
                                        option.IsActive,
                                        model.ModifiedBy
                                    },
                                    tx);
                            }
                        }
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

        public async Task<bool> DeleteAsync( int serviceCategoryId)
        {
            using var connection = _db.GetConnection();

            connection.Open();

            using var tx = connection.BeginTransaction();

            try
            {
                await connection.ExecuteAsync(
                    @"DELETE FROM servicecategoryfieldvalues
              WHERE ServiceCategoryId =
                    @ServiceCategoryId",
                    new
                    {
                        ServiceCategoryId = serviceCategoryId
                    },
                    tx);
                await connection.ExecuteAsync(
                    @"DELETE FROM servicecategorydropdownoption
              WHERE ServiceCategoryId =
                    @ServiceCategoryId",
                    new
                    {
                        ServiceCategoryId = serviceCategoryId
                    },
                    tx);

                tx.Commit();

                return true;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }
        public async Task<bool> UpdateStatusAsync(
        int serviceCategoryId,
        ServiceCategoryStatusUpdate model)
        {
            using var connection = _db.GetConnection();

            var rowsAffected = await connection.ExecuteAsync(
                @"UPDATE servicecategorydynamic
          SET IsActive = @IsActive,
              ModifiedBy = @ModifiedBy,
              ModifiedAt = NOW()
          WHERE ServiceCategoryId = @ServiceCategoryId",
                new
                {
                    ServiceCategoryId = serviceCategoryId,
                    model.IsActive,
                    model.ModifiedBy
                });

            return rowsAffected > 0;
        }
    }
}
