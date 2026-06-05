using ahello_backend.Models.Servicetype;
using ahello_backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ahello_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceTypeController : ControllerBase
    {
        private readonly IServiceTypeService _service;

        public ServiceTypeController(
            IServiceTypeService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var data = await _service.GetAllAsync();

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("{serviceTypeId}")]
        public async Task<IActionResult> GetById(
            int serviceTypeId)
        {
            try
            {
                var data = await _service.GetByIdAsync(
                    serviceTypeId);

                if (data == null)
                {
                    return NotFound(new
                    {
                        Success = false,
                        Message = "Service Type not found"
                    });
                }

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(
             ServiceTypePost model)
        {
            try
            {
                var id = await _service.CreateAsync(model);

                return Ok(new
                {
                    Success = true,
                    ServiceTypeId = id
                });
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;

                // Duplicate Error
                if (ex.Message.Contains("Duplicate"))
                {
                    errorMessage = "Service Type already exists.";
                }

                // Foreign Key Error
                else if (ex.Message.Contains("FOREIGN KEY"))
                {
                    errorMessage = "Invalid reference data.";
                }

                return StatusCode(500, new
                {
                    Success = false,
                    Message = errorMessage
                });
            }
        }


        [HttpPut("{serviceTypeId}")]
        public async Task<IActionResult> Update(
            int serviceTypeId,
            ServiceTypePut model)
        {
            try
            {
                var updated = await _service.UpdateAsync(
                    serviceTypeId,
                    model);

                if (!updated)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Update failed"
                    });
                }

                return Ok(new
                {
                    Success = true,
                    Message = "Updated successfully"
                });
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;

                if (ex.Message.Contains("Duplicate"))
                {
                    errorMessage = "Service Type already exists.";
                }
                else if (ex.Message.Contains("FOREIGN KEY"))
                {
                    errorMessage = "Invalid reference data.";
                }

                return StatusCode(500, new
                {
                    Success = false,
                    Message = errorMessage
                });
            }
        }


        [HttpDelete("{serviceTypeId}")]
        public async Task<IActionResult> Delete(
            int serviceTypeId)
        {
            try
            {
                var deleted = await _service.DeleteAsync(
                    serviceTypeId);

                if (!deleted)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Delete failed"
                    });
                }

                return Ok(new
                {
                    Success = true,
                    Message = "Deleted successfully"
                });
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;

                // Foreign Key Error
                if (ex.Message.Contains("FOREIGN KEY"))
                {
                    errorMessage = "This Service Type is already in use.";
                }

                return StatusCode(500, new
                {
                    Success = false,
                    Message = errorMessage
                });
            }
        }
    }
}