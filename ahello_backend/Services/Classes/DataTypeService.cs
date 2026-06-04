using ahello_backend.Models.DataTypes;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;

namespace ahello_backend.Services.Classes
{
    public class DataTypeService : IDataTypeService
    {
        private readonly IDataTypeRepository _repository;

        public DataTypeService(IDataTypeRepository repository)
        {
            _repository = repository;
        }

        public Task<IEnumerable<DataType>> GetAllAsync()
            => _repository.GetAllAsync();

        public Task<DataType> GetByIdAsync(int id)
            => _repository.GetByIdAsync(id);

        public Task<int> CreateAsync(CreateDataType model)
            => _repository.CreateAsync(model);

        public Task<bool> UpdateAsync(UpdateDataType model)
            => _repository.UpdateAsync(model);

        public Task<bool> DeleteAsync(int id, int userId)
            => _repository.DeleteAsync(id, userId);
    }
}
