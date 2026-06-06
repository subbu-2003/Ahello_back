using ahello_backend.DbContexts;
using ahello_backend.Models.Form;
using ahello_backend.Models.Pagination;
using ahello_backend.Repositorys.Interfaces;
using Dapper;
using System.Linq;
using System.Text.RegularExpressions;

namespace ahello_backend.Repositorys.Classes
{
    public class FormFieldValueRepository : IFormFieldValueRepository
    {
        private readonly DbContext _db;

        public FormFieldValueRepository(DbContext db)
        {
            _db = db;
        }

        public async Task<int> CreateAsync(CreateFormFieldValue model)
        {
            var validationErrors =
                await ValidateFieldValueAsync(
                    model.FormFieldId,
                    model.FieldValue);

            if (validationErrors.Any())
            {
                throw new ApplicationException(
                    string.Join(" | ", validationErrors));
            }

            var sql = @"INSERT INTO formfieldvalues
                (
                    FormId,
                    FieldCode,
                    FormFieldId,
                    FieldValue,
                    CreatedDate,
                    CreatedBy,
                    CreatedAt
                )
                VALUES
                (
                    @FormId,
                    @FieldCode,
                    @FormFieldId,
                    @FieldValue,
                    NOW(),
                    @CreatedBy,
                    CURRENT_TIMESTAMP
                );

                SELECT LAST_INSERT_ID();";

            using var connection = _db.GetConnection();

            return await connection.ExecuteScalarAsync<int>(sql, model);
        }

        public async Task<bool> UpdateAsync(UpdateFormFieldValue model)
        {
            using var connection = _db.GetConnection();

            var formFieldId = await connection.QueryFirstOrDefaultAsync<int>(
     @"SELECT FormFieldId
      FROM formfieldvalues
      WHERE FormFieldValueId = @FormFieldValueId",
     new { FormFieldValueId = model.FormFieldValueId });

            var validationErrors =
                await ValidateFieldValueAsync(
                    formFieldId,
                    model.FieldValue);

            if (validationErrors.Any())
            {
                throw new ApplicationException(
                    string.Join(" | ", validationErrors));
            }

            var sql = @"UPDATE formfieldvalues
                SET
                    FieldValue = @FieldValue,
                    ModifiedBy = @ModifiedBy,
                    ModifiedAt = CURRENT_TIMESTAMP
                WHERE FormFieldValueId = @FormFieldValueId";

            var rows = await connection.ExecuteAsync(sql, model);

            return rows > 0;
        }
        public async Task<IEnumerable<FormFieldValue>> GetByFormAsync(int formId)
        {
            var sql = @"SELECT *
                        FROM formfieldvalues
                        WHERE FormId = @FormId
                        ORDER BY FormFieldValueId DESC";

            using var connection = _db.GetConnection();

            return await connection.QueryAsync<FormFieldValue>(
                sql,
                new { FormId = formId }
            );
        }

        public async Task<FormFieldValue> GetByIdAsync(int formFieldValueId)
        {
            var sql = @"SELECT *
                        FROM formfieldvalues
                        WHERE FormFieldValueId = @FormFieldValueId";

            using var connection = _db.GetConnection();

            return await connection.QueryFirstOrDefaultAsync<FormFieldValue>(
                sql,
                new { FormFieldValueId = formFieldValueId }
            );
        }

        private async Task<List<string>> ValidateFieldValueAsync(
     int formFieldId,
     string fieldValue)
        {
            var errors = new List<string>();

            using var connection = _db.GetConnection();

            var dataTypeCode = await connection.QueryFirstOrDefaultAsync<string>(
                @"SELECT dt.DataTypeCode
          FROM formfields ff
          INNER JOIN datatypes dt
              ON ff.DataTypeId = dt.DataTypeId
          WHERE ff.FormFieldId = @FormFieldId",
                new { FormFieldId = formFieldId });

            if (string.IsNullOrEmpty(dataTypeCode))
            {
                errors.Add("Invalid field configuration.");
                return errors;
            }

            switch (dataTypeCode.ToUpper())
            {
                case "SINGLE_TEXT":

                    if (string.IsNullOrWhiteSpace(fieldValue))
                        errors.Add("Text value is required.");

                    if (!string.IsNullOrEmpty(fieldValue) &&
                        (fieldValue.Contains("\n") || fieldValue.Contains("\r")))
                        errors.Add("Single line text cannot contain multiple lines.");

                    if (!string.IsNullOrEmpty(fieldValue) &&
                        fieldValue.Length > 100)
                        errors.Add("Maximum 100 characters allowed.");

                    break;

                case "ALPHABETS":

                    if (string.IsNullOrWhiteSpace(fieldValue))
                        errors.Add("Value is required.");

                    if (!string.IsNullOrWhiteSpace(fieldValue) &&
                        !Regex.IsMatch(fieldValue, @"^[A-Za-z\s]+$"))
                        errors.Add("Only alphabets are allowed.");

                    if (!string.IsNullOrEmpty(fieldValue) &&
                        fieldValue.Length > 50)
                        errors.Add("Maximum 50 characters allowed.");

                    break;

                case "ALPHANUMERIC":

                    if (string.IsNullOrWhiteSpace(fieldValue))
                        errors.Add("Value is required.");

                    if (!string.IsNullOrWhiteSpace(fieldValue) &&
                        !Regex.IsMatch(fieldValue, @"^[A-Za-z0-9\s]+$"))
                        errors.Add("Only alphabets and numbers are allowed.");

                    break;

                case "NUMBER":

                    if (string.IsNullOrWhiteSpace(fieldValue))
                        errors.Add("Number is required.");

                    if (!string.IsNullOrWhiteSpace(fieldValue) &&
                        !decimal.TryParse(fieldValue, out _))
                        errors.Add("Please enter a valid number.");

                    break;

                case "EMAIL":

                    if (string.IsNullOrWhiteSpace(fieldValue))
                        errors.Add("Email is required.");

                    if (!string.IsNullOrWhiteSpace(fieldValue) &&
                        !Regex.IsMatch(fieldValue,
                        @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                        errors.Add("Please enter a valid email address.");

                    if (!string.IsNullOrEmpty(fieldValue) &&
                        fieldValue.Length > 100)
                        errors.Add("Email exceeds maximum length.");

                    break;

                case "PHONE":

                    if (string.IsNullOrWhiteSpace(fieldValue))
                        errors.Add("Phone number is required.");

                    if (!string.IsNullOrWhiteSpace(fieldValue) &&
                        !Regex.IsMatch(fieldValue, @"^\d+$"))
                        errors.Add("Phone number must contain digits only.");

                    if (!string.IsNullOrWhiteSpace(fieldValue) &&
                        !Regex.IsMatch(fieldValue, @"^\d{10}$"))
                        errors.Add("Phone number must contain exactly 10 digits.");

                    break;

                case "URL":

                    if (string.IsNullOrWhiteSpace(fieldValue))
                        errors.Add("URL is required.");

                    if (!string.IsNullOrWhiteSpace(fieldValue) &&
                        !Uri.TryCreate(fieldValue, UriKind.Absolute, out _))
                        errors.Add("Please enter a valid URL.");

                    break;

                case "TEXTAREA":
                case "MULTILINE_TEXT":

                    if (string.IsNullOrWhiteSpace(fieldValue))
                        errors.Add("Value is required.");

                    if (!string.IsNullOrEmpty(fieldValue) &&
                        fieldValue.Length > 5000)
                        errors.Add("Maximum 5000 characters allowed.");

                    break;

                case "PAN":

                    if (string.IsNullOrWhiteSpace(fieldValue))
                        errors.Add("PAN number is required.");

                    if (!string.IsNullOrWhiteSpace(fieldValue) &&
                        !Regex.IsMatch(fieldValue,
                        @"^[A-Z]{5}[0-9]{4}[A-Z]$"))
                        errors.Add("Please enter a valid PAN number.");

                    break;

                case "PINCODE":

                    if (string.IsNullOrWhiteSpace(fieldValue))
                        errors.Add("Pincode is required.");

                    if (!string.IsNullOrWhiteSpace(fieldValue) &&
                        !Regex.IsMatch(fieldValue, @"^\d{6}$"))
                        errors.Add("Pincode must contain exactly 6 digits.");

                    break;

                case "DROPDOWN":
                case "RADIOBUTTON":

                    if (string.IsNullOrWhiteSpace(fieldValue))
                        errors.Add("Please select a value.");

                    break;

                case "DATE":

                    if (string.IsNullOrWhiteSpace(fieldValue))
                        errors.Add("Date is required.");

                    if (!string.IsNullOrWhiteSpace(fieldValue) &&
                        !DateTime.TryParse(fieldValue, out _))
                        errors.Add("Please enter a valid date.");

                    break;

                case "PRICE":

                    if (string.IsNullOrWhiteSpace(fieldValue))
                    {
                        errors.Add("Price is required.");
                    }
                    else
                    {
                        if (!decimal.TryParse(fieldValue, out decimal price))
                        {
                            errors.Add("Please enter a valid price.");
                        }
                        else
                        {
                            if (price < 0)
                                errors.Add("Price cannot be negative.");
                        }
                    }

                    break;

                case "CHECKBOX":
                case "TOGGLE_SWITCH":

                    if (fieldValue != "true" &&
                        fieldValue != "false")
                        errors.Add("Value must be true or false.");

                    break;

                case "MULTI_SELECT":

                    if (string.IsNullOrWhiteSpace(fieldValue))
                        errors.Add("Please select at least one option.");

                    break;

                case "TIME_PICKER":

                    if (string.IsNullOrWhiteSpace(fieldValue))
                        errors.Add("Time is required.");

                    if (!string.IsNullOrWhiteSpace(fieldValue) &&
                        !TimeSpan.TryParse(fieldValue, out _))
                        errors.Add("Please select a valid time.");

                    break;

                case "DATETIME_PICKER":

                    if (string.IsNullOrWhiteSpace(fieldValue))
                        errors.Add("Date and time is required.");

                    if (!string.IsNullOrWhiteSpace(fieldValue) &&
                        !DateTime.TryParse(fieldValue, out _))
                        errors.Add("Please select a valid date and time.");

                    break;

                case "SIGNATURE_PAD":

                    if (string.IsNullOrWhiteSpace(fieldValue))
                        errors.Add("Signature is required.");

                    break;

                case "RATING_INPUT":

                    if (!int.TryParse(fieldValue, out var rating))
                        errors.Add("Rating must be numeric.");

                    else if (rating < 1 || rating > 5)
                        errors.Add("Rating must be between 1 and 5.");

                    break;

                case "OTP_VERIFICATION":

                    if (string.IsNullOrWhiteSpace(fieldValue))
                        errors.Add("OTP is required.");

                    if (!string.IsNullOrWhiteSpace(fieldValue) &&
                        !Regex.IsMatch(fieldValue, @"^\d{4,8}$"))
                        errors.Add("Please enter a valid OTP.");

                    break;

                case "QR_SCANNER":

                    if (string.IsNullOrWhiteSpace(fieldValue))
                        errors.Add("QR code data is required.");

                    break;

                case "LOCATION_PICKER":

                    if (string.IsNullOrWhiteSpace(fieldValue))
                        errors.Add("Location is required.");

                    break;

                case "CONSENT_CHECKBOX":

                    if (fieldValue?.ToLower() != "true")
                        errors.Add("Consent must be accepted.");

                    break;
            }

            return errors;
        }
        public async Task<PagedResult<FormFieldValueUserResponse>>
                 GetByUserIdAsync(
                     int userId,
                     int pageNumber,
                     int pageSize,
                     string? search)
                        {
                            using var connection = _db.GetConnection();

                            int skip = (pageNumber - 1) * pageSize;

                            var whereClause = @"
                    WHERE f.UserId = @UserId
                    AND f.IsActive = 1";

                            if (!string.IsNullOrWhiteSpace(search))
                            {
                                whereClause += @"
                        AND
                        (
                            f.Title LIKE @Search
                            OR f.Description LIKE @Search
                            OR ff.FieldName LIKE @Search
                            OR ff.FieldCode LIKE @Search
                            OR ffv.FieldValue LIKE @Search
                        )";
                            }

                            // TOTAL COUNT
                            var totalCount = await connection.ExecuteScalarAsync<int>(
                            $@"
                    SELECT COUNT(DISTINCT f.FormId)

                    FROM forms f

                    LEFT JOIN formfields ff
                        ON f.FormId = ff.FormId

                    LEFT JOIN formfieldvalues ffv
                        ON ff.FormFieldId = ffv.FormFieldId

                    {whereClause}",
                            new
                            {
                                UserId = userId,
                                Search = $"%{search}%"
                            });

                            // MAIN QUERY
                            var sql = $@"

                    SELECT

                        f.FormId,
                        f.UserId,
                        f.Title,
                        f.Description,
                        f.IsActive,
                        f.CreatedAt,
                        f.CreatedBy,
                        f.ModifiedAt,
                        f.ModifiedBy,

                        ff.FormFieldId,
                        ff.FormId,
                        ff.FieldName,
                        ff.FieldCode,
                        ff.Placeholder,
                        ff.Description,
                        ff.IsRequired,
                        ff.IsActive,
                        ff.DataTypeId,
                        ff.CreatedBy,
                        ff.CreatedAt,
                        ff.ModifiedBy,
                        ff.ModifiedAt,

                        ffv.FormFieldValueId,
                        ffv.FormId,
                        ffv.FieldCode,
                        ffv.FormFieldId,
                        ffv.FieldValue,
                        ffv.CreatedDate,
                        ffv.CreatedBy,
                        ffv.CreatedAt,
                        ffv.ModifiedBy,
                        ffv.ModifiedAt

                    FROM forms f

                    LEFT JOIN formfields ff
                        ON f.FormId = ff.FormId

                    LEFT JOIN formfieldvalues ffv
                        ON ff.FormFieldId = ffv.FormFieldId

                    {whereClause}

                    ORDER BY f.FormId DESC,
                             ff.FormFieldId

                    LIMIT @PageSize OFFSET @Skip";

            var forms =
                new Dictionary<int, FormFieldValueUserResponse>();

            var fields =
                new Dictionary<int, FormFieldDetailResponse>();

            await connection.QueryAsync
            <
                FormFieldValueUserResponse,
                FormFieldDetailResponse,
                FormFieldValue,
                FormFieldValueUserResponse
            >
            (
                sql,
                (form, field, value) =>
                {
                    if (!forms.TryGetValue(
                        form.FormId,
                        out var existingForm))
                    {
                        existingForm = form;

                        existingForm.Fields =
                            new List<FormFieldDetailResponse>();

                        forms.Add(
                            existingForm.FormId,
                            existingForm);
                    }

                    if (field != null)
                    {
                        if (!fields.TryGetValue(
                            field.FormFieldId,
                            out var existingField))
                        {
                            existingField = field;

                            existingField.FieldValues =
                                new List<FormFieldValue>();

                            fields.Add(
                                existingField.FormFieldId,
                                existingField);

                            existingForm.Fields
                                .Add(existingField);
                        }

                        if (value != null &&
                            value.FormFieldValueId > 0)
                        {
                            existingField.FieldValues
                                .Add(value);
                        }
                    }

                    return existingForm;
                },
                new
                {
                    UserId = userId,
                    Search = $"%{search}%",
                    PageSize = pageSize,
                    Skip = skip
                },
                splitOn: "FormFieldId,FormFieldValueId"
            );

            return new PagedResult<FormFieldValueUserResponse>
            {
                TotalCount = totalCount,
                Details = forms.Values
            };
        }
    }
}