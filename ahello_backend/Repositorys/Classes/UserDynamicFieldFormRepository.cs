using ahello_backend.DbContexts;
using ahello_backend.Models.Users;
using ahello_backend.Repositorys.Interfaces;
using Dapper;

namespace ahello_backend.Repositorys.Classes
{
    public class UserDynamicFieldFormRepository
        : IUserDynamicFieldFormRepository
    {
        private readonly DbContext _db;

        public UserDynamicFieldFormRepository(DbContext db)
        {
            _db = db;
        }

        public async Task<UserDynamicFieldFormResponse>
            GetUserDynamicFieldFormAsync(int userId)
        {
            using var connection = _db.GetConnection();

            // 🔹 1️⃣ DYNAMIC FIELDS
            var fields = await connection.QueryAsync<UserDynamicField>(
                @"SELECT
                    UserFieldId,
                    FieldName,
                    FieldCode,
                    Placeholder,
                    DataTypeId,
                    IsRequired
                  FROM userfields
                  WHERE UserId = @UserId
                    AND IsActive = 1",
                new { UserId = userId });

            var fieldList = fields.ToList();

            // 🔹 2️⃣ STATIC FIELDS
            var basicFields = new List<UserBasicField>
            {
                new()
                {
                    FieldCode = "CategoryId",
                    Label = "Category",
                    DataType = "dropdown",
                    IsRequired = true
                },

                new()
                {
                    FieldCode = "FullName",
                    Label = "Full Name",
                    DataType = "text",
                    IsRequired = true
                },

                new()
                {
                    FieldCode = "Email",
                    Label = "Email",
                    DataType = "email",
                    IsRequired = true
                },
                new()
                {
                    FieldCode = "ProfileUrl",
                    Label = "Profile",
                    DataType = "file",
                    IsRequired = false
                },
                new()
                {
                    FieldCode = "MobileNumber",
                    Label = "Mobile Number",
                    DataType = "text",
                    IsRequired = true
                },

                new()
                {
                    FieldCode = "WhatsAppNumber",
                    Label = "WhatsApp Number",
                    DataType = "text",
                    IsRequired = false
                },

                new()
                {
                    FieldCode = "Gender",
                    Label = "Gender",
                    DataType = "dropdown",
                    IsRequired = true
                },

                new()
                {
                    FieldCode = "DateOfBirth",
                    Label = "Date Of Birth",
                    DataType = "date",
                    IsRequired = false
                },

                new()
                {
                    FieldCode = "Address",
                    Label = "Address",
                    DataType = "textarea",
                    IsRequired = false
                },

                new()
                {
                    FieldCode = "City",
                    Label = "City",
                    DataType = "text",
                    IsRequired = false
                },

                new()
                {
                    FieldCode = "State",
                    Label = "State",
                    DataType = "text",
                    IsRequired = false
                },

                new()
                {
                    FieldCode = "Country",
                    Label = "Country",
                    DataType = "text",
                    IsRequired = false
                },

                new()
                {
                    FieldCode = "Pincode",
                    Label = "Pincode",
                    DataType = "text",
                    IsRequired = false
                },

                new()
                {
                    FieldCode = "Qualification",
                    Label = "Qualification",
                    DataType = "text",
                    IsRequired = false
                },

                new()
                {
                    FieldCode = "Occupation",
                    Label = "Occupation",
                    DataType = "text",
                    IsRequired = false
                },

                new()
                {
                    FieldCode = "CompanyName",
                    Label = "Company Name",
                    DataType = "text",
                    IsRequired = false
                },

                new()
                {
                    FieldCode = "Experience",
                    Label = "Experience",
                    DataType = "text",
                    IsRequired = false
                },

                new()
                {
                    FieldCode = "SocialMediaLinks",
                    Label = "Social Media Links",
                    DataType = "textarea",
                    IsRequired = false
                },

                new()
                {
                    FieldCode = "WebsiteURL",
                    Label = "Website URL",
                    DataType = "text",
                    IsRequired = false
                },

                new()
                {
                    FieldCode = "Notes",
                    Label = "Notes",
                    DataType = "textarea",
                    IsRequired = false
                }
            };

            // 🔹 3️⃣ CATEGORY DROPDOWN
            var categories =
                await connection.QueryAsync<UserDropdownOption>(
                @"SELECT
                    CategoryName AS Label,
                    CAST(CategoryId AS CHAR) AS Value
                  FROM categories",
                new { UserId = userId });

            var categoryField =
                basicFields.First(f => f.FieldCode == "CategoryId");

            categoryField.Options = categories.ToList();

            // 🔹 4️⃣ GENDER DROPDOWN
            var genderField =
                basicFields.First(f => f.FieldCode == "Gender");

            genderField.Options = new List<UserDropdownOption>
            {
                new()
                {
                    Label = "Male",
                    Value = "Male"
                },
                new()
                {
                    Label = "Female",
                    Value = "Female"
                },
                new()
                {
                    Label = "Other",
                    Value = "Other"
                }
            };

            // 🔹 5️⃣ FINAL RESPONSE
            return new UserDynamicFieldFormResponse
            {
                BasicFields = basicFields,
                DynamicFields = fieldList
            };
        }
    }
}