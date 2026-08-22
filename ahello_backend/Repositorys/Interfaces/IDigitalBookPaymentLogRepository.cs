namespace ahello_backend.Repositorys.Interfaces
{
    public interface IDigitalBookPaymentLogRepository
    {
        Task InsertAsync(
            int? digitalBookPaymentId,
            int? digitalBookingId,
            string action,
            string status,
            string? requestJson = null,
            string? responseJson = null,
            string? errorMessage = null,
            string? createdBy = null);
    }
}
