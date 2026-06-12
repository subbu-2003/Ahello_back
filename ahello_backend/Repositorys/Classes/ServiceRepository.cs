using ahello_backend.DbContexts;
using ahello_backend.Models.Service;
using ahello_backend.Repositorys.Interfaces;
using Dapper;

namespace ahello_backend.Repositorys.Classes
{
    public class ServiceRepository : IServiceRepository
    {
        private readonly DbContext _db;

        public ServiceRepository(DbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Service>> GetAllAsync()
        {
            using var connection = _db.GetConnection();

            var sql = @"
                SELECT
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
                    s.CreatedAt,
                    s.CreatedBy,
                    s.ModifiedAt,
                    s.ModifiedBy
                FROM services s
             Inner join servicecategorydynamic sc on sc.ServiceCategoryId = s.ServiceCategoryId
                ORDER BY s.ServiceId DESC";

            return await connection.QueryAsync<Service>(sql);
        }

        public async Task<Service> GetByIdAsync(int serviceId)
        {
            using var connection = _db.GetConnection();

            var sql = @"
                SELECT
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
                s.CreatedAt,
                s.CreatedBy,
                s.ModifiedAt,
                s.ModifiedBy
            FROM services s
            INNER JOIN servicecategorydynamic sc
                ON sc.ServiceCategoryId = s.ServiceCategoryId
            WHERE s.ServiceId = @ServiceId";

            return await connection.QueryFirstOrDefaultAsync<Service>(
                sql,
                new
                {
                    ServiceId = serviceId
                });
        }

        public async Task<int> CreateAsync(ServicePost model)
        {
            using var connection = _db.GetConnection();

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
                    IFNULL(@IsActive,1),
                    NOW(),
                    @CreatedBy
                );

                SELECT LAST_INSERT_ID();";

            return await connection.ExecuteScalarAsync<int>(
                sql,
                model);
        }

        public async Task<bool> UpdateAsync(
            int serviceId,
            ServicePut model)
        {
            using var connection = _db.GetConnection();

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
                    model.ThumbnailImage,
                    model.BannerImage,
                    model.IntroVideo,
                    model.Status,
                    model.IsActive,
                    model.ModifiedBy
                });

            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int serviceId)
        {
            using var connection = _db.GetConnection();

            var sql = @"
                DELETE FROM services
                WHERE ServiceId = @ServiceId";

            var rows = await connection.ExecuteAsync(
                sql,
                new
                {
                    ServiceId = serviceId
                });

            return rows > 0;
        }
        public async Task<IEnumerable<Service>> GetByServiceCategoryIdAsync(
    int serviceCategoryId)
        {
            using var connection = _db.GetConnection();

            var sql = @"
        SELECT
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
            s.CreatedAt,
            s.CreatedBy,
            s.ModifiedAt,
            s.ModifiedBy
        FROM services s
        INNER JOIN servicecategorydynamic sc
            ON sc.ServiceCategoryId = s.ServiceCategoryId
        WHERE s.ServiceCategoryId = @ServiceCategoryId
        ORDER BY s.ServiceId DESC";

            return await connection.QueryAsync<Service>(
                sql,
                new
                {
                    ServiceCategoryId = serviceCategoryId
                });
        }
        public async Task<Service> GetById(int serviceId)
        {
            using var connection = _db.GetConnection();

            var sql = @"
        SELECT
            s.ServiceId,
            s.UserId,
            s.ServiceTitle,
            s.ShortDescription,
            s.Duration,
            s.Price,
            s.IsActive,
            u.FullName,
            u.ProfileUrl,

            sc.ServiceCategoryName AS CategoryName

        FROM services s

        INNER JOIN users u
            ON s.UserId = u.UserId

        LEFT JOIN servicecategorydynamic sc
            ON s.ServiceCategoryId = sc.ServiceCategoryId

        WHERE s.ServiceId = @ServiceId";

            return await connection.QueryFirstOrDefaultAsync<Service>(
                sql,
                new { ServiceId = serviceId });
        }
    }
}
