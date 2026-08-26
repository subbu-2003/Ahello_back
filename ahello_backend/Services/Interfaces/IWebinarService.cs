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

        Task<bool> ApproveWebinarAsync(
            int webinarId,
            int adminId);

        Task<bool> RejectWebinarAsync(
            int webinarId,
            int adminId,
            string reason);

        Task<bool> UploadVideoAsync(
            int webinarId,
            string videoUrl);

        Task<bool> ApproveVideoAsync(
            int webinarId,
            int adminId);

        Task<bool> RejectVideoAsync(
            int webinarId,
            int adminId,
            string reason);

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
