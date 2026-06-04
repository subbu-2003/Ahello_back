using ahello_backend.DbContexts;
using ahello_backend.Models.Service;
using ahello_backend.Repositorys.Interfaces;
using Dapper;

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
    s.CreatedBy,
    s.CreatedAt
FROM services s
LEFT JOIN ServiceCategoryDynamic sc
    ON s.ServiceCategoryId = sc.ServiceCategoryId
ORDER BY s.ServiceId DESC")).ToList();

            foreach (var service in services)
            {
                var fields = await connection.QueryAsync<ServiceDynamicFieldResponse>(
                    @"SELECT
                        sf.ServiceFieldId,
                        sf.FieldName,
                        sf.FieldCode,
                        sfv.FieldValue
                      FROM servicefieldvalues sfv
                      INNER JOIN servicefields sf
                        ON sfv.ServiceFieldId = sf.ServiceFieldId
                      WHERE sfv.ServiceId = @ServiceId",
                    new { ServiceId = service.ServiceId });

                service.Fields = fields.ToList();
            }

            return services;
        }

        public async Task<ServiceDynamicGetResponse> GetByIdAsync(int serviceId)
        {
            using var connection = _db.GetConnection();

            var service = await connection.QueryFirstOrDefaultAsync<ServiceDynamicGetResponse>(
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
    s.CreatedBy,
    s.CreatedAt
FROM services s
LEFT JOIN ServiceCategoryDynamic sc
    ON s.ServiceCategoryId = sc.ServiceCategoryId
WHERE s.ServiceId = @ServiceId",
 new { ServiceId = serviceId });

            if (service == null)
                return null;

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
        new { ServiceId = serviceId })).ToList();

            foreach (var field in fields)
            {
                var dropdownOptions = await connection.QueryAsync<ServiceDropDownOptionResponse>(
                    @"SELECT
                ServiceDropDownId,
                OptionValue,
                OptionLabel,
                IsActive
              FROM servicedropdownoptions
              WHERE ServiceId = @ServiceId
                AND ServiceFieldId = @ServiceFieldId
                AND IsActive = 1",
                    new
                    {
                        ServiceId = serviceId,
                        ServiceFieldId = field.ServiceFieldId
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
            string? search = null)
        {
            using var connection = _db.GetConnection();

            var offset = (pageNumber - 1) * pageSize;

            var whereClause = @"
        WHERE UserId = @UserId";

            if (!string.IsNullOrWhiteSpace(search))
            {
                whereClause += @"
    AND
    (
        s.ServiceTitle LIKE @Search
        OR sc.ServiceCategoryName LIKE @Search
        OR s.ShortDescription LIKE @Search
        OR s.Tags LIKE @Search
        OR s.Language LIKE @Search
        OR s.Status LIKE @Search
    )";
            }

            // TOTAL COUNT
            var totalRecords = await connection.ExecuteScalarAsync<int>(
$@"SELECT COUNT(*)
FROM services s
LEFT JOIN ServiceCategoryDynamic sc
    ON s.ServiceCategoryId = sc.ServiceCategoryId
{whereClause}",
new
{
    UserId = userId,
    Search = $"%{search}%"
});

            // PAGINATION DATA
            var services = (await connection.QueryAsync<ServiceDynamicGetResponse>(
            $@"SELECT
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
    s.CreatedBy,
    s.CreatedAt
FROM services s
LEFT JOIN ServiceCategoryDynamic sc
    ON s.ServiceCategoryId = sc.ServiceCategoryId
{whereClause}
ORDER BY s.ServiceId DESC
LIMIT @PageSize OFFSET @Offset",
            new
            {
                UserId = userId,
                Search = $"%{search}%",
                PageSize = pageSize,
                Offset = offset
            })).ToList();

            // DYNAMIC FIELDS
            foreach (var service in services)
            {
                var fields = await connection.QueryAsync<ServiceDynamicFieldResponse>(
                    @"SELECT
                sf.ServiceFieldId,
                sf.FieldName,
                sf.FieldCode,
                sfv.FieldValue

              FROM servicefieldvalues sfv

              INNER JOIN servicefields sf
                ON sfv.ServiceFieldId = sf.ServiceFieldId

              WHERE sfv.ServiceId = @ServiceId",
                    new
                    {
                        ServiceId = service.ServiceId
                    });

                service.Fields = fields.ToList();
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

            connection.Open();

            using var tx = connection.BeginTransaction();

            try
            {
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
                        NOW(),
                        @CreatedBy
                    );

                    SELECT LAST_INSERT_ID();";

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
           ThumbnailImage = model.ThumbnailImageUrl,   // ← changed
           BannerImage = model.BannerImageUrl,          // ← changed
           model.IntroVideo,
           model.Status,
           model.CreatedBy
       },
       tx);

                if (model.Fields != null && model.Fields.Any())
                {
                    var fieldSql = @"
                        INSERT INTO servicefieldvalues
                        (
                            ServiceId,
                            FieldCode,
                            ServiceFieldId,
                            FieldValue,
                            CreatedDate,
                            CreatedBy,
                            CreatedAt
                        )
                        SELECT
                            @ServiceId,
                            sf.FieldCode,
                            @ServiceFieldId,
                            @FieldValue,
                            NOW(),
                            @CreatedBy,
                            CURRENT_TIMESTAMP
                        FROM servicefields sf
                        WHERE sf.ServiceFieldId = @ServiceFieldId;";

                    foreach (var field in model.Fields)
                    {
                        await connection.ExecuteAsync(
                            fieldSql,
                            new
                            {
                                ServiceId = serviceId,
                                ServiceFieldId = field.ServiceFieldId,
                                FieldValue = field.FieldValue,
                                model.CreatedBy
                            },
                            tx);
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
                        ServiceCategoryId=@ServiceCategoryId,
                        ServiceTitle = @ServiceTitle,
                        Price = @Price,
                        Duration = @Duration,
                        ShortDescription = @ShortDescription,
                        FullDescription = @FullDescription,
                        Tags = @Tags,
                        Language = @Language,
                       ThumbnailImage = COALESCE(@ThumbnailImage, ThumbnailImage),
                        BannerImage    = COALESCE(@BannerImage, BannerImage),
                        IntroVideo = @IntroVideo,
                        Status = @Status,
                        ModifiedAt = NOW(),
                        ModifiedBy = @ModifiedBy
                    WHERE ServiceId = @ServiceId";

                var rows = await connection.ExecuteAsync(
                    sql,
                    new
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
                        ThumbnailImage = model.ThumbnailImageUrl,   // ← changed
                        BannerImage = model.BannerImageUrl,
                        model.IntroVideo,
                        model.Status,
                        model.ModifiedBy
                    },
                    tx);

                if (rows <= 0)
                {
                    tx.Rollback();
                    return false;
                }

                await connection.ExecuteAsync(
                    @"DELETE FROM servicefieldvalues
                      WHERE ServiceId = @ServiceId",
                    new { ServiceId = serviceId },
                    tx);

                if (model.Fields != null && model.Fields.Any())
                {
                    var fieldSql = @"
                        INSERT INTO servicefieldvalues
                        (
                            ServiceId,
                            FieldCode,
                            ServiceFieldId,
                            FieldValue,
                            CreatedDate,
                            CreatedBy,
                            CreatedAt
                        )
                        SELECT
                            @ServiceId,
                            sf.FieldCode,
                            @ServiceFieldId,
                            @FieldValue,
                            NOW(),
                            @ModifiedBy,
                            CURRENT_TIMESTAMP
                        FROM servicefields sf
                        WHERE sf.ServiceFieldId = @ServiceFieldId;";

                    foreach (var field in model.Fields)
                    {
                        await connection.ExecuteAsync(
                            fieldSql,
                            new
                            {
                                ServiceId = serviceId,
                                ServiceFieldId = field.ServiceFieldId,
                                FieldValue = field.FieldValue,
                                model.ModifiedBy
                            },
                            tx);
                    }
                }
                /* ADD THIS CODE HERE ↓↓↓ */

                await connection.ExecuteAsync(
                    @"DELETE FROM servicedropdownoptions
      WHERE ServiceId = @ServiceId",
                    new { ServiceId = serviceId },
                    tx);

                if (model.Fields != null && model.Fields.Any())
                {
                    var dropdownSql = @"
        INSERT INTO servicedropdownoptions
        (
            ServiceFieldId,
            ServiceId,
            OptionValue,
            OptionLabel,
            IsActive,
            CreatedDate,
            CreatedBy
        )
        VALUES
        (
            @ServiceFieldId,
            @ServiceId,
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
                                        ServiceFieldId = field.ServiceFieldId,
                                        ServiceId = serviceId,
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

        public async Task<bool> DeleteAsync(int serviceId)
        {
            using var connection = _db.GetConnection();

            connection.Open();

            using var tx = connection.BeginTransaction();

            try
            {
                await connection.ExecuteAsync(
                    @"DELETE FROM servicefieldvalues
                      WHERE ServiceId = @ServiceId",
                    new { ServiceId = serviceId },
                    tx);

                var rows = await connection.ExecuteAsync(
                    @"DELETE FROM services
                      WHERE ServiceId = @ServiceId",
                    new { ServiceId = serviceId },
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

        public async Task<PagedServiceDynamicResponse> GetAllPagedAsync(
            int pageNumber,
            int pageSize,
            string? search = null)
        {
            using var connection = _db.GetConnection();

            var offset = (pageNumber - 1) * pageSize;

            var whereClause = "";

            if (!string.IsNullOrWhiteSpace(search))
            {
                whereClause = @"
                    WHERE ServiceTitle LIKE @Search";
            }

            var totalRecords = await connection.ExecuteScalarAsync<int>(
$@"SELECT COUNT(*)
FROM services s
LEFT JOIN ServiceCategoryDynamic sc
    ON s.ServiceCategoryId = sc.ServiceCategoryId
{whereClause}",
new
{
    Search = $"%{search}%"
});

            var services = (await connection.QueryAsync<ServiceDynamicGetResponse>(
$@"SELECT
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
    s.CreatedBy,
    s.CreatedAt
FROM services s
LEFT JOIN ServiceCategoryDynamic sc
    ON s.ServiceCategoryId = sc.ServiceCategoryId
{whereClause}
ORDER BY s.ServiceId DESC
LIMIT @PageSize OFFSET @Offset",
new
{
    Search = $"%{search}%",
    PageSize = pageSize,
    Offset = offset
})).ToList();

            foreach (var service in services)
            {
                var fields = await connection.QueryAsync<ServiceDynamicFieldResponse>(
                    @"SELECT
                        sf.ServiceFieldId,
                        sf.FieldName,
                        sf.FieldCode,
                        sfv.FieldValue
                      FROM servicefieldvalues sfv
                      INNER JOIN servicefields sf
                        ON sfv.ServiceFieldId = sf.ServiceFieldId
                      WHERE sfv.ServiceId = @ServiceId",
                    new { ServiceId = service.ServiceId });

                service.Fields = fields.ToList();
            }

            return new PagedServiceDynamicResponse
            {
                TotalRecords = totalRecords,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Data = services
            };
        }
    }
}