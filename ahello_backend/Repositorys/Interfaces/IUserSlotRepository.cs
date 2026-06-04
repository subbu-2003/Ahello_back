using ahello_backend.Models.UserSlots;

namespace ahello_backend.Repositorys.Interfaces
{
    public interface IUserSlotRepository
    {
        Task<IEnumerable<UserSlotGet>> GetAll();

        Task<IEnumerable<UserSlotGet>> GetByUserAndService(
            int userId,
            int serviceId
        );

        Task<IEnumerable<UserSlotGet>> GetAvailableSlots(
            int userId,
            int serviceId,
            DateTime date
        );

        Task<UserSlotGet> GetById(int slotId);

        Task<int> Post(UserSlotPost model);

        Task<int> PostBulk(IEnumerable<UserSlotPost> slots);

        Task<int> Put(UserSlotPut model);

        Task<int> MarkAsBooked(int slotId);

        Task<int> MarkAsUnbooked(int slotId);

        Task<int> Delete(int slotId);
    }
}