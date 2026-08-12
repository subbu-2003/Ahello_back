using ahello_backend.DbContexts;
using ahello_backend.Models.Service;
using ahello_backend.Repositorys.Interfaces;
using Dapper;
using System.Data.Common;

namespace ahello_backend.Repositorys.Classes
{
    public class ServiceDynamicRepository : IServiceDynamicRepository
    {
        private readonly DbContext _db;

        public ServiceDynamicRepository(DbContext db)
        {
            _db = db;
        }


        public async Task<IEnumerable<ServiceDynamicGetResponse>> GetAllAsync()
        {
            using var connection = _db.GetConnection();

            var services = (await connection.QueryAsync<ServiceDynamicGetResponse>(
                @"SELECT
                s.ServiceId,
                s.UserId,
                s.ServiceTypeId,
                s.ServiceCategoryId,
                sc.ServiceCategoryName,
                s.ServiceTitle,
                s.Price,
                s.Duration,
                s.ShortDescription,
                s.FullDescription,
                s.Tags,
                s.Language,
                s.ThumbnailImage,
                s.BannerImage,
                s.IntroVideo,
                s.Status,
                s.IsActive,
                s.CreatedBy,
                s.CreatedAt
            FROM services s
            LEFT JOIN ServiceCategoryDynamic sc
                ON s.ServiceCategoryId = sc.ServiceCategoryId
            ORDER BY s.ServiceId DESC")).ToList();

            foreach (var service in services)
            {
                var fields = (await connection.QueryAsync<ServiceDynamicFieldResponse>(
                    @"SELECT
                sf.ServiceFieldId,
                sf.FieldName,
                sf.FieldCode,
                sfv.FieldValue
                FROM servicefieldvalues sfv
                INNER JOIN servicefields sf
                ON sfv.ServiceFieldId = sf.ServiceFieldId
                WHERE sfv.ServiceId = @ServiceId",
                    new { ServiceId = service.ServiceId })).ToList();

                foreach (var field in fields)
                {
                    var dropdownOptions =
     await connection.QueryAsync<ServiceDropDownOptionResponse>(
         @"
        SELECT
            ServiceDropDownId,
            ServiceFieldId,
            UserId,
            OptionValue,
            OptionLabel,
            IsActive
        FROM servicedropdownoptions
        WHERE ServiceFieldId = @ServiceFieldId
          AND UserId = @UserId
          AND IsActive = 1
        ORDER BY ServiceDropDownId ASC
        ",
         new
         {
             ServiceFieldId = field.ServiceFieldId,
             UserId = service.UserId
         });

                    field.DropDownOptions = dropdownOptions.ToList();
                }

                service.Fields = fields;
            }

            return services;
        }

        public async Task<ServiceDynamicGetResponse?> GetByIdAsync(int serviceId)
        {
            using var connection = _db.GetConnection();

            // ---------------------------------------------------------
            // 1. Get service
            // ---------------------------------------------------------
            var service = await connection.QueryFirstOrDefaultAsync<ServiceDynamicGetResponse>(
                @"
        SELECT
            s.ServiceId,
            s.UserId,
            s.ServiceTypeId,
            st.ServiceTypeName,
            s.ServiceCategoryId,
            sc.ServiceCategoryName,
            s.ServiceTitle,
            s.Price,
            s.Duration,
            s.ShortDescription,
            s.FullDescription,
            s.Tags,
            s.Language,
            s.ThumbnailImage,
            s.BannerImage,
            s.IntroVideo,
            s.Status,
            s.IsActive,
            s.CreatedBy,
            s.CreatedAt
        FROM services s
        LEFT JOIN servicecategorydynamic sc
            ON s.ServiceCategoryId = sc.ServiceCategoryId
        LEFT JOIN servicetypes st
            ON s.ServiceTypeId = st.ServiceTypeId
        WHERE s.ServiceId = @ServiceId
        ",
                new
                {
                    ServiceId = serviceId
                });

            if (service == null)
                return null;

            // ---------------------------------------------------------
            // 2. Get fields for THIS service
            // ---------------------------------------------------------
            var fields = (await connection.QueryAsync<ServiceDynamicFieldResponse>(
                @"
        SELECT
            sf.ServiceFieldId,
            sf.FieldName,
            sf.FieldCode,
            sf.Placeholder,
            sf.IsRequired,
            sf.IsActive,
            sf.DataTypeId,
            dt.DataTypeName,
            sfv.FieldValue
        FROM servicefieldvalues sfv
        INNER JOIN servicefields sf
            ON sf.ServiceFieldId = sfv.ServiceFieldId
        LEFT JOIN datatypes dt
            ON sf.DataTypeId = dt.DataTypeId
        WHERE sfv.ServiceId = @ServiceId
          AND sf.IsActive = 1
        ORDER BY sf.ServiceFieldId ASC
        ",
                new
                {
                    ServiceId = serviceId
                })).ToList();

            // ---------------------------------------------------------
            // 3. Get dropdown options for each field
            // ---------------------------------------------------------
            foreach (var field in fields)
            {
                var dropdownOptions =
                    await connection.QueryAsync<ServiceDropDownOptionResponse>(
                        @"
                SELECT
                    ServiceDropDownId,
                    ServiceFieldId,
                    UserId,
                    OptionValue,
                    OptionLabel,
                    IsActive
                FROM servicedropdownoptions
                WHERE ServiceFieldId = @ServiceFieldId
                  AND UserId = @UserId
                  AND IsActive = 1
                ORDER BY ServiceDropDownId ASC
                ",
                        new
                        {
                            ServiceFieldId = field.ServiceFieldId,
                            UserId = service.UserId
                        });

                field.DropDownOptions = dropdownOptions.ToList();
            }

            service.Fields = fields;

            return service;
        }
        // ServiceDynamicRepository.cs

        public async Task<PagedServiceDynamicResponse> GetByUserIdPagedAsync(
        int userId,
        int pageNumber,
        int pageSize,
        string? search = null,
        string? serviceCategoryName = null,
        string? serviceTypeName = null,
        string? status = null,
        bool? isActive = null)
        {
            using var connection = _db.GetConnection();

            var offset = (pageNumber - 1) * pageSize;

            var whereClause = @"
            WHERE s.UserId = @UserId";

            if (!string.IsNullOrWhiteSpace(search))
            {
                whereClause += @"
            AND
            (
                s.ServiceTitle LIKE @Search
                OR sc.ServiceCategoryName LIKE @Search
                OR st.ServiceTypeName LIKE @Search
                OR s.ShortDescription LIKE @Search
                OR s.Tags LIKE @Search
                OR s.Language LIKE @Search
                OR s.Status LIKE @Search
            )";
            }
            // ADD THIS BLOCK HERE
            if (!string.IsNullOrWhiteSpace(serviceCategoryName))
            {
                whereClause += @"
                 AND sc.ServiceCategoryName LIKE @ServiceCategoryName";
            }
            if (!string.IsNullOrWhiteSpace(serviceTypeName))
            {
                whereClause += @"
               AND st.ServiceTypeName LIKE @ServiceTypeName";
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                whereClause += @"
                    AND s.Status = @Status";
            }

            if (isActive.HasValue)
            {
                whereClause += @"
                     AND s.IsActive = @IsActive";
            }
            // TOTAL COUNT
            var totalRecords = await connection.ExecuteScalarAsync<int>(
            $@"
            SELECT COUNT(*)
            FROM services s
            LEFT JOIN ServiceCategoryDynamic sc
                ON s.ServiceCategoryId = sc.ServiceCategoryId
           LEFT JOIN servicetypes st
            ON s.ServiceTypeId = st.ServiceTypeId
            {whereClause}",
           new
           {
               UserId = userId,
               Search = $"%{search}%",
               ServiceCategoryName = $"%{serviceCategoryName}%",
               ServiceTypeName = $"%{serviceTypeName}%",
               Status = status,
               IsActive = isActive
           });

            // PAGINATION DATA
            var services = (await connection.QueryAsync<ServiceDynamicGetResponse>(
                $@"
                SELECT
                    s.ServiceId,
                    s.UserId,
                    s.ServiceTypeId,
                    st.ServiceTypeName,
                    s.ServiceCategoryId,
                    sc.ServiceCategoryName,
                    s.ServiceTitle,
                    s.Price,
                    s.Duration,
                    s.ShortDescription,
                    s.FullDescription,
                    s.Tags,
                    s.Language,
                    s.ThumbnailImage,
                    s.BannerImage,
                    s.IntroVideo,
                    s.Status,
                    s.IsActive,
                    s.CreatedBy,
                    s.CreatedAt
                FROM services s
                LEFT JOIN ServiceCategoryDynamic sc
                    ON s.ServiceCategoryId = sc.ServiceCategoryId
               LEFT JOIN servicetypes st
                    ON s.ServiceTypeId = st.ServiceTypeId
                {whereClause}
                ORDER BY s.ServiceId DESC
                LIMIT @PageSize OFFSET @Offset",
               new
               {
                   UserId = userId,
                   Search = $"%{search}%",
                   ServiceCategoryName = $"%{serviceCategoryName}%",
                   ServiceTypeName = $"%{serviceTypeName}%",
                   Status = status,
                   IsActive = isActive,
                   PageSize = pageSize,
                   Offset = offset
               })).ToList();

            // DYNAMIC FIELDS
            // DYNAMIC FIELDS
            // Get all user ids from current page
            // ---------------------------------------------------------
            // GET DYNAMIC FIELDS BASED ON SERVICE ID
            // ---------------------------------------------------------

            var serviceIds = services
                .Select(x => x.ServiceId)
                .Distinct()
                .ToList();

            if (serviceIds.Any())
            {
                // -----------------------------------------------------
                // Get all fields for current page services
                // -----------------------------------------------------
                var allFields = (
                    await connection.QueryAsync<ServiceDynamicFieldResponse>(
                        @"
            SELECT
                sfv.ServiceId,
                sf.ServiceFieldId,
                sf.FieldName,
                sf.FieldCode,
                sf.Placeholder,
                sf.IsRequired,
                sf.IsActive,
                sf.DataTypeId,
                dt.DataTypeName,
                sfv.FieldValue

            FROM servicefieldvalues sfv

            INNER JOIN servicefields sf
                ON sf.ServiceFieldId = sfv.ServiceFieldId

            LEFT JOIN datatypes dt
                ON sf.DataTypeId = dt.DataTypeId

            WHERE sfv.ServiceId IN @ServiceIds
              AND sf.IsActive = 1

            ORDER BY
                sfv.ServiceId ASC,
                sf.ServiceFieldId ASC
            ",
                        new
                        {
                            ServiceIds = serviceIds
                        }
                    )
                ).ToList();

                // -----------------------------------------------------
                // Get dropdown options
                //
                // IMPORTANT:
                // servicedropdownoptions does NOT have ServiceId.
                // So use ServiceFieldId + UserId.
                // -----------------------------------------------------
                var userIds = services
                    .Select(x => x.UserId)
                    .Distinct()
                    .ToList();

                var allDropdowns = (
                    await connection.QueryAsync<ServiceDropDownOptionResponse>(
                        @"
            SELECT
                ServiceDropDownId,
                ServiceFieldId,
                UserId,
                OptionValue,
                OptionLabel,
                IsActive

            FROM servicedropdownoptions

            WHERE UserId IN @UserIds
              AND IsActive = 1

            ORDER BY ServiceDropDownId ASC
            ",
                        new
                        {
                            UserIds = userIds
                        }
                    )
                ).ToList();

                // -----------------------------------------------------
                // Group fields by ServiceId
                // -----------------------------------------------------
                var fieldDictionary = allFields
                    .GroupBy(x => x.ServiceId)
                    .ToDictionary(
                        x => x.Key,
                        x => x.ToList()
                    );

                // -----------------------------------------------------
                // Map fields to each service
                // -----------------------------------------------------
                foreach (var service in services)
                {
                    var fields = fieldDictionary.TryGetValue(
                        service.ServiceId,
                        out var serviceFields)
                            ? serviceFields
                            : new List<ServiceDynamicFieldResponse>();

                    // -------------------------------------------------
                    // Map dropdown options
                    // -------------------------------------------------
                    foreach (var field in fields)
                    {
                        field.DropDownOptions = allDropdowns
                            .Where(x =>
                                x.ServiceFieldId == field.ServiceFieldId &&
                                x.UserId == service.UserId)
                            .ToList();
                    }

                    service.Fields = fields;
                }
            }
            else
            {
                // No services on current page
                foreach (var service in services)
                {
                    service.Fields =
                        new List<ServiceDynamicFieldResponse>();
                }
            }
            return new PagedServiceDynamicResponse
            {
                TotalRecords = totalRecords,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Data = services
            };
        }
        public async Task<int> CreateAsync(ServiceDynamicPost model)
        {
            using var connection = _db.GetConnection();

            if (connection is DbConnection dbConnection)
            {
                await dbConnection.OpenAsync();
            }
            else
            {
                connection.Open();
            }

            using var tx = connection.BeginTransaction();

            try
            {
                // -----------------------------------------------------
                // Duplicate service title
                // -----------------------------------------------------
                var duplicateExists = await connection.ExecuteScalarAsync<int>(
                    @"
            SELECT COUNT(*)
            FROM services
            WHERE LOWER(TRIM(ServiceTitle))
                = LOWER(TRIM(@ServiceTitle))
            ",
                    new
                    {
                        model.ServiceTitle
                    },
                    tx);

                if (duplicateExists > 0)
                {
                    throw new Exception("Service Title already exists.");
                }

                // -----------------------------------------------------
                // Insert service
                // -----------------------------------------------------
                var sql = @"
            INSERT INTO services
            (
                UserId,
                ServiceTypeId,
                ServiceCategoryId,
                ServiceTitle,
                Price,
                Duration,
                ShortDescription,
                FullDescription,
                Tags,
                Language,
                ThumbnailImage,
                BannerImage,
                IntroVideo,
                Status,
                IsActive,
                CreatedAt,
                CreatedBy
            )
            VALUES
            (
                @UserId,
                @ServiceTypeId,
                @ServiceCategoryId,
                @ServiceTitle,
                @Price,
                @Duration,
                @ShortDescription,
                @FullDescription,
                @Tags,
                @Language,
                @ThumbnailImage,
                @BannerImage,
                @IntroVideo,
                @Status,
                @IsActive,
                NOW(),
                @CreatedBy
            );

            SELECT LAST_INSERT_ID();
        ";

                var serviceId = await connection.ExecuteScalarAsync<int>(
                    sql,
                    new
                    {
                        model.UserId,
                        model.ServiceTypeId,
                        model.ServiceCategoryId,
                        model.ServiceTitle,
                        model.Price,
                        model.Duration,
                        model.ShortDescription,
                        model.FullDescription,
                        model.Tags,
                        model.Language,

                        ThumbnailImage = model.ThumbnailImageUrl,
                        BannerImage = model.BannerImageUrl,

                        model.IntroVideo,
                        model.Status,
                        model.IsActive,
                        model.CreatedBy
                    },
                    tx);

                // -----------------------------------------------------
                // Insert dynamic fields
                // -----------------------------------------------------
                // -----------------------------------------------------
                // Insert dynamic fields
                // -----------------------------------------------------
                if (model.Fields != null && model.Fields.Any())
                {
                    const string fieldSql = @"
                    INSERT INTO servicefieldvalues
                    (
                        UserId,
                        ServiceId,
                        FieldCode,
                        ServiceFieldId,
                        FieldValue,
                        CreatedDate,
                        CreatedBy,
                        CreatedAt
                    )
                    SELECT
                        @UserId,
                        @ServiceId,
                        sf.FieldCode,
                        sf.ServiceFieldId,
                        @FieldValue,
                        NOW(),
                        @CreatedBy,
                        CURRENT_TIMESTAMP
                    FROM servicefields sf
                    WHERE sf.ServiceFieldId = @ServiceFieldId
                      AND sf.UserId = @UserId
                      AND sf.IsActive = 1;
                ";

                    const string dropdownSql = @"
                            INSERT INTO servicedropdownoptions
                            (
                                ServiceId,
                                ServiceFieldId,
                                OptionValue,
                                OptionLabel,
                                IsActive,
                                CreatedDate,
                                CreatedBy,
                                UserId
                            )
                            VALUES
                            (
                                @ServiceId,
                                @ServiceFieldId,
                                @OptionValue,
                                @OptionLabel,
                                @IsActive,
                                NOW(),
                                @CreatedBy,
                                @UserId
                            );
                        ";

                    foreach (var field in model.Fields)
                    {
                        // Do not allow ServiceFieldId = 0
                        if (field.ServiceFieldId <= 0)
                        {
                            throw new Exception(
                                $"Invalid ServiceFieldId: {field.ServiceFieldId}."
                            );
                        }

                        // Check whether field belongs to this user
                        var fieldExists = await connection.ExecuteScalarAsync<int>(
                            @"
            SELECT COUNT(*)
            FROM servicefields
            WHERE ServiceFieldId = @ServiceFieldId
              AND UserId = @UserId
              AND IsActive = 1
            ",
                            new
                            {
                                ServiceFieldId = field.ServiceFieldId,
                                UserId = model.UserId
                            },
                            tx);

                        if (fieldExists == 0)
                        {
                            throw new Exception(
                                $"ServiceFieldId {field.ServiceFieldId} does not exist for UserId {model.UserId}."
                            );
                        }

                        // Insert field value
                        await connection.ExecuteAsync(
                            fieldSql,
                            new
                            {
                                UserId = model.UserId,
                                ServiceId = serviceId,
                                ServiceFieldId = field.ServiceFieldId,
                                FieldValue = field.FieldValue,
                                CreatedBy = model.CreatedBy
                            },
                            tx);

                        // Insert dropdown options
                        if (field.DropDownOptions != null &&
                            field.DropDownOptions.Any())
                        {
                            foreach (var option in field.DropDownOptions)
                            {
                                await connection.ExecuteAsync(
                                     dropdownSql,
                                     new
                                     {
                                         ServiceId = serviceId,
                                         ServiceFieldId = field.ServiceFieldId,
                                         OptionValue = option.OptionValue,
                                         OptionLabel = option.OptionLabel,
                                         IsActive = option.IsActive,
                                         CreatedBy = model.CreatedBy,
                                         UserId = model.UserId
                                     },
                                     tx);
                            }
                        }
                    }
                }

                tx.Commit();

                return serviceId;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }
        public async Task<bool> UpdateAsync(int serviceId, ServiceDynamicPut model)
        {
            using var connection = _db.GetConnection();
            connection.Open();

            using var tx = connection.BeginTransaction();

            try
            {
                var sql = @"
                UPDATE services
                SET
                    UserId = @UserId,
                    ServiceTypeId = @ServiceTypeId,
                    ServiceCategoryId = @ServiceCategoryId,
                    ServiceTitle = @ServiceTitle,
                    Price = @Price,
                    Duration = @Duration,
                    ShortDescription = @ShortDescription,
                    FullDescription = @FullDescription,
                    Tags = @Tags,
                    Language = @Language,
                    ThumbnailImage = @ThumbnailImage,
                    BannerImage = @BannerImage,
                    IntroVideo = @IntroVideo,
                    Status = @Status,
                    IsActive = @IsActive,
                    ModifiedAt = NOW(),
                    ModifiedBy = @ModifiedBy
                WHERE ServiceId = @ServiceId";
                var exists = await connection.ExecuteScalarAsync<bool>(
                    "SELECT EXISTS(SELECT 1 FROM services WHERE ServiceId = @ServiceId)",
                    new { ServiceId = serviceId }, tx);

                if (!exists)
                {
                    tx.Rollback();
                    return false;
                }
                var duplicateExists = await connection.ExecuteScalarAsync<int>(
                @"SELECT COUNT(*)
                  FROM services
                  WHERE LOWER(TRIM(ServiceTitle)) = LOWER(TRIM(@ServiceTitle))
                  AND ServiceId <> @ServiceId",
                new
                {
                    model.ServiceTitle,
                    ServiceId = serviceId
                },
                tx);

                if (duplicateExists > 0)
                {
                    throw new Exception("Service Title already exists.");
                }
                await connection.ExecuteAsync(sql, new
                {
                    ServiceId = serviceId,
                    model.UserId,
                    model.ServiceTypeId,
                    model.ServiceCategoryId,
                    model.ServiceTitle,
                    model.Price,
                    model.Duration,
                    model.ShortDescription,
                    model.FullDescription,
                    model.Tags,
                    model.Language,
                    ThumbnailImage = model.ThumbnailImageUrl,
                    BannerImage = model.BannerImageUrl,
                    model.IntroVideo,
                    model.Status,
                    model.IsActive,
                    model.ModifiedBy
                }, tx);

                if (model.Fields != null && model.Fields.Any())
                {
                    // Only clear field values / dropdown options for fields present in
                    // THIS request — not the whole service. Prevents wiping untouched
                    // fields when the frontend sends a partial Fields list.
                    var fieldIdsInRequest = model.Fields.Select(f => f.ServiceFieldId).ToList();

                    // Only fields whose DropDownOptions were actually sent — null means
                    // "frontend didn't touch this field's options, leave them alone"
                    var dropdownFieldIdsInRequest = model.Fields
                        .Where(f => f.DropDownOptions != null)
                        .Select(f => f.ServiceFieldId)
                        .ToList();

                    await connection.ExecuteAsync(
                        @"DELETE FROM servicefieldvalues
                            WHERE UserId = @UserId
                              AND ServiceFieldId IN @FieldIds",
                        new { UserId = model.UserId, FieldIds = fieldIdsInRequest },
                        tx);

                    if (dropdownFieldIdsInRequest.Any())
                    {
                        await connection.ExecuteAsync(
                            @"DELETE FROM servicedropdownoptions
                                    WHERE UserId = @UserId
                                      AND ServiceFieldId IN @FieldIds",
                            new { UserId = model.UserId, FieldIds = dropdownFieldIdsInRequest },
                            tx);
                    }
                    var fieldSql = @"
                    INSERT INTO servicefieldvalues
                    (
                        ServiceId,
                        UserId,
                        FieldCode,
                        ServiceFieldId,
                        FieldValue,
                        CreatedDate,
                        CreatedBy,
                        CreatedAt
                    )
                    SELECT
                        @ServiceId,
                        @UserId,
                        sf.FieldCode,
                        sf.ServiceFieldId,
                        @FieldValue,
                        NOW(),
                        @ModifiedBy,
                        CURRENT_TIMESTAMP
                    FROM servicefields sf
                    WHERE sf.ServiceFieldId = @ServiceFieldId";

                    var insertDropdownSql = @"
                    INSERT INTO servicedropdownoptions
                    (
                        ServiceId,
                        ServiceFieldId,
                        UserId,
                        OptionValue,
                        OptionLabel,
                        IsActive,
                        CreatedDate,
                        CreatedBy
                    )
                    VALUES
                    (
                         @ServiceId,
                        @ServiceFieldId,
                        @UserId,
                        @OptionValue,
                        @OptionLabel,
                        @IsActive,
                        NOW(),
                        @ModifiedBy
                    )";

                    foreach (var field in model.Fields)
                    {
                        var insertedFieldRows = await connection.ExecuteAsync(fieldSql, new
                        {
                            ServiceId = serviceId,
                            UserId = model.UserId,
                            ServiceFieldId = field.ServiceFieldId,
                            FieldValue = field.FieldValue,
                            model.ModifiedBy
                        }, tx);

                        if (insertedFieldRows <= 0)
                        {
                            continue; // skip invalid field
                        }

                        if (field.DropDownOptions != null && field.DropDownOptions.Any())
                        {
                            foreach (var option in field.DropDownOptions)
                            {
                                await connection.ExecuteAsync(insertDropdownSql, new
                                {
                                    ServiceId = serviceId,
                                    ServiceFieldId = field.ServiceFieldId,
                                    UserId = model.UserId,
                                    option.OptionValue,
                                    option.OptionLabel,
                                    option.IsActive,
                                    model.ModifiedBy
                                }, tx);
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

        public async Task<bool> DeleteAsync(int serviceId)
        {
            using var connection = _db.GetConnection();

            connection.Open();

            using var tx = connection.BeginTransaction();

            try
            {
                // Delete dropdown options first
                await connection.ExecuteAsync(
                    @"
            DELETE FROM servicedropdownoptions
            WHERE ServiceId = @ServiceId
            ",
                    new
                    {
                        ServiceId = serviceId
                    },
                    tx);

                // Delete field values
                await connection.ExecuteAsync(
                    @"
            DELETE FROM servicefieldvalues
            WHERE ServiceId = @ServiceId
            ",
                    new
                    {
                        ServiceId = serviceId
                    },
                    tx);

                // Delete service
                var rows = await connection.ExecuteAsync(
                    @"
            DELETE FROM services
            WHERE ServiceId = @ServiceId
            ",
                    new
                    {
                        ServiceId = serviceId
                    },
                    tx);

                tx.Commit();

                return rows > 0;
            }
            catch
            {
                tx.Commit();
                throw;
            }
        }

        public async Task<PagedServiceDynamicResponse> GetAllPagedAsync(
    int pageNumber,
    int pageSize,
    string? search = null)
        {
            using var connection = _db.GetConnection();

            if (pageNumber <= 0)
                pageNumber = 1;

            if (pageSize <= 0)
                pageSize = 10;

            var offset = (pageNumber - 1) * pageSize;

            // ---------------------------------------------------------
            // WHERE
            // ---------------------------------------------------------
            var whereClause = "";

            if (!string.IsNullOrWhiteSpace(search))
            {
                whereClause = @"
            WHERE
                s.ServiceTitle LIKE @Search
                OR sc.ServiceCategoryName LIKE @Search
                OR st.ServiceTypeName LIKE @Search
                OR s.ShortDescription LIKE @Search
                OR s.Tags LIKE @Search
                OR s.Language LIKE @Search
                OR s.Status LIKE @Search";
            }

            var parameters = new
            {
                Search = $"%{search}%"
            };

            // ---------------------------------------------------------
            // TOTAL COUNT
            // ---------------------------------------------------------
            var totalRecords = await connection.ExecuteScalarAsync<int>(
                $@"
        SELECT COUNT(*)
        FROM services s
        LEFT JOIN servicecategorydynamic sc
            ON s.ServiceCategoryId = sc.ServiceCategoryId
        LEFT JOIN servicetypes st
            ON s.ServiceTypeId = st.ServiceTypeId
        {whereClause}
        ",
                parameters);

            // ---------------------------------------------------------
            // GET SERVICES
            // ---------------------------------------------------------
            var services = (await connection.QueryAsync<ServiceDynamicGetResponse>(
                $@"
        SELECT
            s.ServiceId,
            s.UserId,

            s.ServiceTypeId,
            st.ServiceTypeName,

            s.ServiceCategoryId,
            sc.ServiceCategoryName,

            s.ServiceTitle,
            s.Price,
            s.Duration,
            s.ShortDescription,
            s.FullDescription,
            s.Tags,
            s.Language,

            s.ThumbnailImage,
            s.BannerImage,
            s.IntroVideo,

            s.Status,
            s.IsActive,
            s.CreatedBy,
            s.CreatedAt

        FROM services s

        LEFT JOIN servicecategorydynamic sc
            ON s.ServiceCategoryId = sc.ServiceCategoryId

        LEFT JOIN servicetypes st
            ON s.ServiceTypeId = st.ServiceTypeId

        {whereClause}

        ORDER BY s.ServiceId DESC

        LIMIT @PageSize OFFSET @Offset
        ",
                new
                {
                    Search = $"%{search}%",
                    PageSize = pageSize,
                    Offset = offset
                })).ToList();

            // ---------------------------------------------------------
            // GET DYNAMIC FIELDS
            // ---------------------------------------------------------
            foreach (var service in services)
            {
                var fields = (await connection.QueryAsync<ServiceDynamicFieldResponse>(
                    @"
            SELECT
                sf.ServiceFieldId,
                sf.FieldName,
                sf.FieldCode,
                sf.Placeholder,
                sf.IsRequired,
                sf.IsActive,
                sf.DataTypeId,
                dt.DataTypeName,
                sfv.FieldValue

            FROM servicefieldvalues sfv

            INNER JOIN servicefields sf
                ON sf.ServiceFieldId = sfv.ServiceFieldId

            LEFT JOIN datatypes dt
                ON sf.DataTypeId = dt.DataTypeId

            WHERE sfv.ServiceId = @ServiceId
              AND sf.IsActive = 1

            ORDER BY sf.ServiceFieldId ASC
            ",
                    new
                    {
                        ServiceId = service.ServiceId
                    })).ToList();

                // -----------------------------------------------------
                // DROPDOWN OPTIONS
                // -----------------------------------------------------
                foreach (var field in fields)
                {
                    var dropdownOptions =
                        await connection.QueryAsync<ServiceDropDownOptionResponse>(
                            @"
                    SELECT
                        ServiceDropDownId,
                        ServiceFieldId,
                        UserId,
                        OptionValue,
                        OptionLabel,
                        IsActive

                    FROM servicedropdownoptions

                    WHERE ServiceFieldId = @ServiceFieldId
                      AND UserId = @UserId
                      AND IsActive = 1

                    ORDER BY ServiceDropDownId ASC
                    ",
                            new
                            {
                                ServiceFieldId = field.ServiceFieldId,
                                UserId = service.UserId
                            });

                    field.DropDownOptions = dropdownOptions.ToList();
                }

                service.Fields = fields;
            }

            return new PagedServiceDynamicResponse
            {
                TotalRecords = totalRecords,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Data = services
            };
        }
        public async Task<bool> UpdateServiceIsActiveAsync(int serviceId, ServiceIsActivePut model)
        {
            using var connection = _db.GetConnection();

            var exists = await connection.ExecuteScalarAsync<bool>(
                "SELECT EXISTS(SELECT 1 FROM services WHERE ServiceId = @ServiceId)",
                new { ServiceId = serviceId });

            if (!exists)
            {
                return false;
            }

              var sql = @"
            UPDATE services
            SET
                IsActive = @IsActive,
                ModifiedAt = NOW(),
                ModifiedBy = @ModifiedBy
            WHERE ServiceId = @ServiceId";

            await connection.ExecuteAsync(
                sql,
                new
                {
                    ServiceId = serviceId,
                    model.IsActive,
                    model.ModifiedBy
                });

            return true;
        }
    }
}