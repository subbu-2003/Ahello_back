namespace ahello_backend.Repositorys.Interfaces
{
    public interface IEscrowPaymentLogRepository
    {
        Task InsertAsync(
          int? escrowPaymentId,
          int? bookingId,
          string action,
          string status,
          string? requestJson = null,
          string? responseJson = null,
          string? errorMessage = null,
          string? createdBy = null);
    }
}

