namespace ahello_backend.Repositorys.Interfaces
{
    public interface IWebinarPaymentLogRepository
    {
        Task InsertAsync(
            int? webinarPaymentId,
            int? webinarRegistrationId,
            string action,
            string status,
            string? requestJson = null,
            string? responseJson = null,
            string? errorMessage = null,
            string? createdBy = null);
    }
}
