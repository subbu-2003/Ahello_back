using ahello_backend.Models.Blockdate;

namespace ahello_backend.Services.Interfaces
{
    public interface IBlockDateService
    {
        Task<IEnumerable<BlockDateGet>> GetAll();

        Task<BlockDateGet> GetById(int id);

        Task<IEnumerable<BlockDateGet>> GetByUserId(int userId);

        Task<IEnumerable<BlockDateGet>> GetByServiceId(int serviceId);

        Task<int> Post(BlockDatePost model);

        Task<int> Update(BlockDatePut model);

        Task<int> Delete(int id);
    }
}
