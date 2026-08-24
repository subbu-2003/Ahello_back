using ahello_backend.Models.DigitalBook;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;

namespace ahello_backend.Services.Classes
{
    public class DigitalBookService : IDigitalBookService
    {
        private readonly IDigitalBookRepository _repository;

        public DigitalBookService(
            IDigitalBookRepository repository)
        {
            _repository = repository;
        }

        // CREATE
        public async Task<int> CreateAsync(
            DigitalBook digitalBook)
        {
            return await _repository.CreateAsync(digitalBook);
        }

        // UPDATE
        public async Task<bool> UpdateAsync(
            DigitalBook digitalBook)
        {
            return await _repository.UpdateAsync(digitalBook);
        }

        // GET ALL
        public async Task<IEnumerable<DigitalBook>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        // GET BY USER ID
        public async Task<IEnumerable<DigitalBook>> GetByUserIdAsync(
            int userId)
        {
            return await _repository.GetByUserIdAsync(userId);
        }

        // DEACTIVATE
        public async Task<bool> DeactivateAsync(
            int digitalBookId,
            int modifiedBy)
        {
            return await _repository.DeactivateAsync(
                digitalBookId,
                modifiedBy);
        }

        // GET BY USER SLUG
        public async Task<IEnumerable<DigitalBook>> GetBySlugAsync(
      string slug)
        {
            return await _repository.GetBySlugAsync(slug);
        }
        public async Task<IEnumerable<DigitalBook>> GetPendingAsync()
        {
            return await _repository.GetPendingAsync();
        }

        public async Task<bool> UpdateApprovalStatusAsync(
         int digitalBookId,
         int adminId,
         DigitalBookApprovalStatus approvalStatus,
         string? rejectionReason)
        {
            return await _repository.UpdateApprovalStatusAsync(
                digitalBookId,
                adminId,
                approvalStatus,
                rejectionReason);
        }

        public async Task<bool> PublishAsync(
            int digitalBookId,
            int userId)
        {
            return await _repository.PublishAsync(
                digitalBookId,
                userId);
        }
    }
}