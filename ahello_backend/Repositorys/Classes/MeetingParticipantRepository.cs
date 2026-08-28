using ahello_backend.DbContexts;
using ahello_backend.Models.MeetingParticipant;
using ahello_backend.Repositorys.Interfaces;
using Dapper;

namespace ahello_backend.Repositorys.Classes
{
    public class MeetingParticipantRepository
        : IMeetingParticipantRepository
    {
        private readonly DbContext _db;

        public MeetingParticipantRepository(DbContext db)
        {
            _db = db;
        }

        // =========================================================
        // CREATE PARTICIPANT SESSION
        // =========================================================

        public async Task<int> CreateAsync(
            MeetingParticipant model)
        {
            using var connection = _db.GetConnection();

            var sql = @"
                INSERT INTO MeetingParticipant
                (
                    MeetingId,
                    UserId,
                    JoinedAt
                )
                VALUES
                (
                    @MeetingId,
                    @UserId,
                    @JoinedAt
                );

                SELECT LAST_INSERT_ID();
            ";

            return await connection.ExecuteScalarAsync<int>(
                sql,
                new
                {
                    model.MeetingId,
                    model.UserId,
                    model.JoinedAt
                });
        }


        // =========================================================
        // CLOSE PARTICIPANT SESSION
        // =========================================================

        public async Task<bool> LeaveAsync(
            int meetingParticipantId)
        {
            using var connection = _db.GetConnection();

            var sql = @"
                UPDATE MeetingParticipant
                SET
                    LeftAt = NOW(),
                    DurationSeconds =
                        TIMESTAMPDIFF(
                            SECOND,
                            JoinedAt,
                            NOW()
                        )
                WHERE MeetingParticipantId =
                    @MeetingParticipantId
                  AND LeftAt IS NULL;
            ";

            var rows = await connection.ExecuteAsync(
                sql,
                new
                {
                    MeetingParticipantId =
                        meetingParticipantId
                });

            return rows > 0;
        }


        // =========================================================
        // GET ACTIVE PARTICIPANT SESSION
        // =========================================================

        public async Task<MeetingParticipant?>
            GetActiveAsync(
                int meetingId,
                int userId)
        {
            using var connection = _db.GetConnection();

            var sql = @"
                SELECT
                    MeetingParticipantId,
                    MeetingId,
                    UserId,
                    JoinedAt,
                    LeftAt,
                    DurationSeconds,
                    CreatedAt

                FROM MeetingParticipant

                WHERE MeetingId = @MeetingId
                  AND UserId = @UserId
                  AND LeftAt IS NULL

                ORDER BY JoinedAt DESC

                LIMIT 1;
            ";

            return await connection.QueryFirstOrDefaultAsync<MeetingParticipant>(
                sql,
                new
                {
                    MeetingId = meetingId,
                    UserId = userId
                });
        }


        // =========================================================
        // GET COMPLETE MEETING LOG
        // =========================================================

        public async Task<IEnumerable<MeetingParticipantLog>>
            GetByMeetingIdAsync(
                int meetingId)
        {
            using var connection = _db.GetConnection();

            var sql = @"
                SELECT
                    mp.MeetingParticipantId,
                    mp.MeetingId,
                    mp.UserId,

                    u.FullName AS UserName,
            CASE
                WHEN mp.UserId = m.UserId THEN 'Host'
                ELSE 'Client'
            END AS ParticipantType,

                    mp.JoinedAt,
                    mp.LeftAt,
                    mp.DurationSeconds,
            CASE
                WHEN mp.DurationSeconds IS NULL THEN NULL

                WHEN mp.DurationSeconds >= 3600 THEN
                    CONCAT(
                        FLOOR(mp.DurationSeconds / 3600), ' hr ',
                        FLOOR((mp.DurationSeconds % 3600) / 60), ' min ',
                        mp.DurationSeconds % 60, ' sec'
                    )

                WHEN mp.DurationSeconds >= 60 THEN
                    CONCAT(
                        FLOOR(mp.DurationSeconds / 60), ' min ',
                        mp.DurationSeconds % 60, ' sec'
                    )

                ELSE
                    CONCAT(mp.DurationSeconds, ' sec')
            END AS Duration,
                    mp.CreatedAt

                FROM MeetingParticipant mp

                INNER JOIN meetings m
                    ON mp.MeetingId = m.MeetingId

                INNER JOIN users u
                    ON mp.UserId = u.UserId

                WHERE mp.MeetingId = @MeetingId

                ORDER BY mp.JoinedAt ASC;
            ";

            return await connection.QueryAsync<MeetingParticipantLog>(
                sql,
                new
                {
                    MeetingId = meetingId
                });
        }


        // =========================================================
        // GET USER PARTICIPATION HISTORY
        // =========================================================

        public async Task<ParticipantMeetingLogResponse?>
    GetByParticipantAsync(
        int meetingId,
        int userId)
        {
            using var connection = _db.GetConnection();

            var sql = @"
        SELECT
            mp.MeetingParticipantId,
            mp.MeetingId,
            mp.UserId,

            u.FullName AS UserName,

            CASE
                WHEN mp.UserId = m.UserId THEN 'Host'
                ELSE 'Client'
            END AS ParticipantType,

            mp.JoinedAt,
            mp.LeftAt,
            mp.DurationSeconds,

            CASE
                WHEN mp.DurationSeconds IS NULL THEN NULL

                WHEN mp.DurationSeconds >= 3600 THEN
                    CONCAT(
                        FLOOR(mp.DurationSeconds / 3600), ' hr ',
                        FLOOR((mp.DurationSeconds % 3600) / 60), ' min ',
                        mp.DurationSeconds % 60, ' sec'
                    )

                WHEN mp.DurationSeconds >= 60 THEN
                    CONCAT(
                        FLOOR(mp.DurationSeconds / 60), ' min ',
                        mp.DurationSeconds % 60, ' sec'
                    )

                ELSE
                    CONCAT(mp.DurationSeconds, ' sec')
            END AS Duration,

            mp.CreatedAt

        FROM MeetingParticipant mp

        INNER JOIN meetings m
            ON mp.MeetingId = m.MeetingId

        INNER JOIN users u
            ON mp.UserId = u.UserId

        WHERE mp.MeetingId = @MeetingId
          AND mp.UserId = @UserId

        ORDER BY mp.JoinedAt ASC;
    ";

            var sessions =
                (await connection.QueryAsync<MeetingParticipantLog>(
                    sql,
                    new
                    {
                        MeetingId = meetingId,
                        UserId = userId
                    }))
                .ToList();

            if (!sessions.Any())
            {
                return null;
            }

            var totalDurationSeconds =
                sessions.Sum(x => x.DurationSeconds ?? 0);

            string totalDuration;

            var hours = totalDurationSeconds / 3600;
            var minutes = (totalDurationSeconds % 3600) / 60;
            var seconds = totalDurationSeconds % 60;

            if (hours > 0)
            {
                totalDuration =
                    $"{hours} hr {minutes} min {seconds} sec";
            }
            else if (minutes > 0)
            {
                totalDuration =
                    $"{minutes} min {seconds} sec";
            }
            else
            {
                totalDuration =
                    $"{seconds} sec";
            }

            var firstSession = sessions.First();

            return new ParticipantMeetingLogResponse
            {
                MeetingId = meetingId,

                UserId = userId,

                UserName = firstSession.UserName,

                ParticipantType = firstSession.ParticipantType,

                TotalDurationSeconds = totalDurationSeconds,

                TotalDuration = totalDuration,

                Sessions = sessions
            };
        }
        public async Task<int> GetActiveParticipantCountAsync(int meetingId)
        {
            using var connection = _db.GetConnection();

            var sql = @"
        SELECT COUNT(*)
        FROM MeetingParticipant
        WHERE MeetingId = @MeetingId
          AND LeftAt IS NULL;
    ";

            return await connection.ExecuteScalarAsync<int>(
                sql,
                new { MeetingId = meetingId });
        }

        public async Task<MeetingParticipant?> GetByIdAsync(
            int meetingParticipantId)
        {
            using var connection = _db.GetConnection();

            var sql = @"
        SELECT
            MeetingParticipantId,
            MeetingId,
            UserId,
            JoinedAt,
            LeftAt,
            DurationSeconds,
            CreatedAt
        FROM MeetingParticipant
        WHERE MeetingParticipantId = @MeetingParticipantId
        LIMIT 1;
    ";

            return await connection.QueryFirstOrDefaultAsync<MeetingParticipant>(
                sql,
                new { MeetingParticipantId = meetingParticipantId });
        }
        // =========================================================
        // GET ALL PARTICIPANT LOGS
        // =========================================================

        public async Task<IEnumerable<MeetingParticipantLog>>
            GetAllAsync()
        {
            using var connection = _db.GetConnection();

            var sql = @"
                SELECT
                    mp.MeetingParticipantId,
                    mp.MeetingId,
                    mp.UserId,

                    u.FullName AS UserName,
                    CASE
                        WHEN mp.UserId = m.UserId THEN 'Host'
                        ELSE 'Client'
                    END AS ParticipantType,

                    mp.JoinedAt,
                    mp.LeftAt,
                    mp.DurationSeconds,
                    CASE
                        WHEN mp.DurationSeconds IS NULL THEN NULL

                        WHEN mp.DurationSeconds >= 3600 THEN
                            CONCAT(
                                FLOOR(mp.DurationSeconds / 3600), ' hr ',
                                FLOOR((mp.DurationSeconds % 3600) / 60), ' min ',
                                mp.DurationSeconds % 60, ' sec'
                            )

                        WHEN mp.DurationSeconds >= 60 THEN
                            CONCAT(
                                FLOOR(mp.DurationSeconds / 60), ' min ',
                                mp.DurationSeconds % 60, ' sec'
                            )

                        ELSE
                            CONCAT(mp.DurationSeconds, ' sec')
                    END AS Duration,
                    mp.CreatedAt

                FROM MeetingParticipant mp

                INNER JOIN meetings m
                    ON mp.MeetingId = m.MeetingId

                INNER JOIN users u
                    ON mp.UserId = u.UserId

                ORDER BY mp.MeetingId DESC, mp.JoinedAt ASC;
            ";

            return await connection.QueryAsync<MeetingParticipantLog>(sql);
        }


        // =========================================================
        // GET ALL PARTICIPANT LOGS WITH RECORDING VIDEO
        // =========================================================

        public async Task<IEnumerable<MeetingParticipantWithRecordingLog>>
            GetAllWithRecordingAsync()
        {
            using var connection = _db.GetConnection();

            var sql = @"
                SELECT
                    mp.MeetingParticipantId,
                    mp.MeetingId,
                    mp.UserId,

                    u.FullName AS UserName,
                    CASE
                        WHEN mp.UserId = m.UserId THEN 'Host'
                        ELSE 'Client'
                    END AS ParticipantType,

                    mp.JoinedAt,
                    mp.LeftAt,
                    mp.DurationSeconds,
                    CASE
                        WHEN mp.DurationSeconds IS NULL THEN NULL

                        WHEN mp.DurationSeconds >= 3600 THEN
                            CONCAT(
                                FLOOR(mp.DurationSeconds / 3600), ' hr ',
                                FLOOR((mp.DurationSeconds % 3600) / 60), ' min ',
                                mp.DurationSeconds % 60, ' sec'
                            )

                        WHEN mp.DurationSeconds >= 60 THEN
                            CONCAT(
                                FLOOR(mp.DurationSeconds / 60), ' min ',
                                mp.DurationSeconds % 60, ' sec'
                            )

                        ELSE
                            CONCAT(mp.DurationSeconds, ' sec')
                    END AS Duration,
                    mp.CreatedAt,

                    mr.RecordingId,
                    mr.RecordingUrl,
                    mr.FileName,
                    mr.Status AS RecordingStatus,
                    mr.StartedAt AS RecordingStartedAt,
                    mr.EndedAt AS RecordingEndedAt,
                    mr.DurationSeconds AS RecordingDurationSeconds

                FROM MeetingParticipant mp

                INNER JOIN meetings m
                    ON mp.MeetingId = m.MeetingId

                INNER JOIN users u
                    ON mp.UserId = u.UserId

                LEFT JOIN MeetingRecording mr
                    ON mr.MeetingId = mp.MeetingId
                   AND mr.IsDeleted = 0

                ORDER BY mp.MeetingId DESC, mp.JoinedAt ASC;
            ";

            return await connection.QueryAsync<MeetingParticipantWithRecordingLog>(sql);
        }
    }
}