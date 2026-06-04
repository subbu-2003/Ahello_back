using ahello_backend.Models.DataTypes;

namespace ahello_backend.Repositorys.Interfaces
{
    public interface IDataTypeRepository
    {
        Task<IEnumerable<DataType>> GetAllAsync();

        Task<DataType> GetByIdAsync(int id);

        Task<int> CreateAsync(CreateDataType model);

        Task<bool> UpdateAsync(UpdateDataType model);

        Task<bool> DeleteAsync(int id, int userId);
    }
}
