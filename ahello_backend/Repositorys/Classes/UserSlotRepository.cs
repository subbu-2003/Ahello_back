using ahello_backend.DbContexts;
using ahello_backend.Models.UserSlots;
using ahello_backend.Repositorys.Interfaces;
using Dapper;

namespace ahello_backend.Repositorys.Classes
{
    public class UserSlotRepository : IUserSlotRepository
    {
        private readonly DbContext _db;

        public UserSlotRepository(DbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<UserSlotGet>> GetAll()
        {
            using var connection = _db.GetConnection();

            return await connection.QueryAsync<UserSlotGet>(
                @"SELECT *
                  FROM userslots
                  ORDER BY SlotDate, StartTime"
            );
        }

        public async Task<IEnumerable<UserSlotGet>> GetByUserAndService(
            int userId,
            int serviceId
        )
        {
            using var connection = _db.GetConnection();

            return await connection.QueryAsync<UserSlotGet>(
                @"SELECT *
                  FROM userslots
                  WHERE UserId = @UserId
                    AND ServiceId = @ServiceId
                  ORDER BY SlotDate, StartTime",
                new
                {
                    UserId = userId,
                    ServiceId = serviceId
                }
            );
        }

        public async Task<IEnumerable<UserSlotGet>> GetAvailableSlots(
      int userId,
      int serviceId,
      DateTime date
  )
        {
            using var connection = _db.GetConnection();

            return await connection.QueryAsync<UserSlotGet>(
                @"SELECT *
          FROM userslots
          WHERE UserId = @UserId
            AND ServiceId = @ServiceId
            AND SlotDate >= @SlotDate
            AND IsBooked = 0
          ORDER BY SlotDate, StartTime",
                new
                {
                    UserId = userId,
                    ServiceId = serviceId,
                    SlotDate = date.Date
                }
            );
        }

        public async Task<UserSlotGet> GetById(int slotId)
        {
            using var connection = _db.GetConnection();

            return await connection.QueryFirstOrDefaultAsync<UserSlotGet>(
                @"SELECT *
                  FROM userslots
                  WHERE SlotId = @SlotId",
                new
                {
                    SlotId = slotId
                }
            );
        }
        public async Task<int> Post(UserSlotPost model)
        {
            using var connection = _db.GetConnection();

            return await connection.ExecuteAsync(
                @"INSERT INTO userslots
            (
                UserId,
                ServiceId,
                SlotDate,
                StartTime,
                EndTime,
                IsBooked,
                RecurrenceType,
                DayOfWeek,
                DayOfMonth,
                CreatedAt,
                CreatedBy
            )
          VALUES
            (
                @UserId,
                @ServiceId,
                @SlotDate,
                @StartTime,
                @EndTime,
                0,
                @RecurrenceType,
                @DayOfWeek,
                @DayOfMonth,
                NOW(),
                @CreatedBy
            )",
                model
            );
        }

        public async Task<int> PostBulk(IEnumerable<UserSlotPost> slots)
        {
            using var connection = _db.GetConnection();

            return await connection.ExecuteAsync(
                @"INSERT INTO userslots
            (
                UserId,
                ServiceId,
                SlotDate,
                StartTime,
                EndTime,
                IsBooked,
                RecurrenceType,
                DayOfWeek,
                DayOfMonth,
                CreatedAt,
                CreatedBy
            )
          VALUES
            (
                @UserId,
                @ServiceId,
                @SlotDate,
                @StartTime,
                @EndTime,
                0,
                @RecurrenceType,
                @DayOfWeek,
                @DayOfMonth,
                NOW(),
                @CreatedBy
            )",
                slots
            );
        }

        public async Task<int> Put(UserSlotPut model)
        {
            using var connection = _db.GetConnection();

            return await connection.ExecuteAsync(
                @"UPDATE userslots
          SET SlotDate       = @SlotDate,
              StartTime      = @StartTime,
              EndTime        = @EndTime,
              RecurrenceType = @RecurrenceType,
              DayOfWeek      = @DayOfWeek,
              DayOfMonth     = @DayOfMonth,
              ModifiedAt     = NOW(),
              ModifiedBy     = @ModifiedBy
          WHERE SlotId = @SlotId
            AND IsBooked = 0",
                model
            );
        }

        public async Task<int> MarkAsBooked(int slotId)
        {
            using var connection = _db.GetConnection();

            return await connection.ExecuteAsync(
                @"UPDATE userslots
                  SET IsBooked = 1
                  WHERE SlotId = @SlotId",
                new
                {
                    SlotId = slotId
                }
            );
        }

        public async Task<int> MarkAsUnbooked(int slotId)
        {
            using var connection = _db.GetConnection();

            return await connection.ExecuteAsync(
                @"UPDATE userslots
                  SET IsBooked = 0
                  WHERE SlotId = @SlotId",
                new
                {
                    SlotId = slotId
                }
            );
        }

        public async Task<int> Delete(int slotId)
        {
            using var connection = _db.GetConnection();

            return await connection.ExecuteAsync(
                @"DELETE FROM userslots
                  WHERE SlotId = @SlotId
                    AND IsBooked = 0",
                new
                {
                    SlotId = slotId
                }
            );
        }
    }
}