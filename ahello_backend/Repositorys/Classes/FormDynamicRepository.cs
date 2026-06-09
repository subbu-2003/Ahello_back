using ahello_backend.DbContexts;
using ahello_backend.Models.Form;
using ahello_backend.Models.Forms;
using ahello_backend.Repositorys.Interfaces;
using Dapper;

namespace ahello_backend.Repositorys.Classes
{
    public class FormDynamicRepository : IFormDynamicRepository
    {
        private readonly DbContext _db;

        public FormDynamicRepository(DbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<FormDynamicGetResponse>>
            GetAllAsync()
        {
            using var connection = _db.GetConnection();

            var forms = (await connection.QueryAsync<FormDynamicGetResponse>(
                @"SELECT *
                  FROM forms
                  ORDER BY FormId DESC")).ToList();

            foreach (var form in forms)
            {
                var fields = await connection.QueryAsync <FormDynamicFieldResponse>(
                @"SELECT
                        ff.FormFieldId,
                        ff.FieldName,
                        ff.FieldCode,
                        ffv.FieldValue
                    FROM formfields ff
                    LEFT JOIN formfieldvalues ffv
                        ON ff.FormFieldId = ffv.FormFieldId
                        AND ffv.FormId = @FormId
                    ORDER BY ff.FormFieldId",
                    new { FormId = form.FormId });

                form.Fields = fields.ToList();

                var dropdowns = await connection.QueryAsync<FormDropdownOptionResponse>(
                    @"SELECT DISTINCT
                        FormDropDownId,
                        FormFieldId,
                        FormId,
                        OptionValue,
                        OptionLabel,
                        IsActive
                    FROM formdropdownoptions
                    WHERE FormId = @FormId",
                    new { FormId = form.FormId });

                form.DropdownOptions =
                    dropdowns.ToList();
            }

            return forms;
        }
        public async Task<FormDynamicGetResponse> GetByIdAsync(int formId)
        {
            using var connection = _db.GetConnection();

            var form = await connection.QueryFirstOrDefaultAsync<FormDynamicGetResponse>(
                @"SELECT *
          FROM forms
          WHERE FormId = @FormId",
                new { FormId = formId });

            if (form == null)
                return null;

            var fields = (await connection.QueryAsync<FormDynamicFieldResponse>(
                @"SELECT
            ff.FormFieldId,
            ff.FieldName,
            ff.FieldCode,
            ff.Placeholder,
            ff.Description,
            ff.IsRequired,
            ff.DataTypeId,
            ffv.FieldValue
          FROM formfields ff
          LEFT JOIN formfieldvalues ffv
                ON ff.FormFieldId = ffv.FormFieldId
               AND ffv.FormId = @FormId
             WHERE ff.FormId = @FormId
          ORDER BY ff.FormFieldId",
                new { FormId = formId }))
                .ToList();

            var dropdowns = (await connection.QueryAsync<FormDropdownOptionResponse>(
                @"SELECT
            FormDropDownId,
            FormFieldId,
            FormId,
            OptionValue,
            OptionLabel,
            IsActive
          FROM formdropdownoptions
          WHERE FormId = @FormId",
                new { FormId = formId }))
                .ToList();

            foreach (var field in fields)
            {
                field.DropDownOptions = dropdowns
                    .Where(x => x.FormFieldId == field.FormFieldId)
                    .ToList();
            }

            form.Fields = fields;

            return form;
        }

        public async Task<IEnumerable<FormDynamicGetResponse>>
            GetByUserIdAsync(int userId)
        {
            using var connection = _db.GetConnection();

            var forms = (await connection.QueryAsync <FormDynamicGetResponse>(
                @"SELECT *
                  FROM forms
                  WHERE UserId = @UserId
                  ORDER BY FormId DESC",
                new { UserId = userId })).ToList();

            foreach (var form in forms)
            {
                var fields = await connection.QueryAsync<FormDynamicFieldResponse>(
                    @"SELECT
                    ff.FormFieldId,
                    ff.FieldName,
                    ff.FieldCode,
                    ff.Placeholder,
                    ff.Description,
                    ff.IsRequired,
                    ff.DataTypeId,
                    dt.DataTypeName
                FROM formfields ff
                LEFT JOIN datatypes dt
                    ON ff.DataTypeId = dt.DataTypeId
                WHERE ff.FormId = @FormId
                ORDER BY ff.FormFieldId",
                    new { FormId = form.FormId });

                form.Fields = fields.ToList();

                var dropdowns = await connection.QueryAsync<FormDropdownOptionResponse>(
                    @"SELECT DISTINCT
                        FormDropDownId,
                        FormFieldId,
                        FormId,
                        OptionValue,
                        OptionLabel,
                        IsActive
                    FROM formdropdownoptions
                    WHERE FormId = @FormId",
                    new { FormId = form.FormId });

                form.DropdownOptions =
                    dropdowns.ToList();
            }

            return forms;
        }

        public async Task<int> CreateAsync(FormDynamicPost model)
        {
            using var connection = _db.GetConnection();

            connection.Open();

            using var tx = connection.BeginTransaction();

            try
            {
                var formSql = @"
            INSERT INTO forms
            (
                UserId,
                Title,
                Description,
                IsActive,
                CreatedAt,
                CreatedBy
            )
            VALUES
            (
                @UserId,
                @Title,
                @Description,
                @IsActive,
                NOW(),
                @CreatedBy
            );

            SELECT LAST_INSERT_ID();";

                var formId = await connection.ExecuteScalarAsync<int>(
                    formSql,
                    model,
                    tx);

                // Save Field Values
                if (model.Fields != null && model.Fields.Any())
                {
                    var fieldSql = @"
                INSERT INTO formfieldvalues
                (
                    FormId,
                    FieldCode,
                    FormFieldId,
                    FieldValue,
                    CreatedDate,
                    CreatedBy,
                    CreatedAt
                )
                SELECT
                    @FormId,
                    ff.FieldCode,
                    @FormFieldId,
                    @FieldValue,
                    NOW(),
                    @CreatedBy,
                    CURRENT_TIMESTAMP
                FROM formfields ff
                WHERE ff.FormFieldId = @FormFieldId;";

                    foreach (var field in model.Fields)
                    {
                        await connection.ExecuteAsync(
                            fieldSql,
                            new
                            {
                                FormId = formId,
                                FormFieldId = field.FormFieldId,
                                FieldValue = field.FieldValue,
                                model.CreatedBy
                            },
                            tx);
                    }

                    // Save Dropdown Options
                    var dropdownSql = @"
                INSERT INTO formdropdownoptions
                (
                    FormFieldId,
                    FormId,
                    OptionValue,
                    OptionLabel,
                    IsActive,
                    CreatedDate,
                    CreatedBy
                )
                VALUES
                (
                    @FormFieldId,
                    @FormId,
                    @OptionValue,
                    @OptionLabel,
                    @IsActive,
                    NOW(),
                    @CreatedBy
                );";

                    foreach (var field in model.Fields)
                    {
                        if (field.DropDownOptions != null &&
                            field.DropDownOptions.Any())
                        {
                            foreach (var option in field.DropDownOptions)
                            {
                                await connection.ExecuteAsync(
                                    dropdownSql,
                                    new
                                    {
                                        FormFieldId = field.FormFieldId,
                                        FormId = formId,
                                        option.OptionValue,
                                        option.OptionLabel,
                                        option.IsActive,
                                        model.CreatedBy
                                    },
                                    tx);
                            }
                        }
                    }
                }

                tx.Commit();

                return formId;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public async Task<bool> UpdateAsync(
            int formId,
            FormDynamicPut model)
        {
            using var connection =
                _db.GetConnection();

            connection.Open();

            using var tx =
                connection.BeginTransaction();

            try
            {
                var sql = @"
                    UPDATE forms
                    SET
                        UserId = @UserId,
                        Title = @Title,
                        Description = @Description,
                        IsActive = @IsActive,
                        ModifiedAt = NOW(),
                        ModifiedBy = @ModifiedBy
                    WHERE FormId = @FormId";

                var rows =
                    await connection.ExecuteAsync(
                    sql,
                    new
                    {
                        FormId = formId,
                        model.UserId,
                        model.Title,
                        model.Description,
                        model.IsActive,
                        model.ModifiedBy
                    },
                    tx);

                if (rows <= 0)
                {
                    tx.Rollback();
                    return false;
                }

                if (model.Fields != null
                    && model.Fields.Any())
                {
                    var fieldSql = @"
                        INSERT INTO formfieldvalues
                        (
                            FormId,
                            FieldCode,
                            FormFieldId,
                            FieldValue,
                            CreatedDate,
                            CreatedBy,
                            CreatedAt
                        )
                        SELECT
                            @FormId,
                            ff.FieldCode,
                            @FormFieldId,
                            @FieldValue,
                            NOW(),
                            @ModifiedBy,
                            CURRENT_TIMESTAMP
                        FROM formfields ff
                        WHERE ff.FormFieldId =
                            @FormFieldId;";

                    foreach (var field
                        in model.Fields)
                    {
                        await connection.ExecuteAsync(
                            fieldSql,
                            new
                            {
                                FormId = formId,
                                FormFieldId =
                                    field.FormFieldId,
                                FieldValue =
                                    field.FieldValue,
                                model.ModifiedBy
                            },
                            tx);
                    }
                }

                if (model.Fields != null
                    && model.Fields.Any())
                {
                    var dropdownSql = @"
                        INSERT INTO
                        formdropdownoptions
                        (
                            FormFieldId,
                            FormId,
                            OptionValue,
                            OptionLabel,
                            IsActive,
                            CreatedDate,
                            CreatedBy
                        )
                        VALUES
                        (
                            @FormFieldId,
                            @FormId,
                            @OptionValue,
                            @OptionLabel,
                            @IsActive,
                            NOW(),
                            @ModifiedBy
                        );";

                    foreach (var field
                        in model.Fields)
                    {
                        if (field.DropDownOptions != null
                            && field.DropDownOptions.Any())
                        {
                            foreach (var option
                                in field.DropDownOptions)
                            {
                                await connection
                                    .ExecuteAsync(
                                    dropdownSql,
                                    new
                                    {
                                        FormFieldId =
                                            field.FormFieldId,
                                        FormId = formId,
                                        option.OptionValue,
                                        option.OptionLabel,
                                        option.IsActive,
                                        model.ModifiedBy
                                    },
                                    tx);
                            }
                        }
                    }
                }

                tx.Commit();

                return true;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public async Task<bool> DeleteAsync(
            int formId)
        {
            using var connection =
                _db.GetConnection();

            connection.Open();

            using var tx =
                connection.BeginTransaction();

            try
            {
                await connection.ExecuteAsync(
                    @"DELETE FROM formfieldvalues
                      WHERE FormId = @FormId",
                    new { FormId = formId },
                    tx);

                await connection.ExecuteAsync(
                    @"DELETE FROM
                      formdropdownoptions
                      WHERE FormId = @FormId",
                    new { FormId = formId },
                    tx);

                var rows =
                    await connection.ExecuteAsync(
                    @"DELETE FROM forms
                      WHERE FormId = @FormId",
                    new { FormId = formId },
                    tx);

                tx.Commit();

                return rows > 0;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }
        public async Task<int> CreateFormTemplateAsync(FormTemplatePost model)
        {
            using var connection = _db.GetConnection();

            connection.Open();

            using var tx = connection.BeginTransaction();

            try
            {
                var formId = await connection.ExecuteScalarAsync<int>(
                    @"INSERT INTO forms
            (
                UserId,
                Title,
                Description,
                IsActive,
                CreatedAt,
                CreatedBy
            )
            VALUES
            (
                @UserId,
                @Title,
                @Description,
                1,
                NOW(),
                @CreatedBy
            );

            SELECT LAST_INSERT_ID();",
                    new
                    {
                        model.UserId,
                        model.Title,
                        model.Description,
                        model.CreatedBy
                    },
                    tx);

                if (model.Fields != null && model.Fields.Any())
                {
                    foreach (var field in model.Fields)
                    {
                        // Insert Form Field
                        var formFieldId = await connection.ExecuteScalarAsync<int>(
                            @"
                    INSERT INTO formfields
                    (
                        FormId,
                        FieldName,
                        FieldCode,
                        Placeholder,
                        Description,
                        IsRequired,
                        IsActive,
                        DataTypeId,
                        CreatedBy,
                        CreatedAt
                    )
                    VALUES
                    (
                        @FormId,
                        @FieldName,
                        @FieldCode,
                        @Placeholder,
                        @Description,
                        @IsRequired,
                        1,
                        @DataTypeId,
                        @CreatedBy,
                        NOW()
                    );

                    SELECT LAST_INSERT_ID();
                    ",
                            new
                            {
                                FormId = formId,
                                field.FieldName,
                                FieldCode = field.FieldName.Replace(" ", "").ToLower(),
                                field.Placeholder,
                                field.Description,
                                field.IsRequired,
                                field.DataTypeId,
                                CreatedBy = model.CreatedBy.ToString()
                            },
                            tx);

                        // Insert Dropdown Options
                        if (field.DropdownOptions != null &&
                            field.DropdownOptions.Any())
                        {
                            foreach (var option in field.DropdownOptions)
                            {
                                await connection.ExecuteAsync(
                                    @"
                            INSERT INTO formdropdownoptions
                            (
                                FormFieldId,
                                FormId,
                                OptionValue,
                                OptionLabel,
                                IsActive,
                                CreatedDate,
                                CreatedBy
                            )
                            VALUES
                            (
                                @FormFieldId,
                                @FormId,
                                @OptionValue,
                                @OptionLabel,
                                1,
                                NOW(),
                                @CreatedBy
                            )
                            ",
                                    new
                                    {
                                        FormFieldId = formFieldId,
                                        FormId = formId,
                                        OptionValue = option.OptionLabel, // auto fill
                                        option.OptionLabel,
                                        CreatedBy = model.CreatedBy.ToString()
                                    },
                                    tx);
                            }
                        }
                    }
                }

                tx.Commit();

                return formId;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }
        public async Task<FormDynamicGetResponse?> GetSubmittedFormAsync(int formId)
        {
            using var connection = _db.GetConnection();

            var form = await connection.QueryFirstOrDefaultAsync<FormDynamicGetResponse>(
                @"SELECT *
          FROM forms
          WHERE FormId = @FormId",
                new { FormId = formId });

            if (form == null)
                return null;

            var fields = (await connection.QueryAsync<FormDynamicFieldResponse>(
                @"SELECT
              ff.FormFieldId,
              ff.FieldName,
              ff.FieldCode,
              ff.Placeholder,
              ff.Description,
              ff.IsRequired,
              ff.DataTypeId,
              MAX(ffv.FieldValue) AS FieldValue
          FROM formfields ff
          LEFT JOIN formfieldvalues ffv
                 ON ff.FormFieldId = ffv.FormFieldId
                AND ffv.FormId = @FormId
          WHERE ff.FormId = @FormId
          GROUP BY
              ff.FormFieldId,
              ff.FieldName,
              ff.FieldCode,
              ff.Placeholder,
              ff.Description,
              ff.IsRequired,
              ff.DataTypeId
          ORDER BY ff.FormFieldId",
                new { FormId = formId }))
                .ToList();

            var dropdowns = (await connection.QueryAsync<FormDropdownOptionResponse>(
                @"SELECT
              FormDropDownId,
              FormFieldId,
              FormId,
              OptionValue,
              OptionLabel,
              IsActive
          FROM formdropdownoptions
          WHERE FormId = @FormId",
                new { FormId = formId }))
                .ToList();

            foreach (var field in fields)
            {
                field.DropDownOptions = dropdowns
                    .Where(x => x.FormFieldId == field.FormFieldId)
                    .ToList();
            }

            form.Fields = fields;

            return form;
        }
       public async Task<bool> UpdateFormTemplateAsync(
    int formId,
    FormTemplatePut model)
{
    using var connection = _db.GetConnection();

    connection.Open();

    using var tx = connection.BeginTransaction();

    try
    {
        var rows = await connection.ExecuteAsync(
            @"
            UPDATE forms
            SET
                UserId = @UserId,
                Title = @Title,
                Description = @Description,
                ModifiedAt = NOW(),
                ModifiedBy = @ModifiedBy
            WHERE FormId = @FormId
            ",
            new
            {
                FormId = formId,
                model.UserId,
                model.Title,
                model.Description,
                model.ModifiedBy
            },
            tx);

        if (rows <= 0)
        {
            tx.Rollback();
            return false;
        }

        if (model.Fields != null && model.Fields.Any())
        {
            foreach (var field in model.Fields)
            {
                int formFieldId = field.FormFieldId;

                // UPDATE EXISTING FIELD
                if (field.FormFieldId > 0)
                {
                    await connection.ExecuteAsync(
                        @"
                        UPDATE formfields
                        SET
                            FieldName = @FieldName,
                            FieldCode = @FieldCode,
                            Placeholder = @Placeholder,
                            Description = @Description,
                            IsRequired = @IsRequired,
                            DataTypeId = @DataTypeId
                        WHERE FormFieldId = @FormFieldId
                        ",
                        new
                        {
                            field.FormFieldId,
                            field.FieldName,
                            FieldCode = field.FieldName
                                .Replace(" ", "")
                                .ToLower(),
                            field.Placeholder,
                            field.Description,
                            field.IsRequired,
                            field.DataTypeId
                        },
                        tx);

                    formFieldId = field.FormFieldId;
                }
                // INSERT NEW FIELD
                else
                {
                    formFieldId =
                        await connection.ExecuteScalarAsync<int>(
                        @"
                        INSERT INTO formfields
                        (
                            FormId,
                            FieldName,
                            FieldCode,
                            Placeholder,
                            Description,
                            IsRequired,
                            IsActive,
                            DataTypeId,
                            CreatedBy,
                            CreatedAt
                        )
                        VALUES
                        (
                            @FormId,
                            @FieldName,
                            @FieldCode,
                            @Placeholder,
                            @Description,
                            @IsRequired,
                            1,
                            @DataTypeId,
                            @CreatedBy,
                            NOW()
                        );

                        SELECT LAST_INSERT_ID();
                        ",
                        new
                        {
                            FormId = formId,
                            field.FieldName,
                            FieldCode = field.FieldName
                                .Replace(" ", "")
                                .ToLower(),
                            field.Placeholder,
                            field.Description,
                            field.IsRequired,
                            field.DataTypeId,
                            CreatedBy = model.ModifiedBy
                        },
                        tx);
                }

                // DROPDOWN UPDATE
                if (field.DropdownOptions != null &&
                    field.DropdownOptions.Any())
                {
                    await connection.ExecuteAsync(
                        @"
                        DELETE FROM formdropdownoptions
                        WHERE FormFieldId = @FormFieldId
                        ",
                        new
                        {
                            FormFieldId = formFieldId
                        },
                        tx);

                    foreach (var option in field.DropdownOptions)
                    {
                        await connection.ExecuteAsync(
                            @"
                            INSERT INTO formdropdownoptions
                            (
                                FormFieldId,
                                FormId,
                                OptionValue,
                                OptionLabel,
                                IsActive,
                                CreatedDate,
                                CreatedBy
                            )
                            VALUES
                            (
                                @FormFieldId,
                                @FormId,
                                @OptionValue,
                                @OptionLabel,
                                @IsActive,
                                NOW(),
                                @CreatedBy
                            )
                            ",
                            new
                            {
                                FormFieldId = formFieldId,
                                FormId = formId,
                                option.OptionValue,
                                option.OptionLabel,
                                option.IsActive,
                                CreatedBy = model.ModifiedBy
                            },
                            tx);
                    }
                }
            }
        }

        tx.Commit();

        return true;
    }
    catch
    {
        tx.Rollback();
        throw;
    }
}
        public async Task<bool> SubmitFormAsync(FormSubmitPost model)
        {
            using var connection = _db.GetConnection();

            connection.Open();

            using var tx = connection.BeginTransaction();

            try
            {
                // Save Field Values
                if (model.FormFieldValues != null &&
                    model.FormFieldValues.Any())
                {
                    foreach (var field in model.FormFieldValues)
                    {
                        await connection.ExecuteAsync(
                            @"
                    INSERT INTO formfieldvalues
                    (
                        FormId,
                        FieldCode,
                        FormFieldId,
                        FieldValue,
                        CreatedDate,
                        CreatedAt
                    )
                    SELECT
                        @FormId,
                        ff.FieldCode,
                        @FormFieldId,
                        @FieldValue,
                        NOW(),
                        NOW()
                    FROM formfields ff
                    WHERE ff.FormFieldId = @FormFieldId
                    ",
                            new
                            {
                                model.FormId,
                                field.FormFieldId,
                                field.FieldValue
                            },
                            tx);
                    }
                }

                // Save Dropdown Values
                if (model.FormDropdownOptions != null &&
                    model.FormDropdownOptions.Any())
                {
                    foreach (var dropdown in model.FormDropdownOptions)
                    {
                        await connection.ExecuteAsync(
                            @"
                    INSERT INTO formdropdownoptions
                    (
                        FormFieldId,
                        FormId,
                        OptionValue,
                        OptionLabel,
                        IsActive,
                        CreatedDate
                    )
                    VALUES
                    (
                        @FormFieldId,
                        @FormId,
                        @OptionValue,
                        @OptionValue,
                        1,
                        NOW()
                    )
                    ",
                            new
                            {
                                dropdown.FormFieldId,
                                model.FormId,
                                dropdown.OptionValue
                            },
                            tx);
                    }
                }

                tx.Commit();
                return true;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }
    }
}
