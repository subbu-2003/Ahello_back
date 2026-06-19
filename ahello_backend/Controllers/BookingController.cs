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
        public async Task<IActionResult> Create([FromBody] BookingPost model)
        {
            try
            {
                var id = await _service.CreateAsync(model);

                return Ok(new
                {
                    Message = "Booking created successfully.",
                    BookingId = id
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Message = ex.Message
                });
            }
        }

        [HttpPost("{id}/reschedule")]
        public async Task<IActionResult> Reschedule(
            int id,
            [FromBody] RescheduleRequest req)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { Message = "BookingId is required." });

                if (req == null)
                    return BadRequest(new { Message = "Request body is required." });

                if (req.NewDate == default)
                    return BadRequest(new { Message = "NewDate is required." });

                if (req.NewStartTime == default || req.NewEndTime == default)
                    return BadRequest(new { Message = "NewStartTime and NewEndTime are required." });

                if (req.NewEndTime <= req.NewStartTime)
                    return BadRequest(new { Message = "NewEndTime must be greater than NewStartTime." });

                if (req.SlotId <= 0)
                    return BadRequest(new { Message = "SlotId is required." });

                if (req.RescheduledBy <= 0)
                    return BadRequest(new { Message = "RescheduledBy is required." });

                var newBookingId = await _service.RescheduleAsync(
                    oldBookingId: id,
                    newDate: req.NewDate,
                    newStart: req.NewStartTime,
                    newEnd: req.NewEndTime,
                    slotId: req.SlotId,
                    rescheduledBy: req.RescheduledBy.ToString(),
                    reason: "Manual");

                return Ok(new
                {
                    Message = "Booking rescheduled successfully.",
                    OldBookingId = id,
                    NewBookingId = newBookingId
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Message = ex.Message
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _service.GetAllAsync();

                return Ok(new
                {
                    Message = "Bookings fetched successfully.",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Message = ex.Message
                });
            }
        }

        [HttpGet("{bookingId}")]
        public async Task<IActionResult> GetById(int bookingId)
        {
            try
            {
                if (bookingId <= 0)
                    return BadRequest(new { Message = "BookingId is required." });

                var result = await _service.GetByIdAsync(bookingId);

                if (result == null)
                {
                    return NotFound(new
                    {
                        Message = "Booking not found."
                    });
                }

                return Ok(new
                {
                    Message = "Booking fetched successfully.",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Message = ex.Message
                });
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] BookingPut model)
        {
            try
            {
                if (model == null)
                    return BadRequest(new { Message = "Request body is required." });

                var result = await _service.UpdateAsync(model);

                return Ok(new
                {
                    Message = result
                        ? "Booking updated successfully."
                        : "Booking update failed.",
                    Success = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Message = ex.Message
                });
            }
        }

        [HttpDelete("{bookingId}")]
        public async Task<IActionResult> Delete(int bookingId)
        {
            try
            {
                if (bookingId <= 0)
                    return BadRequest(new { Message = "BookingId is required." });

                var result = await _service.DeleteAsync(bookingId);

                return Ok(new
                {
                    Message = result
                        ? "Booking deleted successfully."
                        : "Booking delete failed.",
                    Success = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Message = ex.Message
                });
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserId(
            int userId,
            int pageNumber = 1,
            int pageSize = 10,
            string? search = null, string? status = null,
            DateTime? scheduleDate = null)
        {
            try
            {
                if (userId <= 0)
                    return BadRequest(new { Message = "UserId is required." });

                if (pageNumber <= 0)
                    return BadRequest(new { Message = "PageNumber must be greater than 0." });

                if (pageSize <= 0)
                    return BadRequest(new { Message = "PageSize must be greater than 0." });

                var result = await _service.GetByUserIdAsync(
                    userId,
                    pageNumber,
                    pageSize,
                    search, status,
                    scheduleDate);

                return Ok(new
                {
                    Message = "User bookings fetched successfully.",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Message = ex.Message
                });
            }
        }

        [HttpGet("client/{clientId}")]
        public async Task<IActionResult> GetByClientId(
            int clientId,
            int pageNumber = 1,
            int pageSize = 10,
            string? search = null, string? status = null,
            DateTime? scheduleDate = null)
        {
            try
            {
                if (clientId <= 0)
                    return BadRequest(new { Message = "ClientId is required." });

                if (pageNumber <= 0)
                    return BadRequest(new { Message = "PageNumber must be greater than 0." });

                if (pageSize <= 0)
                    return BadRequest(new { Message = "PageSize must be greater than 0." });

                var result = await _service.GetByClientIdAsync(
                    clientId,
                    pageNumber,
                    pageSize,
                    search, status,
                    scheduleDate);

                return Ok(new
                {
                    Message = "Client bookings fetched successfully.",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Message = ex.Message
                });
            }
        }

        [HttpGet("service/{serviceId}")]
        public async Task<IActionResult> GetBookingModal(int serviceId)
        {
            try
            {
                if (serviceId <= 0)
                    return BadRequest(new { Message = "ServiceId is required." });

                var result = await _service.GetBookingModal(serviceId);

                if (result == null)
                {
                    return NotFound(new
                    {
                        Message = "Service not found."
                    });
                }

                return Ok(new
                {
                    Message = "Booking modal fetched successfully.",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Message = ex.Message
                });
            }
        }
        [HttpGet("clients-service-wise")]
        public async Task<IActionResult> GetClientsServiceWise(
    int userId,
    int pageNumber = 1,
    int pageSize = 10,
    string? search = null,
    string? bookingStatus = null,
    DateTime? lastBookingDate = null)
        {
            try
            {
                var data = await _service.GetClientsServiceWiseAsync(
                    userId,
                    pageNumber,
                    pageSize,
                    search,
                    bookingStatus,
                    lastBookingDate);

                return Ok(new
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalServices = data.Count,
                    Data = data
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}