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
        public async Task<IActionResult> Create(
            BookingPost model)
        {
            var id = await _service.CreateAsync(model);

            return Ok(id);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();

            return Ok(result);
        }

        [HttpGet("{bookingId}")]
        public async Task<IActionResult> GetById(
            int bookingId)
        {
            var result =
                await _service.GetByIdAsync(bookingId);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> Update(
            BookingPut model)
        {
            var result = await _service.UpdateAsync(model);

            return Ok(result);
        }

        [HttpDelete("{bookingId}")]
        public async Task<IActionResult> Delete(
            int bookingId)
        {
            var result =
                await _service.DeleteAsync(bookingId);

            return Ok(result);
        }
       
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserId(
            int userId,
            int pageNumber = 1,
            int pageSize = 10,
            string? search = null)
        {
            var result = await _service.GetByUserIdAsync(
                userId,
                pageNumber,
                pageSize,
                search);

            return Ok(result);
        }
        // BookingController.cs

        [HttpGet("client/{clientId}")]
        public async Task<IActionResult> GetByClientId(
            int clientId,
            int pageNumber = 1,
            int pageSize = 10,
            string? search = null)
        {
            var result = await _service.GetByClientIdAsync(
                clientId,
                pageNumber,
                pageSize,
                search);

            return Ok(result);
        }

        // GET:
        // api/Booking/service/5

        [HttpGet("service/{serviceId}")]
        public async Task<IActionResult>
        GetBookingModal(
            int serviceId
        )
        {
            var result =
    await _service
        .GetBookingModal(
            serviceId
        );

            if (result == null)
            {
                return NotFound(
                    "Service not found."
                );
            }

            return Ok(result);
        }
    }
}