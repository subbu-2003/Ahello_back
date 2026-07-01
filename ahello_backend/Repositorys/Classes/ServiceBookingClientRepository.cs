using ahello_backend.DbContexts;
using ahello_backend.Models.Bookings;
using ahello_backend.Models.Pagination;
using ahello_backend.Models.ServiceBookingClientResponse;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Classes;
using Dapper;
namespace ahello_backend.Repositorys.Classes
{
    public class ServiceBookingClientRepository : IServiceBookingClientRepository
    {
        private readonly DbContext _db;

        public ServiceBookingClientRepository(DbContext db)
        {
            _db = db;
        }

        public async Task<PagedResult<ServiceBookingClientResponse>> GetClientsByUserIdAsync(
        int userId,
        int pageNumber,
        int pageSize,
        string? search)
        {
            using var connection = _db.GetConnection();

            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 10 : pageSize;

            var countSql = @"
        SELECT COUNT(DISTINCT c.UserId)

        FROM bookings b

        INNER JOIN users c
            ON b.ClientId = c.UserId

        WHERE b.UserId = @UserId

        AND
        (
            @Search IS NULL
            OR @Search = ''

            OR c.FullName LIKE CONCAT('%', @Search, '%')
            OR c.Email LIKE CONCAT('%', @Search, '%')
            OR c.MobileNumber LIKE CONCAT('%', @Search, '%')
        );";

            var totalCount = await connection.ExecuteScalarAsync<int>(
                countSql,
                new
                {
                    UserId = userId,
                    Search = search
                });

            var sql = @"
        SELECT DISTINCT
            c.UserId AS ClientId,
            c.FullName AS ClientName,
            c.Email,
            c.MobileNumber

        FROM bookings b

        INNER JOIN users c
            ON b.ClientId = c.UserId

        WHERE b.UserId = @UserId

        AND
        (
            @Search IS NULL
            OR @Search = ''

            OR c.FullName LIKE CONCAT('%', @Search, '%')
            OR c.Email LIKE CONCAT('%', @Search, '%')
            OR c.MobileNumber LIKE CONCAT('%', @Search, '%')
        )

        ORDER BY c.UserId DESC

        LIMIT @PageSize OFFSET @Offset;";

            var data = await connection.QueryAsync<ServiceBookingClientResponse>(
                sql,
                new
                {
                    UserId = userId,
                    Search = search,
                    PageSize = pageSize,
                    Offset = (pageNumber - 1) * pageSize
                });

            return new PagedResult<ServiceBookingClientResponse>
            {
                TotalCount = totalCount,
                Details = data
            };
        }
    }
}