using ahello_backend.Models.Bookings;
using ahello_backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ahello_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _service;

        public BookingController(IBookingService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create(BookingPost model)
        {
            try
            {
                var id = await _service.CreateAsync(model);

                return Ok(id);
            }
            catch
            {
                return BadRequest(new
                {
                    Message = "Invalid data"
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _service.GetAllAsync();

                return Ok(result);
            }
            catch
            {
                return BadRequest(new
                {
                    Message = "Something went wrong"
                });
            }
        }

        [HttpGet("{bookingId}")]
        public async Task<IActionResult> GetById(int bookingId)
        {
            try
            {
                var result =
                    await _service.GetByIdAsync(bookingId);

                if (result == null)
                {
                    return NotFound(new
                    {
                        Message = "Booking not found"
                    });
                }

                return Ok(result);
            }
            catch
            {
                return BadRequest(new
                {
                    Message = "Something went wrong"
                });
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update(BookingPut model)
        {
            try
            {
                var result = await _service.UpdateAsync(model);

                return Ok(result);
            }
            catch
            {
                return BadRequest(new
                {
                    Message = "Invalid data"
                });
            }
        }

        [HttpDelete("{bookingId}")]
        public async Task<IActionResult> Delete(int bookingId)
        {
            try
            {
                var result =
                    await _service.DeleteAsync(bookingId);

                return Ok(result);
            }
            catch
            {
                return BadRequest(new
                {
                    Message = "Something went wrong"
                });
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserId(
            int userId,
            int pageNumber = 1,
            int pageSize = 10,
            string? search = null)
        {
            try
            {
                var result =
                    await _service.GetByUserIdAsync(
                        userId,
                        pageNumber,
                        pageSize,
                        search);

                return Ok(result);
            }
            catch
            {
                return BadRequest(new
                {
                    Message = "Something went wrong"
                });
            }
        }

        [HttpGet("client/{clientId}")]
        public async Task<IActionResult> GetByClientId(
            int clientId,
            int pageNumber = 1,
            int pageSize = 10,
            string? search = null)
        {
            try
            {
                var result =
                    await _service.GetByClientIdAsync(
                        clientId,
                        pageNumber,
                        pageSize,
                        search);

                return Ok(result);
            }
            catch
            {
                return BadRequest(new
                {
                    Message = "Something went wrong"
                });
            }
        }

        [HttpGet("service/{serviceId}")]
        public async Task<IActionResult> GetBookingModal(
            int serviceId)
        {
            try
            {
                var result =
                    await _service.GetBookingModal(
                        serviceId);

                if (result == null)
                {
                    return NotFound(new
                    {
                        Message = "Service not found"
                    });
                }

                return Ok(result);
            }
            catch
            {
                return BadRequest(new
                {
                    Message = "Something went wrong"
                });
            }
        }
    }
}