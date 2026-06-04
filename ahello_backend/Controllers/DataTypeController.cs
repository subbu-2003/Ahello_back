using ahello_backend.Models.DataTypes;
using ahello_backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ahello_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataTypeController : ControllerBase
    {
        private readonly IDataTypeService _service;

        public DataTypeController(IDataTypeService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateDataType model)
        {
            var id = await _service.CreateAsync(model);

            return Ok(new
            {
                DataTypeId = id
            });
        }

        [HttpPut]
        public async Task<IActionResult> Update(UpdateDataType model)
        {
            bool success = await _service.UpdateAsync(model);

            if (!success)
                return BadRequest("Update failed");

            return Ok("Updated successfully");
        }

        [HttpDelete("{id}/{userId}")]
        public async Task<IActionResult> Delete(int id, int userId)
        {
            bool success = await _service.DeleteAsync(id, userId);

            if (!success)
                return BadRequest("Delete failed");

            return Ok("Deleted successfully");
        }
    }
}