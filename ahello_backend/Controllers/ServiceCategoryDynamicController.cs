using ahello_backend.Models.Service;
using ahello_backend.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ahello_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceCategoryDynamicController : ControllerBase
    {
        private readonly IServiceCategoryDynamicService _service;

        public ServiceCategoryDynamicController(
            IServiceCategoryDynamicService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Add(ServiceCategoryDynamic model)
        {
            var result = await _service.AddAsync(model);

            return Ok(new
            {
                Message = "Created Successfully",
                Id = result
            });
        }

        [HttpPut]
        public async Task<IActionResult> Update(ServiceCategoryDynamic model)
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
        public async Task<IActionResult> GetAll(
            int pageNumber = 1,
            int pageSize = 10)
        {
            var result = await _service.GetAllAsync(pageNumber, pageSize);

            return Ok(result);
        }
    }
}
