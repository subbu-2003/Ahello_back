using ahello_backend.DbContexts;
using ahello_backend.Models.Service;
using ahello_backend.Repositorys.Interfaces;
using Dapper;

namespace ahello_backend.Repositorys.Classes
{
    public class ServiceDropdownOptionRepository
        : IServiceDropdownOptionRepository
    {
        private readonly DbContext _db;

        public ServiceDropdownOptionRepository(
            DbContext db)
        {
            _db = db;
        }

        public async Task<int> CreateAsync(
            CreateServiceDropdownOption model)
        {
            var sql = @"
            INSERT INTO servicedropdownoptions
            (
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
                @ServiceFieldId,
                @UserId,
                @OptionValue,
                @OptionLabel,
                @IsActive,
                NOW(),
                @CreatedBy
            );";

            using var connection = _db.GetConnection();

            int rows = 0;

            foreach (var opt in model.Options)
            {
                rows += await connection.ExecuteAsync(
                    sql,
                    new
                    {
                        model.ServiceFieldId,
                        model.UserId,
                        opt.OptionValue,
                        opt.OptionLabel,
                        opt.IsActive,
                        model.CreatedBy
                    });
            }

            return rows;
        }

        public async Task<bool> UpdateAsync(
            UpdateServiceDropdownOption model)
        {
            var sql = @"
            UPDATE servicedropdownoptions
            SET
                OptionValue = @OptionValue,
                OptionLabel = @OptionLabel,
                IsActive = @IsActive,
                ModifiedDate = NOW(),
                ModifiedBy = @ModifiedBy
            WHERE ServiceDropDownId = @ServiceDropDownId";

            using var connection = _db.GetConnection();

            var rows = await connection.ExecuteAsync(
                sql,
                model);

            return rows > 0;
        }

        public async Task<GetServiceDropdownOption>
            GetByIdAsync(
                int optionId)
        {
            var sql = @"
            SELECT *
            FROM servicedropdownoptions
            WHERE ServiceDropDownId = @optionId";

            using var connection = _db.GetConnection();

            return await connection
                .QueryFirstOrDefaultAsync<GetServiceDropdownOption>(
                    sql,
                    new { optionId });
        }

        public async Task<IEnumerable<GetServiceDropdownOption>>
            GetByFieldIdAsync(
                int serviceFieldId,
                int? UserId)
        {
            var sql = @"
            SELECT *
            FROM servicedropdownoptions
            WHERE ServiceFieldId = @serviceFieldId
            AND (@UserId IS NULL 
                 OR UserId = @UserId)
            AND IsActive = 1";

            using var connection = _db.GetConnection();

            return await connection
                .QueryAsync<GetServiceDropdownOption>(
                    sql,
                    new
                    {
                        serviceFieldId,
                        UserId
                    });
        }
    }
}
