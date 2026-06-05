using ahello_backend.DbContexts;
using ahello_backend.Models.Servicecategory;
using ahello_backend.Repositorys.Interfaces;
using Dapper;


namespace ahello_backend.Repositorys.Classes
{
    public class ServiceCategoryDynamicValueRepository
        : IServiceCategoryDynamicValueRepository
    {
        private readonly DbContext _db;

        public ServiceCategoryDynamicValueRepository(DbContext db)
        {
            _db = db;
        }

        public async Task<int> CreateAsync(
            ServiceCategoryDynamicPost model)
        {
            using var connection = _db.GetConnection();

            connection.Open();

            using var tx = connection.BeginTransaction();

            try
            {
                if (model.Fields != null && model.Fields.Any())
                {
                    var sql = @"
                    INSERT INTO servicecategoryfieldvalues
                    (
                        ServiceCategoryId,
                        FieldCode,
                        ServiceCategoryFieldId,
                        FieldValue,
                        CreatedDate,
                        CreatedBy,
                        CreatedAt
                    )
                    SELECT
                        @ServiceCategoryId,
                        scf.FieldCode,
                        @ServiceCategoryFieldId,
                        @FieldValue,
                        NOW(),
                        @CreatedBy,
                        CURRENT_TIMESTAMP
                    FROM servicecategoryfields scf
                    WHERE scf.ServiceCategoryFieldId =
                          @ServiceCategoryFieldId";

                    foreach (var field in model.Fields)
                    {
                        await connection.ExecuteAsync(
                            sql,
                            new
                            {
                                model.ServiceCategoryId,
                                field.ServiceCategoryFieldId,
                                field.FieldValue,
                                model.CreatedBy
                            },
                            tx);
                    }
                }

                tx.Commit();

                return model.ServiceCategoryId;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public async Task<IEnumerable<ServiceCategoryDynamicGetResponse>>
            GetAllAsync()
        {
            using var connection = _db.GetConnection();

            var categories =
                (await connection.QueryAsync<
                    ServiceCategoryDynamicGetResponse>(
                @"SELECT
                    ServiceCategoryId,
                    ServiceCategoryName
                  FROM servicecategorydynamic"))
                .ToList();

            foreach (var category in categories)
            {
                var fields =
                    await connection.QueryAsync<
                        ServiceCategoryDynamicFieldResponse>(
                    @"SELECT
                        scf.ServiceCategoryFieldId,
                        scf.FieldName,
                        scf.FieldCode,
                        scf.Placeholder,
                        scf.IsRequired,
                        scf.DataTypeId,
                        scfv.FieldValue

                    FROM servicecategoryfieldvalues scfv

                    INNER JOIN servicecategoryfields scf
                        ON scfv.ServiceCategoryFieldId =
                           scf.ServiceCategoryFieldId

                    WHERE scfv.ServiceCategoryId =
                          @ServiceCategoryId",
                    new
                    {
                        category.ServiceCategoryId
                    });

                category.Fields = fields.ToList();
            }

            return categories;
        }

        public async Task<ServiceCategoryDynamicGetResponse>
            GetByIdAsync(int serviceCategoryId)
        {
            using var connection = _db.GetConnection();

            var category =
                await connection.QueryFirstOrDefaultAsync<
                    ServiceCategoryDynamicGetResponse>(
                @"SELECT
                    ServiceCategoryId,
                    ServiceCategoryName
                  FROM servicecategorydynamic
                  WHERE ServiceCategoryId =
                        @ServiceCategoryId",
                new
                {
                    ServiceCategoryId = serviceCategoryId
                });

            if (category == null)
                return null;

            var fields =
                await connection.QueryAsync<
                    ServiceCategoryDynamicFieldResponse>(
                @"SELECT
                    scf.ServiceCategoryFieldId,
                    scf.FieldName,
                    scf.FieldCode,
                    scf.Placeholder,
                    scf.IsRequired,
                    scf.DataTypeId,
                    scfv.FieldValue

                FROM servicecategoryfieldvalues scfv

                INNER JOIN servicecategoryfields scf
                    ON scfv.ServiceCategoryFieldId =
                       scf.ServiceCategoryFieldId

                WHERE scfv.ServiceCategoryId =
                      @ServiceCategoryId",
                new
                {
                    ServiceCategoryId = serviceCategoryId
                });

            category.Fields = fields.ToList();

            return category;
        }

        public async Task<bool> UpdateAsync(
            int serviceCategoryId,
            ServiceCategoryDynamicPut model)
        {
            using var connection = _db.GetConnection();

            connection.Open();

            using var tx = connection.BeginTransaction();

            try
            {
                await connection.ExecuteAsync(
                    @"DELETE FROM servicecategoryfieldvalues
                      WHERE ServiceCategoryId =
                            @ServiceCategoryId",
                    new
                    {
                        ServiceCategoryId = serviceCategoryId
                    },
                    tx);

                if (model.Fields != null && model.Fields.Any())
                {
                    var sql = @"
                    INSERT INTO servicecategoryfieldvalues
                    (
                        ServiceCategoryId,
                        FieldCode,
                        ServiceCategoryFieldId,
                        FieldValue,
                        CreatedDate,
                        CreatedBy,
                        CreatedAt
                    )
                    SELECT
                        @ServiceCategoryId,
                        scf.FieldCode,
                        @ServiceCategoryFieldId,
                        @FieldValue,
                        NOW(),
                        @ModifiedBy,
                        CURRENT_TIMESTAMP
                    FROM servicecategoryfields scf
                    WHERE scf.ServiceCategoryFieldId =
                          @ServiceCategoryFieldId";

                    foreach (var field in model.Fields)
                    {
                        await connection.ExecuteAsync(
                            sql,
                            new
                            {
                                ServiceCategoryId =
                                    serviceCategoryId,

                                field.ServiceCategoryFieldId,

                                field.FieldValue,

                                model.ModifiedBy
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

        public async Task<bool> DeleteAsync(
            int serviceCategoryId)
        {
            using var connection = _db.GetConnection();

            var rows = await connection.ExecuteAsync(
                @"DELETE FROM servicecategoryfieldvalues
                  WHERE ServiceCategoryId =
                        @ServiceCategoryId",
                new
                {
                    ServiceCategoryId = serviceCategoryId
                });

            return rows > 0;
        }
    }
}
