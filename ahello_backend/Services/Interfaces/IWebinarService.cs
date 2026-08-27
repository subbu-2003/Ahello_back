using ahello_backend.Models.Webinar;

namespace ahello_backend.Services.Interfaces
{
    public interface IWebinarService
    {
        Task<int> CreateAsync(CreateWebinarRequest request);

        Task<Webinar?> GetByIdAsync(int webinarId);

        Task<IEnumerable<Webinar>> GetAllAsync();

        Task<bool> UpdateAsync(
            int webinarId,
            UpdateWebinarRequest request);

        Task<bool> DeleteAsync(int webinarId);

        Task<bool> PublishAsync(int webinarId);

        Task<int> RegisterAsync(
            RegisterWebinarRequest request);

        Task<WebinarRegistration?> GetRegistrationAsync(
            int webinarId,
            int userId);

        Task<IEnumerable<WebinarRegistration>>
            GetRegistrationsAsync(int webinarId);
    }
}
