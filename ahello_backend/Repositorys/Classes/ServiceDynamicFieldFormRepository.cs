using ahello_backend.DbContexts;
using ahello_backend.Models.Service;
using ahello_backend.Repositorys.Interfaces;
using Dapper;

namespace ahello_backend.Repositorys.Classes
{
    public class ServiceDynamicFieldFormRepository
        : IServiceDynamicFieldFormRepository
    {
        private readonly DbContext _db;

        public ServiceDynamicFieldFormRepository(DbContext db)
        {
            _db = db;
        }

        public async Task<ServiceDynamicFieldFormResponse>
            GetServiceDynamicFieldFormAsync(int serviceId)
        {
            using var connection = _db.GetConnection();

            // 🔹 1️⃣ DYNAMIC FIELDS
            var fields = await connection.QueryAsync<ServiceDynamicField>(
                @"SELECT
                    ServiceFieldId,
                    FieldName,
                    FieldCode,
                    Placeholder,
                    DataTypeId,
                    IsRequired
                  FROM servicefields
                  WHERE ServiceId = @ServiceId
                    AND IsActive = 1",
                new { ServiceId = serviceId });

            var fieldList = fields.ToList();

            // 🔹 2️⃣ STATIC FIELDS
            var basicFields = new List<ServiceBasicField>
            {
                 new()
                    {
                        FieldCode = "UserId",
                        Label = "User",
                        DataType = "dropdown",
                        IsRequired = true
                    },

                new()
                {
                    FieldCode = "ServiceTypeId",
                    Label = "Service Type",
                    DataType = "dropdown",
                    IsRequired = true
                },

                new()
                {
                    FieldCode = "ServiceTitle",
                    Label = "Service Title",
                    DataType = "text",
                    IsRequired = true
                },
                new()
                {
                    FieldCode = "Price",
                    Label = "Price",
                    DataType = "number",
                    IsRequired = true
                },

                new()
                {
                    FieldCode = "Duration",
                    Label = "Duration",
                    DataType = "text",
                    IsRequired = true
                },
                new()
                {
                    FieldCode = "ShortDescription",
                    Label = "Short Description",
                    DataType = "textarea",
                    IsRequired = false
                },

                new()
                {
                    FieldCode = "FullDescription",
                    Label = "Full Description",
                    DataType = "textarea",
                    IsRequired = false
                },

                new()
                {
                    FieldCode = "Tags",
                    Label = "Tags",
                    DataType = "text",
                    IsRequired = false
                },

                new()
                {
                    FieldCode = "Language",
                    Label = "Language",
                    DataType = "text",
                    IsRequired = false
                },

                new()
                {
                    FieldCode = "ThumbnailImage",
                    Label = "Thumbnail Image",
                    DataType = "text",
                    IsRequired = false
                },

                new()
                {
                    FieldCode = "BannerImage",
                    Label = "Banner Image",
                    DataType = "text",
                    IsRequired = false
                },

                new()
                {
                    FieldCode = "IntroVideo",
                    Label = "Intro Video",
                    DataType = "text",
                    IsRequired = false
                },

                new()
                {
                    FieldCode = "Status",
                    Label = "Status",
                    DataType = "dropdown",
                    IsRequired = true
                }
            };
            // 🔹 3️⃣ USER DROPDOWN
            var users =
                await connection.QueryAsync<ServiceDropdownOption>(
                @"SELECT
                    FullName AS Label,
                    CAST(UserId AS CHAR) AS Value
                  FROM users");

            var userField =
                basicFields.First(f => f.FieldCode == "UserId");

            userField.Options = users.ToList();

            // 🔹 3️⃣ SERVICE TYPE DROPDOWN
            var serviceTypes =
                await connection.QueryAsync<ServiceDropdownOption>(
                @"SELECT
                    ServiceTypeName AS Label,
                    CAST(ServiceTypeId AS CHAR) AS Value
                  FROM servicetypes");

            var serviceTypeField =
                basicFields.First(f => f.FieldCode == "ServiceTypeId");

            serviceTypeField.Options = serviceTypes.ToList();

            // 🔹 4️⃣ STATUS DROPDOWN
            var statusField =
                basicFields.First(f => f.FieldCode == "Status");

            statusField.Options = new List<ServiceDropdownOption>
            {
                new()
                {
                    Label = "Draft",
                    Value = "Draft"
                },

                new()
                {
                    Label = "Published",
                    Value = "Published"
                }
            };

            // 🔹 5️⃣ FINAL RESPONSE
            return new ServiceDynamicFieldFormResponse
            {
                BasicFields = basicFields,
                DynamicFields = fieldList
            };
        }
    }
}