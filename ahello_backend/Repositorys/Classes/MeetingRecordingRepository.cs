using ahello_backend.DbContexts;
using ahello_backend.Models.MeetingParticipant;
using ahello_backend.Repositorys.Interfaces;
using Dapper;

namespace ahello_backend.Repositorys.Classes
{
    public class MeetingRecordingRepository
        : IMeetingRecordingRepository
    {
        private readonly DbContext _db;

        public MeetingRecordingRepository(
            DbContext db)
        {
            _db = db;
        }


        // =========================================================
        // CREATE RECORDING
        // =========================================================

        public async Task<int> CreateAsync(
            MeetingRecording model)
        {
            using var connection =
                _db.GetConnection();

            var sql = @"
                INSERT INTO MeetingRecording
                (
                    MeetingId,
                    RoomId,
                    RoomName,
                    HMSRecordingId,
                    StartedAt,
                    Status,
                    CreatedAt
                )
                VALUES
                (
                    @MeetingId,
                    @RoomId,
                    @RoomName,
                    @HMSRecordingId,
                    @StartedAt,
                    @Status,
                    NOW()
                );

                SELECT LAST_INSERT_ID();
            ";

            return await connection
                .ExecuteScalarAsync<int>(
                    sql,
                    new
                    {
                        model.MeetingId,
                        model.RoomId,
                        model.RoomName,
                        model.HMSRecordingId,
                        model.StartedAt,
                        model.Status
                    });
        }


        // =========================================================
        // GET ACTIVE RECORDING
        // =========================================================

        public async Task<MeetingRecording?>
            GetActiveAsync(
                int meetingId)
        {
            using var connection =
                _db.GetConnection();

            var sql = @"
                SELECT
                    RecordingId,
                    MeetingId,
                    RoomId,
                    RoomName,
                    HMSRecordingId,
                    RecordingAssetId,
                    FileName,
                    RecordingUrl,
                    StartedAt,
                    EndedAt,
                    DurationSeconds,
                    Status,
                    DeleteAt,
                    IsDeleted,
                    CreatedAt

                FROM MeetingRecording

                WHERE MeetingId = @MeetingId
                  AND IsDeleted = 0
                  AND Status IN
                  (
                      'Starting',
                      'Recording',
                      'Stopping',
                      'Processing'
                  )

                ORDER BY CreatedAt DESC

                LIMIT 1;
            ";

            return await connection
                .QueryFirstOrDefaultAsync<MeetingRecording>(
                    sql,
                    new
                    {
                        MeetingId = meetingId
                    });
        }


        // =========================================================
        // GET RECORDING BY ID
        // =========================================================

        public async Task<MeetingRecording?>
            GetByIdAsync(
                int recordingId)
        {
            using var connection =
                _db.GetConnection();

            var sql = @"
                SELECT
                    RecordingId,
                    MeetingId,
                    RoomId,
                    RoomName,
                    HMSRecordingId,
                    RecordingAssetId,
                    FileName,
                    RecordingUrl,
                    StartedAt,
                    EndedAt,
                    DurationSeconds,
                    Status,
                    DeleteAt,
                    IsDeleted,
                    CreatedAt

                FROM MeetingRecording

                WHERE RecordingId = @RecordingId
                  AND IsDeleted = 0

                LIMIT 1;
            ";

            return await connection
                .QueryFirstOrDefaultAsync<MeetingRecording>(
                    sql,
                    new
                    {
                        RecordingId = recordingId
                    });
        }


        // =========================================================
        // GET RECORDING BY MEETING ID
        // =========================================================

        public async Task<MeetingRecording?>
            GetByMeetingIdAsync(
                int meetingId)
        {
            using var connection =
                _db.GetConnection();

            var sql = @"
                SELECT
                    RecordingId,
                    MeetingId,
                    RoomId,
                    RoomName,
                    HMSRecordingId,
                    RecordingAssetId,
                    FileName,
                    RecordingUrl,
                    StartedAt,
                    EndedAt,
                    DurationSeconds,
                    Status,
                    DeleteAt,
                    IsDeleted,
                    CreatedAt

                FROM MeetingRecording

                WHERE MeetingId = @MeetingId
                  AND IsDeleted = 0

                ORDER BY CreatedAt DESC

                LIMIT 1;
            ";

            return await connection
                .QueryFirstOrDefaultAsync<MeetingRecording>(
                    sql,
                    new
                    {
                        MeetingId = meetingId
                    });
        }


        // =========================================================
        // GET ALL RECORDINGS
        // =========================================================

        public async Task<IEnumerable<MeetingRecording>>
            GetAllAsync()
        {
            using var connection =
                _db.GetConnection();

            var sql = @"
                SELECT
                    RecordingId,
                    MeetingId,
                    RoomId,
                    RoomName,
                    HMSRecordingId,
                    RecordingAssetId,
                    FileName,
                    RecordingUrl,
                    StartedAt,
                    EndedAt,
                    DurationSeconds,
                    Status,
                    DeleteAt,
                    IsDeleted,
                    CreatedAt

                FROM MeetingRecording

                WHERE IsDeleted = 0

                ORDER BY CreatedAt DESC;
            ";

            return await connection
                .QueryAsync<MeetingRecording>(sql);
        }


        // =========================================================
        // UPDATE STATUS
        // =========================================================

        public async Task<bool> UpdateStatusAsync(
            int recordingId,
            string status)
        {
            using var connection =
                _db.GetConnection();

            var sql = @"
                UPDATE MeetingRecording

                SET
                    Status = @Status

                WHERE RecordingId =
                    @RecordingId;
            ";

            var rows =
                await connection.ExecuteAsync(
                    sql,
                    new
                    {
                        RecordingId = recordingId,
                        Status = status
                    });

            return rows > 0;
        }


        // =========================================================
        // UPDATE STOPPED RECORDING
        // =========================================================

        public async Task<bool> UpdateStoppedAsync(
            int recordingId,
            DateTime endedAt,
            int durationSeconds)
        {
            using var connection =
                _db.GetConnection();

            var sql = @"
                UPDATE MeetingRecording

                SET
                    EndedAt = @EndedAt,
                    DurationSeconds = @DurationSeconds,
                    Status = 'Processing'

                WHERE RecordingId =
                    @RecordingId;
            ";

            var rows =
                await connection.ExecuteAsync(
                    sql,
                    new
                    {
                        RecordingId = recordingId,
                        EndedAt = endedAt,
                        DurationSeconds =
                            durationSeconds
                    });

            return rows > 0;
        }


        // =========================================================
        // UPDATE COMPLETED RECORDING
        // =========================================================

        public async Task<bool> UpdateCompletedAsync(
            int recordingId,
            string? recordingAssetId,
            string? recordingUrl,
            string? fileName,
            DateTime? endedAt,
            int? durationSeconds)
        {
            using var connection =
                _db.GetConnection();

            var sql = @"
                UPDATE MeetingRecording

                SET
                    RecordingAssetId =
                        @RecordingAssetId,

                    RecordingUrl =
                        @RecordingUrl,

                    FileName =
                        @FileName,

                    EndedAt =
                        @EndedAt,

                    DurationSeconds =
                        @DurationSeconds,

                    Status =
                        'Completed'

                WHERE RecordingId =
                    @RecordingId;
            ";

            var rows =
                await connection.ExecuteAsync(
                    sql,
                    new
                    {
                        RecordingId = recordingId,
                        RecordingAssetId =
                            recordingAssetId,
                        RecordingUrl =
                            recordingUrl,
                        FileName =
                            fileName,
                        EndedAt =
                            endedAt,
                        DurationSeconds =
                            durationSeconds
                    });

            return rows > 0;
        }


        // =========================================================
        // UPDATE FAILED RECORDING
        // =========================================================

        public async Task<bool> UpdateFailedAsync(
            int recordingId)
        {
            using var connection =
                _db.GetConnection();

            var sql = @"
                UPDATE MeetingRecording

                SET
                    Status = 'Failed'

                WHERE RecordingId =
                    @RecordingId;
            ";

            var rows =
                await connection.ExecuteAsync(
                    sql,
                    new
                    {
                        RecordingId = recordingId
                    });

            return rows > 0;
        }
        // =========================================================
        // GET BY HMS RECORDING ID
        // =========================================================

        public async Task<MeetingRecording?>
            GetByHmsRecordingIdAsync(
                string hmsRecordingId)
        {
            using var connection =
                _db.GetConnection();

            var sql = @"
        SELECT
            RecordingId,
            MeetingId,
            RoomId,
            RoomName,
            HMSRecordingId,
            RecordingAssetId,
            FileName,
            RecordingUrl,
            StartedAt,
            EndedAt,
            DurationSeconds,
            Status,
            DeleteAt,
            IsDeleted,
            CreatedAt

        FROM MeetingRecording

        WHERE HMSRecordingId = @HMSRecordingId
          AND IsDeleted = 0

        LIMIT 1;
    ";

            return await connection
                .QueryFirstOrDefaultAsync<MeetingRecording>(
                    sql,
                    new
                    {
                        HMSRecordingId =
                            hmsRecordingId
                    });
        }

        public async Task<MeetingRecording?>
    GetByRoomIdAsync(
        string roomId)
        {
            using var connection =
                _db.GetConnection();

            var sql = @"
        SELECT
            RecordingId,
            MeetingId,
            RoomId,
            RoomName,
            HMSRecordingId,
            RecordingAssetId,
            FileName,
            RecordingUrl,
            StartedAt,
            EndedAt,
            DurationSeconds,
            Status,
            DeleteAt,
            IsDeleted,
            CreatedAt

        FROM MeetingRecording

        WHERE RoomId = @RoomId
          AND IsDeleted = 0

        ORDER BY CreatedAt DESC

        LIMIT 1;
    ";

            return await connection
                .QueryFirstOrDefaultAsync<MeetingRecording>(
                    sql,
                    new
                    {
                        RoomId = roomId
                    });
        }
    }
}