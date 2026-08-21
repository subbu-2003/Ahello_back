using ahello_backend.DbContexts;
using Microsoft.AspNetCore.Mvc;
using Dapper;

namespace ahello_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WhatsAppLogController : ControllerBase
    {
        private readonly DbContext _db;

        public WhatsAppLogController(DbContext db) => _db = db;

        // GET api/WhatsAppLog/booking/5
        [HttpGet("booking/{bookingId}")]
        public async Task<IActionResult> GetByBooking(int bookingId)
        {
            using var conn = _db.GetConnection();
            var logs = await conn.QueryAsync(@"
                SELECT * FROM WhatsAppLog
                WHERE BookingId = @BookingId
                ORDER BY CreatedAt DESC;",
                new { BookingId = bookingId });

            return Ok(logs);
        }

        // GET api/WhatsAppLog?page=1&pageSize=20
        [HttpGet]
        public async Task<IActionResult> GetLogs(int page = 1, int pageSize = 20)
        {
            var offset = (page - 1) * pageSize;
            using var conn = _db.GetConnection();

            var logs = await conn.QueryAsync(@"
                SELECT * FROM WhatsAppLog
                ORDER BY CreatedAt DESC
                LIMIT @PageSize OFFSET @Offset;",
                new { PageSize = pageSize, Offset = offset });

            var total = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM WhatsAppLog;");

            return Ok(new { total, logs });
        }
    }
}