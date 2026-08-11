using ahello_backend.Models.Bookings;
using ahello_backend.Models.Reschedulerequest;
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
                if (model.UserId <= 0)
                {
                    return BadRequest(new
                    {
                        Message = "UserId is required."
                    });
                }

                if (model.ClientId <= 0)
                {
                    return BadRequest(new
                    {
                        Message = "ClientId is required."
                    });
                }

                if (model.ServiceId <= 0)
                {
                    return BadRequest(new
                    {
                        Message = "ServiceId is required."
                    });
                }

                if (model.SlotId <= 0)
                {
                    return BadRequest(new
                    {
                        Message = "SlotId is required."
                    });
                }

                if (model.ScheduleDate == default)
                {
                    return BadRequest(new
                    {
                        Message = "ScheduleDate is required."
                    });
                }

                if (model.StartTime == default)
                {
                    return BadRequest(new
                    {
                        Message = "StartTime is required."
                    });
                }

                if (model.EndTime == default)
                {
                    return BadRequest(new
                    {
                        Message = "EndTime is required."
                    });
                }

                if (string.IsNullOrWhiteSpace(model.Status))
                {
                    return BadRequest(new
                    {
                        Message = "Status is required."
                    });
                }

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
                if (model == null)
                {
                    return BadRequest(new
                    {
                        Message = "Request body is required."
                    });
                }

                if (model.BookingId <= 0)
                {
                    return BadRequest(new
                    {
                        Message = "BookingId is required."
                    });
                }

                if (model.UserId <= 0)
                {
                    return BadRequest(new
                    {
                        Message = "UserId is required."
                    });
                }

                if (model.ClientId <= 0)
                {
                    return BadRequest(new
                    {
                        Message = "ClientId is required."
                    });
                }

                if (model.ServiceId <= 0)
                {
                    return BadRequest(new
                    {
                        Message = "ServiceId is required."
                    });
                }

                if (model.SlotId <= 0)
                {
                    return BadRequest(new
                    {
                        Message = "SlotId is required."
                    });
                }
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
        [HttpPost("reschedule-request")]
        public async Task<IActionResult> CreateRescheduleRequest(
    [FromBody] RescheduleRequestPost model)
        {
            try
            {
                if (model == null)
                {
                    return BadRequest(new
                    {
                        Message = "Request body is required."
                    });
                }

                if (model.BookingId <= 0)
                {
                    return BadRequest(new
                    {
                        Message = "BookingId is required."
                    });
                }

                if (model.SlotId <= 0)
                {
                    return BadRequest(new
                    {
                        Message = "SlotId is required."
                    });
                }

                if (model.RequestedDate == default)
                {
                    return BadRequest(new
                    {
                        Message = "RequestedDate is required."
                    });
                }

                if (model.RequestedStartTime == default)
                {
                    return BadRequest(new
                    {
                        Message = "RequestedStartTime is required."
                    });
                }

                if (model.RequestedEndTime == default)
                {
                    return BadRequest(new
                    {
                        Message = "RequestedEndTime is required."
                    });
                }

                if (model.RequestedEndTime <= model.RequestedStartTime)
                {
                    return BadRequest(new
                    {
                        Message = "RequestedEndTime must be greater than RequestedStartTime."
                    });
                }

                var requestId =
                    await _service.CreateRescheduleRequestAsync(model);

                return Ok(new
                {
                    Message = "Reschedule request sent to the host successfully.",
                    RequestId = requestId
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
        [HttpGet("reschedule-requests/user/{userId}")]
        public async Task<IActionResult> GetRescheduleRequestsByUserId(
        int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    return BadRequest(new
                    {
                        Message = "UserId is required."
                    });
                }

                var data =
                    await _service.GetRescheduleRequestsByUserIdAsync(userId);

                return Ok(new
                {
                    Message = "Reschedule requests fetched successfully.",
                    Data = data
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
        [HttpGet("reschedule-request/{requestId}")]
        public async Task<IActionResult> GetRescheduleRequestById(
        int requestId)
        {
            try
            {
                if (requestId <= 0)
                {
                    return BadRequest(new
                    {
                        Message = "RequestId is required."
                    });
                }

                var data =
                    await _service.GetRescheduleRequestByIdAsync(requestId);

                if (data == null)
                {
                    return NotFound(new
                    {
                        Message = "Reschedule request not found."
                    });
                }

                return Ok(new
                {
                    Message = "Reschedule request fetched successfully.",
                    Data = data
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
        [HttpPut("reschedule-request/{requestId}/status")]
        public async Task<IActionResult> UpdateRescheduleRequestStatus(
        int requestId,
        [FromBody] RescheduleRequestStatusPut model)
            {
            try
            {
                if (requestId <= 0)
                {
                    return BadRequest(new
                    {
                        Message = "RequestId is required."
                    });
                }

                if (model == null)
                {
                    return BadRequest(new
                    {
                        Message = "Request body is required."
                    });
                }

                if (string.IsNullOrWhiteSpace(model.Status))
                {
                    return BadRequest(new
                    {
                        Message = "Status is required."
                    });
                }

                if (model.Status != "Accepted" &&
                    model.Status != "Rejected")
                {
                    return BadRequest(new
                    {
                        Message = "Status must be Accepted or Rejected."
                    });
                }

                var result =
                    await _service.UpdateRescheduleRequestStatusAsync(
                        requestId,
                        model.Status,
                        model.ModifiedBy);

                if (!result)
                {
                    return BadRequest(new
                    {
                        Message = "Unable to update reschedule request."
                    });
                }

                return Ok(new
                {
                    Message = model.Status == "Accepted"
                        ? "Reschedule request accepted successfully."
                        : "Reschedule request rejected successfully.",
                    Success = true
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
    }
}