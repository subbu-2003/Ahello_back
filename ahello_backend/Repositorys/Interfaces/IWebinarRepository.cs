using ahello_backend.Models.Webinar;

namespace ahello_backend.Repositorys.Interfaces
{
    public interface IWebinarRepository
    {
        Task<int> CreateAsync(Webinar webinar);

        Task<Webinar?> GetByIdAsync(int webinarId);

        Task<IEnumerable<Webinar>> GetAllAsync();

        Task<bool> UpdateAsync(
            int webinarId,
            UpdateWebinarRequest request);

        Task<bool> DeleteAsync(int webinarId);

        Task<bool> PublishAsync(int webinarId);

        Task<int> RegisterAsync(
            WebinarRegistration registration);

        Task<WebinarRegistration?> GetRegistrationAsync(
            int webinarId,
            int userId);

        Task<IEnumerable<WebinarRegistration>>
            GetRegistrationsAsync(int webinarId);
    }
}
