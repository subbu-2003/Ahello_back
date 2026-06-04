using ahello_backend.DbContexts;
using ahello_backend.Models.DataTypes;
using ahello_backend.Repositorys.Interfaces;
using Dapper;

namespace ahello_backend.Repositorys.Classes
{
    public class DataTypeRepository : IDataTypeRepository
    {
        private readonly DbContext _db;

        public DataTypeRepository(DbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<DataType>> GetAllAsync()
        {
            var sql = @"SELECT * FROM DataTypes
                        WHERE IsActive = 1";

            using var connection = _db.GetConnection();

            return await connection.QueryAsync<DataType>(sql);
        }

        public async Task<DataType> GetByIdAsync(int id)
        {
            var sql = @"SELECT * FROM DataTypes
                        WHERE DataTypeId = @Id";

            using var connection = _db.GetConnection();

            return await connection.QueryFirstOrDefaultAsync<DataType>(
                sql,
                new { Id = id }
            );
        }

        public async Task<int> CreateAsync(CreateDataType model)
        {
            var sql = @"INSERT INTO DataTypes
                        (
                            DataTypeName,
                            DataTypeCode,
                            IsActive,
                            CreatedDate,
                            CreatedBy
                        )
                        VALUES
                        (
                            @DataTypeName,
                            @DataTypeCode,
                            1,
                            NOW(),
                            @CreatedBy
                        );

                        SELECT LAST_INSERT_ID();";

            using var connection = _db.GetConnection();

            return await connection.ExecuteScalarAsync<int>(sql, model);
        }

        public async Task<bool> UpdateAsync(UpdateDataType model)
        {
            var sql = @"UPDATE DataTypes
                        SET
                            DataTypeName = @DataTypeName,
                            DataTypeCode = @DataTypeCode,
                            IsActive = @IsActive,
                            ModifiedDate = NOW(),
                            ModifiedBy = @ModifiedBy
                        WHERE DataTypeId = @DataTypeId";

            using var connection = _db.GetConnection();

            var rows = await connection.ExecuteAsync(sql, model);

            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int id, int userId)
        {
            var sql = @"UPDATE DataTypes
                        SET
                            IsActive = 0,
                            ModifiedDate = NOW(),
                            ModifiedBy = @UserId
                        WHERE DataTypeId = @Id";

            using var connection = _db.GetConnection();

            var rows = await connection.ExecuteAsync(sql,
                new
                {
                    Id = id,
                    UserId = userId
                });

            return rows > 0;
        }
    }
}
