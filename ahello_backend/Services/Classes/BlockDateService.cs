using ahello_backend.Models.Blockdate;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;

namespace ahello_backend.Services.Classes
{
    public class BlockDateService : IBlockDateService
    {
        private readonly IBlockDateRepository _repository;

        public BlockDateService(
            IBlockDateRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<BlockDateGet>> GetAll()
        {
            return await _repository.GetAll();
        }

        public async Task<BlockDateGet> GetById(int id)
        {
            return await _repository.GetById(id);
        }

        public async Task<IEnumerable<BlockDateGet>> GetByUserId(int userId)
        {
            return await _repository.GetByUserId(userId);
        }

        public async Task<IEnumerable<BlockDateGet>> GetByServiceId(int serviceId)
        {
            return await _repository.GetByServiceId(serviceId);
        }

        public async Task<int> Post(BlockDatePost model)
        {
            return await _repository.Post(model);
        }

        public async Task<int> Update(BlockDatePut model)
        {
            return await _repository.Update(model);
        }

        public async Task<int> Delete(int id)
        {
            return await _repository.Delete(id);
        }
    }
}