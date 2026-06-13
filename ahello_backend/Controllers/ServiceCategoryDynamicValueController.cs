using ahello_backend.Models.Servicecategory;
using ahello_backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ahello_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceCategoryDynamicValueController
        : ControllerBase
    {
        private readonly
            IServiceCategoryDynamicValueService _service;

        public ServiceCategoryDynamicValueController(
            IServiceCategoryDynamicValueService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _service.GetAllAsync();

                return Ok(result);
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _service.GetByIdAsync(id);

                if (result == null)
                {
                    return NotFound(new
                    {
                        Success = false,
                        Message = "Data not found"
                    });
                }

                return Ok(result);
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
            [FromBody] ServiceCategoryDynamicPost model)
        {
            try
            {
                var id = await _service.CreateAsync(model);

                return Ok(new
                {
                    Success = true,
                    Message = "Created Successfully",
                    ServiceCategoryId = id
                });
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;

                // Foreign Key Error
                if (ex.Message.Contains("FOREIGN KEY"))
                {
                    errorMessage =
                        "Invalid ServiceCategoryFieldId. Related data does not exist.";
                }

                // Duplicate Error
                else if (ex.Message.Contains("Duplicate"))
                {
                    errorMessage =
                        "Duplicate data already exists.";
                }

                // Null Error
                else if (ex.Message.Contains("cannot be null"))
                {
                    errorMessage =
                        "Required fields are missing.";
                }

                return StatusCode(500, new
                {
                    Success = false,
                    Message = errorMessage
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] ServiceCategoryDynamicPut model)
        {
            try
            {
                var result = await _service.UpdateAsync(
                    id,
                    model);

                if (!result)
                {
                    return NotFound(new
                    {
                        Success = false,
                        Message = "Data not found"
                    });
                }

                return Ok(new
                {
                    Success = true,
                    Message = "Updated Successfully"
                });
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;

                // Foreign Key Error
                if (ex.Message.Contains("FOREIGN KEY"))
                {
                    errorMessage =
                        "Invalid ServiceCategoryFieldId. Related data does not exist.";
                }

                // Duplicate Error
                else if (ex.Message.Contains("Duplicate"))
                {
                    errorMessage =
                        "Duplicate data already exists.";
                }

                // Null Error
                else if (ex.Message.Contains("cannot be null"))
                {
                    errorMessage =
                        "Required fields are missing.";
                }

                return StatusCode(500, new
                {
                    Success = false,
                    Message = errorMessage
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _service.DeleteAsync(id);

                if (!result)
                {
                    return NotFound(new
                    {
                        Success = false,
                        Message = "Data not found"
                    });
                }

                return Ok(new
                {
                    Success = true,
                    Message = "Deleted Successfully"
                });
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;

                // Foreign Key Error
                if (ex.Message.Contains("FOREIGN KEY"))
                {
                    errorMessage =
                        "This data is already in use.";
                }

                return StatusCode(500, new
                {
                    Success = false,
                    Message = errorMessage
                });
            }
        }
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(
    int id,
    [FromBody] ServiceCategoryStatusUpdate model)
        {
            try
            {
                var result = await _service.UpdateStatusAsync(
                    id,
                    model);

                if (!result)
                {
                    return NotFound(new
                    {
                        Success = false,
                        Message = "Service Category not found"
                    });
                }

                return Ok(new
                {
                    Success = true,
                    Message = "Status updated successfully"
                });
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
    }
}
