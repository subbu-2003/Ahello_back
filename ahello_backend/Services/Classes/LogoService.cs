using ahello_backend.Models.Logos;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;

namespace ahello_backend.Services.Classes
{
    public class LogoService : ILogoService
    {
        private readonly ILogoRepository _repository;

        public LogoService(ILogoRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<LogoGetResponse>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }
        public async Task<IEnumerable<LogoGetResponse>> GetByIsActiveAsync()
        {
            return await _repository.GetByIsActiveAsync();
        }

        public async Task<int> CreateAsync(LogoPost model)
        {
            return await _repository.CreateAsync(model);
        }

        public async Task<bool> UpdateAsync(
            int id,
            LogoPut model)
        {
            return await _repository.UpdateAsync(id, model);
        }
        public async Task<bool> UpdateLogoIsActiveAsync( int id, LogoIsActivePut model)
        {
            return await _repository.UpdateLogoIsActiveAsync(
                id,
                model);
        }
    }
}
