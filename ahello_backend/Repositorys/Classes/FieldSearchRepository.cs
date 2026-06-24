using ahello_backend.DbContexts;
using ahello_backend.Models.DynamicSearch;
using ahello_backend.Repositorys.Interfaces;
using Dapper;

namespace ahello_backend.Repositorys.Classes
{
    public class FieldSearchRepository : IFieldSearchRepository
    {
        private readonly DbContext _db;

        public FieldSearchRepository(DbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<DynamicFieldSearch>>
            SearchFieldAsync(int userId, string keyword)
        {
            var sql = @"

            SELECT
                'User' AS Module,
                uf.UserFieldId AS FieldId,
                uf.FieldName,
                uf.DataTypeId,
                dt.DataTypeName
            FROM userfields uf
            INNER JOIN datatypes dt
                ON dt.DataTypeId = uf.DataTypeId
            WHERE uf.UserId = @UserId
            AND uf.IsActive = 1
            AND uf.FieldName LIKE @Keyword

            UNION ALL

            SELECT
                'Service' AS Module,
                sf.ServiceFieldId AS FieldId,
                sf.FieldName,
                sf.DataTypeId,
                dt.DataTypeName
            FROM servicefields sf
            INNER JOIN services s
                ON sf.ServiceId = s.ServiceId
            INNER JOIN datatypes dt
                ON dt.DataTypeId = sf.DataTypeId
            WHERE s.UserId = @UserId
            AND sf.IsActive = 1
            AND sf.FieldName LIKE @Keyword

            ORDER BY FieldName;";

            using var connection = _db.GetConnection();

            return await connection.QueryAsync<DynamicFieldSearch>(
                sql,
                new
                {
                    UserId = userId,
                    Keyword = $"%{keyword.Trim()}%"
                });
        }
    }
}
