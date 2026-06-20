using ahello_backend.Models.Form;
using ahello_backend.Models.Pagination;

namespace ahello_backend.Services.Interfaces
{
    public interface IFormFieldValueService
    {
        Task<int> CreateAsync(CreateFormFieldValue model);

        Task<bool> UpdateAsync(UpdateFormFieldValue model);

        Task<IEnumerable<FormFieldValue>> GetByFormAsync(int formId);

        Task<FormFieldValue> GetByIdAsync(int formFieldValueId);
        Task<PagedResult<FormFieldValueUserResponse>>
        GetByUserIdAsync(
            int userId,
            int pageNumber,
            int pageSize,
            string? search, DateTime? date);
    }
}
