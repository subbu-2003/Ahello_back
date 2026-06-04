using ahello_backend.DbContexts;
using ahello_backend.Models.Service;
using ahello_backend.Repositorys.Interfaces;
using Dapper;

namespace ahello_backend.Repositorys.Classes
{
    public class ServiceFieldRepository : IServiceFieldRepository
    {
        private readonly DbContext _db;

        public ServiceFieldRepository(DbContext db)
        {
            _db = db;
        }

        public async Task<int> CreateAsync(CreateServiceField model)
        {
            var sql = @"INSERT INTO servicefields
                        (
                            ServiceId,
                            FieldName,
                            FieldCode,
                            Placeholder,
                            IsRequired,
                            IsActive,
                            DataTypeId,
                            CreatedBy
                        )
                        VALUES
                        (
                            @ServiceId,
                            @FieldName,
                            @FieldCode,
                            @Placeholder,
                            @IsRequired,
                            1,
                            @DataTypeId,
                            @CreatedBy
                        );

                        SELECT LAST_INSERT_ID();";

            using var connection = _db.GetConnection();

            return await connection.ExecuteScalarAsync<int>(sql, model);
        }

        public async Task<bool> UpdateAsync(UpdateServiceField model)
        {
            var sql = @"UPDATE servicefields
                        SET
                            ServiceId = @ServiceId,
                            FieldName = @FieldName,
                            FieldCode = @FieldCode,
                            Placeholder = @Placeholder,
                            IsRequired = @IsRequired,
                            IsActive = @IsActive,
                            DataTypeId = @DataTypeId,
                            ModifiedBy = @ModifiedBy
                        WHERE ServiceFieldId = @ServiceFieldId";

            using var connection = _db.GetConnection();

            var rows = await connection.ExecuteAsync(sql, model);

            return rows > 0;
        }

        public async Task<IEnumerable<ServiceField>>
    GetByServiceAsync(int serviceId)
        {
            var sql = @"
    SELECT 
        sf.ServiceFieldId,
        sf.ServiceId,
        sf.FieldName,
        sf.FieldCode,
        sf.Placeholder,
        sf.IsRequired,
        sf.IsActive,
        sf.DataTypeId,
        sf.CreatedBy,
        sf.CreatedAt,
        sf.ModifiedBy,
        sf.ModifiedAt,

        sdo.ServiceDropDownId,
        sdo.ServiceFieldId,
        sdo.ServiceId,
        sdo.OptionValue,
        sdo.OptionLabel,
        sdo.IsActive

    FROM servicefields sf

    LEFT JOIN servicedropdownoptions sdo
        ON sf.ServiceFieldId = sdo.ServiceFieldId
        AND sdo.IsActive = 1

    WHERE sf.ServiceId = @ServiceId
    AND sf.IsActive = 1

    ORDER BY sf.ServiceFieldId DESC";

            using var connection = _db.GetConnection();

            var fieldDictionary =
                new Dictionary<int, ServiceField>();

            var result = await connection.QueryAsync
            <
                ServiceField,
                ServiceDropdownOptionModel,
                ServiceField
            >
            (
                sql,
                (field, dropdown) =>
                {
                    if (!fieldDictionary.TryGetValue(
                            field.ServiceFieldId,
                            out var existingField))
                    {
                        existingField = field;

                        existingField.DropdownOptions =
                            new List<ServiceDropdownOptionModel>();

                        fieldDictionary.Add(
                            existingField.ServiceFieldId,
                            existingField);
                    }

                    if (dropdown != null &&
                        dropdown.ServiceDropDownId > 0)
                    {
                        existingField.DropdownOptions
                            .Add(dropdown);
                    }

                    return existingField;
                },
                new { ServiceId = serviceId },
                splitOn: "ServiceDropDownId"
            );

            return fieldDictionary.Values;
        }

        public async Task<ServiceField> GetByIdAsync(int serviceFieldId)
        {
            var sql = @"SELECT *
                        FROM servicefields
                        WHERE ServiceFieldId = @ServiceFieldId";

            using var connection = _db.GetConnection();

            return await connection.QueryFirstOrDefaultAsync<ServiceField>(
                sql,
                new { ServiceFieldId = serviceFieldId }
            );
        }

        public async Task<bool> DeleteAsync(int serviceFieldId, string modifiedBy)
        {
            var sql = @"UPDATE servicefields
                        SET
                            IsActive = 0,
                            ModifiedBy = @ModifiedBy
                        WHERE ServiceFieldId = @ServiceFieldId";

            using var connection = _db.GetConnection();

            var rows = await connection.ExecuteAsync(sql,
                new
                {
                    ServiceFieldId = serviceFieldId,
                    ModifiedBy = modifiedBy
                });

            return rows > 0;
        }
    }
}