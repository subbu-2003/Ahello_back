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
            using var connection = _db.GetConnection();

            // CHECK DUPLICATE FIELDNAME
            var duplicateSql = @"SELECT COUNT(*)
                                 FROM servicefields
                                 WHERE UserId = @UserId
                                 AND LOWER(FieldName) = LOWER(@FieldName)
                                 AND IsActive = 1";

            var exists = await connection.ExecuteScalarAsync<int>(
                duplicateSql,
                new
                {
                    model.UserId,
                    model.FieldName
                });

            if (exists > 0)
            {
                throw new Exception("FieldName already exists for this service.");
            }

            var sql = @"INSERT INTO servicefields
                        (
                            UserId,
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
                            @UserId,
                            @FieldName,
                            @FieldCode,
                            @Placeholder,
                            @IsRequired,
                            1,
                            @DataTypeId,
                            @CreatedBy
                        );

                        SELECT LAST_INSERT_ID();";

            return await connection.ExecuteScalarAsync<int>(sql, model);
        }

        public async Task<bool> UpdateAsync(UpdateServiceField model)
        {
            using var connection = _db.GetConnection();

            // CHECK DUPLICATE FIELDNAME
            var duplicateSql = @"SELECT COUNT(*)
                                 FROM servicefields
                                 WHERE UserId = @UserId
                                 AND LOWER(FieldName) = LOWER(@FieldName)
                                 AND ServiceFieldId != @ServiceFieldId
                                 AND IsActive = 1";

            var exists = await connection.ExecuteScalarAsync<int>(
                duplicateSql,
                new
                {
                    model.UserId,
                    model.FieldName,
                    model.ServiceFieldId
                });

            if (exists > 0)
            {
                throw new Exception("FieldName already exists for this service.");
            }

            var sql = @"UPDATE servicefields
                        SET
                            UserId = @UserId,
                            FieldName = @FieldName,
                            FieldCode = @FieldCode,
                            Placeholder = @Placeholder,
                            IsRequired = @IsRequired,
                            IsActive = @IsActive,
                            DataTypeId = @DataTypeId,
                            ModifiedBy = @ModifiedBy
                        WHERE ServiceFieldId = @ServiceFieldId";

            var rows = await connection.ExecuteAsync(sql, model);

            return rows > 0;
        }

        public async Task<IEnumerable<ServiceField>> GetByServiceAsync(int UserId)
        {
            var sql = @"
    SELECT 
        sf.ServiceFieldId,
        sf.UserId,
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
        sdo.UserId,
        sdo.OptionValue,
        sdo.OptionLabel,
        sdo.IsActive

    FROM servicefields sf

    LEFT JOIN servicedropdownoptions sdo
        ON sf.ServiceFieldId = sdo.ServiceFieldId
        AND sdo.IsActive = 1

    WHERE sf.UserId = @UserId
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
                new { UserId = UserId },
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


public async Task<IEnumerable<ServiceField>> GetByUserAsync(string userId)
{
    var sql = @"
    SELECT 
        sf.ServiceFieldId,
        sf.UserId,
        sf.FieldName,
        sf.FieldCode,
        sf.Placeholder,
        sf.IsRequired,
        sf.IsActive,
        sf.DataTypeId,
        dt.DataTypeName,
        sf.CreatedBy,
        sf.CreatedAt,
        sf.ModifiedBy,
        sf.ModifiedAt,

        sdo.ServiceDropDownId,
        sdo.ServiceFieldId,
        sdo.UserId,
        sdo.OptionValue,
        sdo.OptionLabel,
        sdo.IsActive

    FROM servicefields sf

    LEFT JOIN datatypes dt
        ON sf.DataTypeId = dt.DataTypeId

    LEFT JOIN servicedropdownoptions sdo
        ON sf.ServiceFieldId = sdo.ServiceFieldId
        AND sdo.IsActive = 1

    WHERE sf.UserId = @UserId
    AND sf.IsActive = 1

    ORDER BY sf.ServiceFieldId DESC";


    using var connection = _db.GetConnection();

    var fieldDictionary = new Dictionary<int, ServiceField>();

    await connection.QueryAsync<ServiceField, ServiceDropdownOptionModel, ServiceField>(
        sql,
        (field, dropdown) =>
        {
            if (!fieldDictionary.TryGetValue(field.ServiceFieldId, out var existingField))
            {
                existingField = field;

                existingField.DropdownOptions =
                    new List<ServiceDropdownOptionModel>();

                fieldDictionary.Add(
                    existingField.ServiceFieldId,
                    existingField
                );
            }


            if (dropdown != null &&
                dropdown.ServiceDropDownId > 0)
            {
                existingField.DropdownOptions.Add(dropdown);
            }

            return existingField;

        },
        new { UserId = userId },
        splitOn: "ServiceDropDownId"
    );


    return fieldDictionary.Values;
}
        public async Task<IEnumerable<ServiceField>> GetByUserAllAsync(string userId)
        {
            var sql = @"
        SELECT 
            sf.ServiceFieldId,
            sf.FieldName,
            sf.FieldCode,
            sf.Placeholder,
            sf.IsRequired,
            sf.IsActive,
            sf.DataTypeId,
            dt.DataTypeName,
            sf.CreatedBy,
            sf.CreatedAt,
            sf.ModifiedBy,
            sf.ModifiedAt,

            sdo.ServiceDropDownId,
            sdo.ServiceFieldId,
            sdo.OptionValue,
            sdo.OptionLabel,
            sdo.IsActive

        FROM servicefields sf

        LEFT JOIN datatypes dt
            ON sf.DataTypeId = dt.DataTypeId

        LEFT JOIN servicedropdownoptions sdo
            ON sf.ServiceFieldId = sdo.ServiceFieldId
            AND sdo.IsActive = 1

        WHERE sf.CreatedBy = @UserId
          AND sf.IsActive = 1

        ORDER BY sf.ServiceFieldId DESC";

            using var connection = _db.GetConnection();

            var fieldDictionary = new Dictionary<int, ServiceField>();

            await connection.QueryAsync<
                ServiceField,
                ServiceDropdownOptionModel,
                ServiceField>(
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
                        existingField.DropdownOptions.Add(dropdown);
                    }

                    return existingField;
                },
                new { UserId = userId },
                splitOn: "ServiceDropDownId");

            return fieldDictionary.Values;
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