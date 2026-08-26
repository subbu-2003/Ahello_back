using ahello_backend.Models.Webinar;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;

namespace ahello_backend.Services.Classes
{
    public class WebinarService : IWebinarService
    {
        private readonly IWebinarRepository _repository;

        public WebinarService(
            IWebinarRepository repository)
        {
            _repository = repository;
        }


        public async Task<int> CreateAsync(
            CreateWebinarRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
                throw new ArgumentException("Webinar title is required.");

            if (request.WebinarType != "Live" &&
                request.WebinarType != "Recorded")
            {
                throw new ArgumentException(
                    "WebinarType must be Live or Recorded.");
            }

            if (request.WebinarType == "Recorded" &&
                string.IsNullOrWhiteSpace(request.VideoUrl))
            {
                throw new ArgumentException(
                    "Video is required for Recorded webinar.");
            }

            var webinar = new Webinar
            {
                UserId = request.UserId,
                Title = request.Title,
                Description = request.Description,
                WebinarType = request.WebinarType,
                ScheduleDate = request.ScheduleDate,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                DurationMinutes = request.DurationMinutes,
                MaxParticipants = request.MaxParticipants,
                RegistrationFee = request.RegistrationFee,
                VideoUrl = request.VideoUrl,
                CreatedBy = request.CreatedBy
            };

            return await _repository.CreateAsync(webinar);
        }


        public async Task<Webinar?> GetByIdAsync(
            int webinarId)
        {
            return await _repository.GetByIdAsync(webinarId);
        }


        public async Task<IEnumerable<Webinar>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }


        public async Task<bool> UpdateAsync(
            int webinarId,
            UpdateWebinarRequest request)
        {
            return await _repository.UpdateAsync(
                webinarId,
                request);
        }


        public async Task<bool> DeleteAsync(
            int webinarId)
        {
            return await _repository.DeleteAsync(webinarId);
        }


        public async Task<bool> ApproveWebinarAsync(
            int webinarId,
            int adminId)
        {
            var webinar =
                await _repository.GetByIdAsync(webinarId);

            if (webinar == null)
                throw new KeyNotFoundException(
                    "Webinar not found.");

            return await _repository.ApproveWebinarAsync(
                webinarId,
                adminId);
        }


        public async Task<bool> RejectWebinarAsync(
            int webinarId,
            int adminId,
            string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException(
                    "Rejection reason is required.");

            return await _repository.RejectWebinarAsync(
                webinarId,
                adminId,
                reason);
        }


        public async Task<bool> UploadVideoAsync(
            int webinarId,
            string videoUrl)
        {
            var webinar =
                await _repository.GetByIdAsync(webinarId);

            if (webinar == null)
                throw new KeyNotFoundException(
                    "Webinar not found.");

            if (webinar.WebinarType != "Recorded")
                throw new ArgumentException(
                    "Video can only be uploaded for Recorded webinar.");

            return await _repository.UploadVideoAsync(
                webinarId,
                videoUrl);
        }


        public async Task<bool> ApproveVideoAsync(
            int webinarId,
            int adminId)
        {
            var webinar =
                await _repository.GetByIdAsync(webinarId);

            if (webinar == null)
                throw new KeyNotFoundException(
                    "Webinar not found.");

            if (webinar.WebinarType != "Recorded")
                throw new ArgumentException(
                    "Video approval is only applicable for Recorded webinar.");

            if (string.IsNullOrWhiteSpace(webinar.VideoUrl))
                throw new ArgumentException(
                    "No video uploaded.");

            return await _repository.ApproveVideoAsync(
                webinarId,
                adminId);
        }


        public async Task<bool> RejectVideoAsync(
            int webinarId,
            int adminId,
            string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException(
                    "Video rejection reason is required.");

            return await _repository.RejectVideoAsync(
                webinarId,
                adminId,
                reason);
        }


        public async Task<bool> PublishAsync(
            int webinarId)
        {
            var webinar =
                await _repository.GetByIdAsync(webinarId);

            if (webinar == null)
                throw new KeyNotFoundException(
                    "Webinar not found.");

            if (webinar.ApprovalStatus != "Approved")
                throw new InvalidOperationException(
                    "Webinar must be approved before publishing.");

            if (webinar.WebinarType == "Recorded" &&
                webinar.VideoApprovalStatus != "Approved")
            {
                throw new InvalidOperationException(
                    "Recorded webinar video must be approved before publishing.");
            }

            return await _repository.PublishAsync(
                webinarId);
        }


        public async Task<int> RegisterAsync(
            RegisterWebinarRequest request)
        {
            var webinar =
                await _repository.GetByIdAsync(
                    request.WebinarId);

            if (webinar == null)
                throw new KeyNotFoundException(
                    "Webinar not found.");

            if (webinar.Status != "Published")
                throw new InvalidOperationException(
                    "Webinar is not available for registration.");

            var existing =
                await _repository.GetRegistrationAsync(
                    request.WebinarId,
                    request.UserId);

            if (existing != null)
                throw new InvalidOperationException(
                    "User is already registered for this webinar.");

            var registration = new WebinarRegistration
            {
                WebinarId = request.WebinarId,
                UserId = request.UserId,
                Status = "Confirmed",
                PaymentStatus =
                    webinar.RegistrationFee > 0
                        ? "Pending"
                        : "NotRequired",
                PaymentAmount = webinar.RegistrationFee,
                CreatedBy = request.CreatedBy
            };

            return await _repository.RegisterAsync(
                registration);
        }


        public async Task<WebinarRegistration?>
            GetRegistrationAsync(
                int webinarId,
                int userId)
        {
            return await _repository.GetRegistrationAsync(
                webinarId,
                userId);
        }


        public async Task<IEnumerable<WebinarRegistration>>
            GetRegistrationsAsync(
                int webinarId)
        {
            return await _repository.GetRegistrationsAsync(
                webinarId);
        }
    }
}
