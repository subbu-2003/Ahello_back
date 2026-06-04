using ahello_backend.Models.Service;
using ahello_backend.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ahello_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceCategoryDropdownOptionController : ControllerBase
    {
        private readonly IServiceCategoryDropdownOptionService _service;

        public ServiceCategoryDropdownOptionController(
            IServiceCategoryDropdownOptionService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Add(ServiceCategoryDropdownOption model)
        {
            var result = await _service.AddAsync(model);

            return Ok(new
            {
                Message = "Created Successfully",
                Id = result
            });
        }

        [HttpPut]
        public async Task<IActionResult> Update(ServiceCategoryDropdownOption model)
        {
            var result = await _service.UpdateAsync(model);

            return Ok(new
            {
                Message = "Updated Successfully",
                Result = result
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);

            return Ok(new
            {
                Message = "Deleted Successfully",
                Result = result
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();

            return Ok(result);
        }
    }
}
